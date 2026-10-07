<?php
declare(strict_types=1);

require_once __DIR__ . '/bootstrap.php';

const MAX_REQUEST_BYTES = 1048576;

function respond(int $status, array $body)
{
    http_response_code($status);
    header('Content-Type: application/json; charset=utf-8');
    header('Cache-Control: no-store');
    header('X-SSW-API-Version: 1');
    $encoded = json_encode($body, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
    if ($encoded === false) {
        $encoded = '{"error":"internal_error","message":"The technical selection service is temporarily unavailable."}';
    }
    echo $encoded;
    exit;
}

function request_header(string $name): string
{
    $serverName = 'HTTP_' . strtoupper(str_replace('-', '_', $name));
    $value = $_SERVER[$serverName] ?? ($_SERVER['REDIRECT_' . $serverName] ?? '');
    if ($value === '' && function_exists('getallheaders')) {
        foreach (getallheaders() as $headerName => $headerValue) {
            if (strcasecmp($headerName, $name) === 0) {
                $value = $headerValue;
                break;
            }
        }
    }
    return trim((string) $value);
}

function bearer_token(): string
{
    $authorization = request_header('Authorization');
    return preg_match('/^Bearer\s+(.+)$/i', $authorization, $matches) ? trim($matches[1]) : '';
}

function request_json(): array
{
    $length = (int) ($_SERVER['CONTENT_LENGTH'] ?? 0);
    if ($length > MAX_REQUEST_BYTES) {
        throw new ApiException(413, 'payload_too_large', 'The request exceeds the 1 MiB limit.');
    }
    $raw = file_get_contents('php://input', false, null, 0, MAX_REQUEST_BYTES + 1);
    if ($raw === false || strlen($raw) > MAX_REQUEST_BYTES) {
        throw new ApiException(413, 'payload_too_large', 'The request exceeds the 1 MiB limit.');
    }
    $decoded = json_decode($raw, true, 128);
    if (json_last_error() !== JSON_ERROR_NONE) {
        throw new ApiException(400, 'invalid_json', 'The request body is not valid JSON.');
    }
    if (!is_array($decoded) || selection_array_is_list($decoded)) {
        throw new ApiException(400, 'invalid_json', 'The request body must be a JSON object.');
    }
    return $decoded;
}

function selection_array_is_list(array $value): bool
{
    $expected = 0;
    foreach ($value as $key => $_) {
        if ($key !== $expected++) {
            return false;
        }
    }
    return true;
}

function route_path(): string
{
    $path = parse_url($_SERVER['REQUEST_URI'] ?? '/', PHP_URL_PATH) ?: '/';
    $marker = '/api/v1';
    $position = strpos($path, $marker);
    if ($position !== false) {
        $path = substr($path, $position + strlen($marker));
    }
    return '/' . trim($path, '/');
}

try {
    $method = strtoupper((string) ($_SERVER['REQUEST_METHOD'] ?? ''));
    $database = selection_database();
    $service = new TechnicalSelectionService($database);
    $path = route_path();

    if ($path === '/installations/register') {
        if ($method !== 'POST') {
            throw new ApiException(405, 'method_not_allowed', 'This operation requires POST.');
        }
        $input = request_json();
        $registrationIp = selection_client_ip() ?? ($_SERVER['REMOTE_ADDR'] ?? 'unknown');
        $rateSubject = (selection_env('SSW_SELECTION_RATE_SECRET') ?? '') . '|register|'
            . $registrationIp . '|' . ($input['customer_code'] ?? 'unknown');
        $service->enforceRateLimit($rateSubject, 'installation.register', 10);
        $body = $service->registerInstallation(
            $input,
            request_header('X-SSW-Bootstrap-Key'),
            selection_bootstrap_keys(),
            selection_public_enrollment_customer()
        );
        respond(201, $body);
    }

    if ($path === '/license/activate') {
        if ($method !== 'POST') throw new ApiException(405, 'method_not_allowed', 'This operation requires POST.');
        $input = request_json();
        $registrationIp = selection_client_ip() ?? ($_SERVER['REMOTE_ADDR'] ?? 'unknown');
        $service->enforceRateLimit((selection_env('SSW_SELECTION_RATE_SECRET') ?? '') . '|license|' . $registrationIp . '|' . strtolower((string) ($input['email'] ?? 'unknown')), 'license.activate', 5);
        respond(201, $service->activateLicense($input));
    }

    if ($path === '/license/request') {
        if ($method !== 'POST') throw new ApiException(405, 'method_not_allowed', 'This operation requires POST.');
        $input = request_json();
        $registrationIp = selection_client_ip() ?? ($_SERVER['REMOTE_ADDR'] ?? 'unknown');
        $service->enforceRateLimit((selection_env('SSW_SELECTION_RATE_SECRET') ?? '') . '|license-request|' . $registrationIp . '|' . strtolower((string) ($input['email'] ?? 'unknown')), 'license.request', 5);
        respond(201, $service->requestLicenseActivation($input));
    }

    $principal = $service->authenticate(bearer_token());
    $service->enforceRateLimit((selection_env('SSW_SELECTION_RATE_SECRET') ?? '') . '|installation|' . $principal['installation_id'], 'authenticated', 60);
    $geolocation = selection_geolocation($database);

    if ($path === '/follow-ups' && $method === 'GET') {
        respond(200, $service->listFollowUps($principal, $_GET));
    }

    if ($method !== 'POST') {
        throw new ApiException(405, 'method_not_allowed', 'This operation requires POST.');
    }
    $input = request_json();

    if ($path === '/installations/token/renew') {
        respond(200, $service->renewToken($principal));
    }

    if ($path === '/license/claim-legacy') {
        respond(200, $service->claimLegacyLicense($principal, $input));
    }

    if ($path === '/license/check') {
        respond(200, $service->checkLicense($principal, $input));
    }

    $idempotencyKey = request_header('Idempotency-Key');
    if ($path === '/selections') {
        $result = $service->executeIdempotent($principal, 'selection.create', $idempotencyKey, $input,
            function () use ($service, $principal, $input, $geolocation) {
                return $service->createSelection($principal, $input, $geolocation);
            });
        respond($result['status'], $result['body']);
    }

    if (preg_match('#^/selections/([0-9]{16})/revisions$#D', $path, $matches)) {
        $operation = 'selection.revise.' . $matches[1];
        $result = $service->executeIdempotent($principal, $operation, $idempotencyKey, $input,
            function () use ($service, $principal, $input, $matches, $geolocation) {
                return $service->createRevision($principal, $matches[1], $input, $geolocation);
            });
        respond($result['status'], $result['body']);
    }

    if (preg_match('#^/selections/([0-9]{16})/offer$#D', $path, $matches)) {
        $result = $service->executeIdempotent($principal, 'selection.offer.' . $matches[1], $idempotencyKey, $input,
            function () use ($service, $principal, $input, $matches) {
                return $service->recordOffer($principal, $matches[1], $input);
            });
        respond($result['status'], $result['body']);
    }

    if (preg_match('#^/projects/([0-9a-fA-F-]{36})$#D', $path, $matches)) {
        $projectUuid = strtolower($matches[1]);
        $operation = 'project.sync.' . $projectUuid;
        $result = $service->executeIdempotent($principal, $operation, $idempotencyKey, $input,
            function () use ($service, $principal, $projectUuid, $input) {
                return $service->syncMultiProject($principal, $projectUuid, $input);
            });
        respond($result['status'], $result['body']);
    }

    if ($path === '/follow-ups') {
        $result = $service->executeIdempotent($principal, 'follow-up.create', $idempotencyKey, $input,
            function () use ($service, $principal, $input) {
                return $service->createFollowUp($principal, $input);
            });
        respond($result['status'], $result['body']);
    }

    if (preg_match('#^/follow-ups/([0-9a-fA-F-]{36})/reschedule$#D', $path, $matches)) {
        $reminderUuid = strtolower($matches[1]);
        $operation = 'follow-up.reschedule.' . $reminderUuid;
        $result = $service->executeIdempotent($principal, $operation, $idempotencyKey, $input,
            function () use ($service, $principal, $reminderUuid, $input) {
                return $service->rescheduleFollowUp($principal, $reminderUuid, $input);
            });
        respond($result['status'], $result['body']);
    }

    if (preg_match('#^/follow-ups/([0-9a-fA-F-]{36})/close$#D', $path, $matches)) {
        $reminderUuid = strtolower($matches[1]);
        $operation = 'follow-up.close.' . $reminderUuid;
        $result = $service->executeIdempotent($principal, $operation, $idempotencyKey, $input,
            function () use ($service, $principal, $reminderUuid, $input) {
                return $service->closeFollowUp($principal, $reminderUuid, $input);
            });
        respond($result['status'], $result['body']);
    }

    throw new ApiException(404, 'route_not_found', 'The requested API operation does not exist.');
} catch (ApiException $exception) {
    respond($exception->status, ['error' => $exception->errorCode, 'message' => $exception->getMessage()]);
} catch (Throwable $exception) {
    error_log(json_encode([
        'timestamp' => gmdate('c'),
        'event' => 'technical_selection_api_error',
        'type' => get_class($exception),
        'message' => $exception->getMessage(),
    ], JSON_UNESCAPED_SLASHES));
    respond(500, ['error' => 'internal_error', 'message' => 'The technical selection service is temporarily unavailable.']);
}
