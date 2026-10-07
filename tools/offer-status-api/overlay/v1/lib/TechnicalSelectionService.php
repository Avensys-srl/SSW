<?php
declare(strict_types=1);

require_once __DIR__ . '/ApiException.php';
require_once __DIR__ . '/PublicReference.php';

final class TechnicalSelectionService
{
    private const TOKEN_LIFETIME_DAYS = 180;
    private const LICENSE_LIFETIME_DAYS = 30;
    private const IDEMPOTENCY_LIFETIME_DAYS = 30;
    private $db;

    public function __construct(PDO $db)
    {
        $this->db = $db;
        $this->db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $this->db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
    }

    public function registerInstallation(
        array $input,
        string $bootstrapKey,
        array $bootstrapKeys,
        ?string $publicEnrollmentCustomer = null
    ): array
    {
        $customerCode = $this->requiredString($input, 'customer_code', 2, 32);
        $installationUuid = strtolower($this->requiredUuid($input, 'installation_id'));
        $installationCode = strtoupper($this->requiredPattern($input, 'installation_code', '/^[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{4}$/D'));
        $configured = $bootstrapKeys[$customerCode] ?? null;
        $publicEnrollment = is_string($publicEnrollmentCustomer)
            && $publicEnrollmentCustomer !== ''
            && hash_equals($publicEnrollmentCustomer, $customerCode);
        if (!$publicEnrollment
            && (!is_string($configured) || $configured === '' || !hash_equals($configured, $bootstrapKey))) {
            throw new ApiException(401, 'invalid_bootstrap_credentials', 'Installation registration is not authorized.');
        }

        $customer = $this->fetchOne('SELECT id, status FROM ssw_customers WHERE customer_code = ?', [$customerCode]);
        if ($customer === null || $customer['status'] !== 'active') {
            throw new ApiException(403, 'customer_disabled', 'The customer is not enabled for technical selections.');
        }

        $this->db->beginTransaction();
        try {
            $installation = $this->fetchOne('SELECT id, customer_id, status FROM ssw_installations WHERE installation_uuid = ?', [$installationUuid]);
            if ($installation !== null && (int) $installation['customer_id'] !== (int) $customer['id']) {
                throw new ApiException(409, 'installation_customer_mismatch', 'The installation is already assigned to another customer.');
            }
            if ($installation !== null && $installation['status'] !== 'active') {
                throw new ApiException(403, 'installation_revoked', 'The installation has been revoked.');
            }

            $version = $this->optionalString($input, 'software_version', 32);
            $dbSchema = $this->optionalPositiveInt($input, 'database_schema_version');
            $dbHash = $this->optionalString($input, 'database_content_hash', 128);
            $apiVersion = $this->optionalPositiveInt($input, 'api_contract_version');
            if ($installation === null) {
                try {
                    $statement = $this->db->prepare('INSERT INTO ssw_installations '
                        . '(customer_id, installation_uuid, installation_code, software_version, database_schema_version, database_content_hash, api_contract_version) '
                        . 'VALUES (?, ?, ?, ?, ?, ?, ?)');
                    $statement->execute([(int) $customer['id'], $installationUuid, $installationCode, $version, $dbSchema, $dbHash, $apiVersion]);
                    $installationId = (int) $this->db->lastInsertId();
                    $event = 'installation.registered';
                } catch (PDOException $exception) {
                    if (!$this->isUniqueViolation($exception)) throw $exception;
                    $currentRead = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
                    $concurrent = $this->fetchOne('SELECT id, customer_id, status FROM ssw_installations WHERE installation_uuid = ?' . $currentRead, [$installationUuid]);
                    if ($concurrent === null || (int) $concurrent['customer_id'] !== (int) $customer['id']) throw $exception;
                    if ($concurrent['status'] !== 'active') throw new ApiException(403, 'installation_revoked', 'The installation has been revoked.');
                    $installationId = (int) $concurrent['id'];
                    $event = 'installation.reregistered';
                }
            } else {
                $installationId = (int) $installation['id'];
                $statement = $this->db->prepare('UPDATE ssw_installations SET installation_code = ?, software_version = ?, '
                    . 'database_schema_version = ?, database_content_hash = ?, api_contract_version = ?, last_seen_at = CURRENT_TIMESTAMP '
                    . 'WHERE id = ?');
                $statement->execute([$installationCode, $version, $dbSchema, $dbHash, $apiVersion, $installationId]);
                $event = 'installation.reregistered';
            }

            $token = $this->issueToken($installationId);
            $this->audit((int) $customer['id'], $installationId, null, $event, ['api_contract_version' => $apiVersion]);
            $this->db->commit();
            return [
                'installation_id' => $installationUuid,
                'access_token' => $token['raw'],
                'token_type' => 'Bearer',
                'expires_at' => $token['expires_at'],
                'api_contract_version' => 1,
            ];
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) {
                $this->db->rollBack();
            }
            throw $exception;
        }
    }

    public function authenticate(string $token): array
    {
        if ($token === '') {
            throw new ApiException(401, 'missing_token', 'A bearer token is required.');
        }
        $row = $this->fetchOne(
            'SELECT i.id AS installation_id, i.customer_id, i.status AS installation_status, c.status AS customer_status, u.status AS user_status '
            . 'FROM ssw_installation_tokens t JOIN ssw_installations i ON i.id = t.installation_id '
            . 'JOIN ssw_customers c ON c.id = i.customer_id '
            . 'LEFT JOIN ssw_users u ON u.id = i.user_id '
            . 'WHERE t.token_hash = ? AND t.revoked_at IS NULL AND t.expires_at > CURRENT_TIMESTAMP',
            [hash('sha256', $token)]
        );
        if ($row === null) {
            throw new ApiException(401, 'invalid_token', 'The installation token is invalid or expired.');
        }
        if ($row['installation_status'] !== 'active' || $row['customer_status'] !== 'active') {
            throw new ApiException(403, 'installation_revoked', 'The installation is not active.');
        }
        if ($row['user_status'] !== null && $row['user_status'] !== 'active') {
            throw new ApiException(403, 'license_revoked', 'The SSW license is not active.');
        }
        $this->db->prepare('UPDATE ssw_installations SET last_seen_at = CURRENT_TIMESTAMP WHERE id = ?')
            ->execute([(int) $row['installation_id']]);
        return ['installation_id' => (int) $row['installation_id'], 'customer_id' => (int) $row['customer_id']];
    }

    public function renewToken(array $principal): array
    {
        $this->db->beginTransaction();
        try {
            $this->db->prepare('UPDATE ssw_installation_tokens SET revoked_at = CURRENT_TIMESTAMP '
                . 'WHERE installation_id = ? AND revoked_at IS NULL')->execute([$principal['installation_id']]);
            $token = $this->issueToken($principal['installation_id']);
            $this->audit($principal['customer_id'], $principal['installation_id'], null, 'installation.token_renewed', []);
            $this->db->commit();
            return ['access_token' => $token['raw'], 'token_type' => 'Bearer', 'expires_at' => $token['expires_at']];
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function activateLicense(array $input): array
    {
        $email = $this->requiredEmail($input, 'email');
        $pin = $this->requiredPattern($input, 'activation_code', '/^[0-9]{6}$/D');
        $installationUuid = strtolower($this->requiredUuid($input, 'installation_id'));
        $installationCode = strtoupper($this->requiredPattern($input, 'installation_code', '/^[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{4}$/D'));
        $this->db->beginTransaction();
        try {
            $lock = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
            $user = $this->fetchOne('SELECT * FROM ssw_users WHERE email = ?' . $lock, [$email]);
            if ($user === null || $user['status'] === 'revoked') {
                throw new ApiException(403, 'activation_denied', 'Activation details are not valid.');
            }
            $code = $this->fetchOne('SELECT * FROM ssw_activation_codes WHERE user_id = ? AND consumed_at IS NULL '
                . 'AND expires_at > CURRENT_TIMESTAMP ORDER BY id DESC LIMIT 1' . $lock, [(int) $user['id']]);
            if ($code === null || (int) $code['attempts'] >= 5 || !password_verify($pin, (string) $code['code_hash'])) {
                if ($code !== null) $this->db->prepare('UPDATE ssw_activation_codes SET attempts = attempts + 1 WHERE id = ?')->execute([(int) $code['id']]);
                $this->db->commit();
                throw new ApiException(403, 'activation_denied', 'Activation details are not valid.');
            }
            $installation = $this->fetchOne('SELECT * FROM ssw_installations WHERE installation_uuid = ?' . $lock, [$installationUuid]);
            if ($installation !== null && $installation['user_id'] !== null && (int) $installation['user_id'] !== (int) $user['id']) {
                $previousOwner = $this->fetchOne('SELECT status FROM ssw_users WHERE id = ?', [(int) $installation['user_id']]);
                $canReassignDeletedInstallation = $installation['archived_at'] !== null
                    || ($previousOwner !== null && $previousOwner['status'] === 'deleted');
                if (!$canReassignDeletedInstallation) {
                    throw new ApiException(409, 'installation_already_assigned', 'This installation is already assigned.');
                }
            }
            $deviceNumber = $this->nextDeviceNumber((int) $user['id'], $installation === null ? null : (int) $installation['id']);
            $firstName = $this->optionalString($input, 'first_name', 100) ?? (string) $user['first_name'];
            $lastName = $this->optionalString($input, 'last_name', 100) ?? (string) $user['last_name'];
            $this->db->prepare("UPDATE ssw_users SET first_name = ?, last_name = ?, status = 'active', activated_at = COALESCE(activated_at, CURRENT_TIMESTAMP), revoked_at = NULL WHERE id = ?")
                ->execute([$firstName, $lastName, (int) $user['id']]);
            $archiveParameters = [(int) $user['id'], $installationCode];
            $archiveExclusion = '';
            if ($installation !== null) {
                $archiveExclusion = ' AND id <> ?';
                $archiveParameters[] = (int) $installation['id'];
            }
            $this->db->prepare("UPDATE ssw_installations SET archived_at = CURRENT_TIMESTAMP "
                . "WHERE user_id = ? AND installation_code = ? AND status = 'revoked' AND archived_at IS NULL" . $archiveExclusion)
                ->execute($archiveParameters);
            if ($installation === null) {
                $statement = $this->db->prepare('INSERT INTO ssw_installations (customer_id, user_id, installation_uuid, installation_code, device_number, status, software_version, database_schema_version, database_content_hash, api_contract_version) VALUES (?, ?, ?, ?, ?, \'active\', ?, ?, ?, ?)');
                $statement->execute([(int) $user['customer_id'], (int) $user['id'], $installationUuid, $installationCode, $deviceNumber,
                    $this->optionalString($input, 'software_version', 32), $this->optionalPositiveInt($input, 'database_schema_version'),
                    $this->optionalString($input, 'database_content_hash', 128), $this->optionalPositiveInt($input, 'api_contract_version')]);
                $installationId = (int) $this->db->lastInsertId();
            } else {
                $installationId = (int) $installation['id'];
                $this->db->prepare("UPDATE ssw_installations SET user_id = ?, device_number = ?, installation_code = ?, status = 'active', revoked_at = NULL, archived_at = NULL WHERE id = ?")
                    ->execute([(int) $user['id'], $deviceNumber, $installationCode, $installationId]);
            }
            $this->db->prepare('UPDATE ssw_activation_codes SET consumed_at = CURRENT_TIMESTAMP, display_code = NULL WHERE id = ?')->execute([(int) $code['id']]);
            $token = $this->issueToken($installationId);
            $lease = $this->renewLicenseLease((int) $user['id'], $installationId, $deviceNumber, 'license.activated');
            $this->db->commit();
            return array_merge($lease, ['access_token' => $token['raw'], 'token_expires_at_utc' => $token['expires_at']]);
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function requestLicenseActivation(array $input): array
    {
        $email = $this->requiredEmail($input, 'email');
        $firstName = $this->requiredString($input, 'first_name', 1, 100);
        $lastName = $this->requiredString($input, 'last_name', 1, 100);
        $companyName = $this->requiredString($input, 'company_name', 2, 160);
        $installationUuid = strtolower($this->requiredUuid($input, 'installation_id'));
        $installationCode = strtoupper($this->requiredPattern($input, 'installation_code', '/^[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{4}$/D'));
        $user = $this->fetchOne('SELECT id,company_name,status FROM ssw_users WHERE email = ?', [$email]);
        $data = [
            'first_name' => $firstName, 'last_name' => $lastName, 'email' => $email,
            'company_name' => $companyName, 'installation_id' => $installationUuid,
            'installation_code' => $installationCode,
            'software_version' => $this->optionalString($input, 'software_version', 32),
            'requested_at_utc' => gmdate('c'),
        ];
        $this->db->prepare('INSERT INTO ssw_license_events (user_id, installation_id, event_type, event_data) VALUES (?, NULL, ?, ?)')
            ->execute([$user === null ? null : (int) $user['id'], 'license.activation_requested', json_encode($data, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE)]);
        LicensePushNotifier::send($this->db, $data);
        return ['status' => 'REQUESTED', 'email' => $email];
    }

    private function companyNamesMatch(string $expected, string $provided): bool
    {
        $normalize = function (string $value): string {
            $value = trim(preg_replace('/\s+/', ' ', $value) ?? '');
            return function_exists('mb_strtolower') ? mb_strtolower($value, 'UTF-8') : strtolower($value);
        };
        return hash_equals($normalize($expected), $normalize($provided));
    }

    public function claimLegacyLicense(array $principal, array $input): array
    {
        $email = $this->requiredEmail($input, 'email');
        $firstName = $this->requiredString($input, 'first_name', 1, 100);
        $lastName = $this->requiredString($input, 'last_name', 1, 100);
        $companyName = $this->requiredString($input, 'company_name', 2, 160);
        $this->db->beginTransaction();
        try {
            $lock = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
            $installation = $this->fetchOne('SELECT * FROM ssw_installations WHERE id = ?' . $lock, [$principal['installation_id']]);
            if ($installation === null || $installation['status'] !== 'active') throw new ApiException(403, 'installation_revoked', 'The installation is not active.');
            if ((int) $installation['legacy_license_eligible'] !== 1 || $installation['user_id'] !== null) {
                throw new ApiException(403, 'activation_code_required', 'This installation requires an activation code.');
            }
            $user = $this->fetchOne('SELECT * FROM ssw_users WHERE email = ?' . $lock, [$email]);
            if ($user === null) {
                $statement = $this->db->prepare("INSERT INTO ssw_users (customer_id, email, company_name, first_name, last_name, status, activated_at) VALUES (?, ?, ?, ?, ?, 'active', CURRENT_TIMESTAMP)");
                $statement->execute([$principal['customer_id'], $email, $companyName, $firstName, $lastName]);
                $userId = (int) $this->db->lastInsertId();
            } else {
                if ((int) $user['customer_id'] !== (int) $principal['customer_id'] || $user['status'] === 'revoked') throw new ApiException(409, 'user_assignment_conflict', 'The user cannot be assigned automatically.');
                $userId = (int) $user['id'];
                $this->db->prepare('UPDATE ssw_users SET first_name = ?, last_name = ?, company_name = ? WHERE id = ?')->execute([$firstName, $lastName, $companyName, $userId]);
            }
            $deviceNumber = $this->nextDeviceNumber($userId, (int) $installation['id']);
            $this->db->prepare('UPDATE ssw_installations SET user_id = ?, device_number = ?, legacy_license_eligible = 0 WHERE id = ?')->execute([$userId, $deviceNumber, (int) $installation['id']]);
            $lease = $this->renewLicenseLease($userId, (int) $installation['id'], $deviceNumber, 'license.legacy_claimed');
            $this->db->commit();
            return $lease;
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function checkLicense(array $principal, array $input): array
    {
        $row = $this->fetchOne('SELECT i.id, i.user_id, i.device_number, i.status AS installation_status, u.status AS user_status '
            . 'FROM ssw_installations i LEFT JOIN ssw_users u ON u.id = i.user_id WHERE i.id = ?', [$principal['installation_id']]);
        if ($row === null || $row['installation_status'] !== 'active' || $row['user_id'] === null) throw new ApiException(403, 'license_inactive', 'The SSW license is not active.');
        if ($row['user_status'] !== 'active') throw new ApiException(403, 'license_revoked', 'The SSW license is not active.');
        $softwareVersion = $this->optionalString($input, 'software_version', 32);
        if ($softwareVersion !== null && preg_match('/^\d+\.\d+\.\d+\.\d+$/D', $softwareVersion) === 1) {
            $this->db->prepare('UPDATE ssw_installations SET software_version = ? WHERE id = ?')->execute([$softwareVersion, (int) $row['id']]);
        }
        $this->recordUsage((int) $row['id'], $input);
        return $this->renewLicenseLease((int) $row['user_id'], (int) $row['id'], (int) $row['device_number'], 'license.checked');
    }

    private function nextDeviceNumber(int $userId, ?int $currentInstallationId): int
    {
        $statement = $this->db->prepare("SELECT id, device_number FROM ssw_installations WHERE user_id = ? AND status = 'active' ORDER BY device_number");
        $statement->execute([$userId]);
        $used = [];
        foreach ($statement->fetchAll() as $row) {
            if ($currentInstallationId !== null && (int) $row['id'] === $currentInstallationId) return (int) $row['device_number'];
            $used[(int) $row['device_number']] = true;
        }
        foreach ([1, 2] as $number) if (!isset($used[$number])) return $number;
        throw new ApiException(409, 'device_limit_reached', 'This user already has two active devices.');
    }

    private function renewLicenseLease(int $userId, int $installationId, int $deviceNumber, string $event): array
    {
        $validUntil = gmdate('Y-m-d H:i:s', time() + self::LICENSE_LIFETIME_DAYS * 86400);
        $this->db->prepare('UPDATE ssw_installations SET license_valid_until = ?, license_last_check_at = CURRENT_TIMESTAMP, last_seen_at = CURRENT_TIMESTAMP WHERE id = ?')->execute([$validUntil, $installationId]);
        $this->db->prepare('INSERT INTO ssw_license_events (user_id, installation_id, event_type, event_data) VALUES (?, ?, ?, ?)')
            ->execute([$userId, $installationId, $event, json_encode(['device_number' => $deviceNumber], JSON_UNESCAPED_SLASHES)]);
        return ['status' => 'ACTIVE', 'device_number' => $deviceNumber, 'server_time_utc' => gmdate('Y-m-d\TH:i:s\Z'), 'valid_until_utc' => str_replace(' ', 'T', $validUntil) . 'Z'];
    }

    private function recordUsage(int $installationId, array $input): void
    {
        $session = isset($input['session_id']) ? strtolower((string) $input['session_id']) : '';
        if (!preg_match('/^[0-9a-f-]{36}$/D', $session)) return;
        $date = gmdate('Y-m-d');
        if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql') {
            $this->db->prepare('INSERT INTO ssw_usage_daily (installation_id, usage_date, launch_count, last_session_uuid) VALUES (?, ?, 1, ?) '
                . 'ON DUPLICATE KEY UPDATE launch_count = launch_count + IF(last_session_uuid = VALUES(last_session_uuid), 0, 1), last_session_uuid = VALUES(last_session_uuid), last_seen_at = CURRENT_TIMESTAMP')
                ->execute([$installationId, $date, $session]);
        } else {
            $row = $this->fetchOne('SELECT last_session_uuid FROM ssw_usage_daily WHERE installation_id = ? AND usage_date = ?', [$installationId, $date]);
            if ($row === null) $this->db->prepare('INSERT INTO ssw_usage_daily (installation_id, usage_date, launch_count, last_session_uuid) VALUES (?, ?, 1, ?)')->execute([$installationId, $date, $session]);
            elseif ($row['last_session_uuid'] !== $session) $this->db->prepare('UPDATE ssw_usage_daily SET launch_count = launch_count + 1, last_session_uuid = ?, last_seen_at = CURRENT_TIMESTAMP WHERE installation_id = ? AND usage_date = ?')->execute([$session, $installationId, $date]);
        }
    }

    public function executeIdempotent(array $principal, string $operation, string $key, array $input, callable $action): array
    {
        if (!preg_match('/^[A-Za-z0-9._:-]{16,128}$/D', $key)) {
            throw new ApiException(400, 'invalid_idempotency_key', 'Idempotency-Key must contain 16 to 128 safe characters.');
        }
        $requestHash = hash('sha256', $this->canonicalJson($input));
        $this->db->beginTransaction();
        try {
            try {
                $statement = $this->db->prepare('INSERT INTO ssw_api_idempotency '
                    . '(installation_id, operation_name, idempotency_key, request_hash, expires_at) '
                    . 'VALUES (?, ?, ?, ?, ?)');
                $statement->execute([
                    $principal['installation_id'], $operation, $key, $requestHash,
                    gmdate('Y-m-d H:i:s', time() + self::IDEMPOTENCY_LIFETIME_DAYS * 86400),
                ]);
            } catch (PDOException $exception) {
                if (!$this->isUniqueViolation($exception)) throw $exception;
                $currentRead = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
                $existing = $this->fetchOne('SELECT request_hash, response_status, response_body FROM ssw_api_idempotency '
                    . 'WHERE installation_id = ? AND operation_name = ? AND idempotency_key = ?' . $currentRead,
                    [$principal['installation_id'], $operation, $key]);
                if ($existing === null || !hash_equals($existing['request_hash'], $requestHash)) {
                    throw new ApiException(409, 'idempotency_conflict', 'The idempotency key was already used for another request.');
                }
                if ($existing['response_status'] === null) {
                    throw new ApiException(409, 'request_in_progress', 'An identical request is still in progress.');
                }
                $this->db->commit();
                return ['status' => (int) $existing['response_status'], 'body' => $this->decodeJson($existing['response_body'])];
            }

            $result = $action();
            $body = $result['body'];
            $this->db->prepare('UPDATE ssw_api_idempotency SET response_status = ?, response_body = ? '
                . 'WHERE installation_id = ? AND operation_name = ? AND idempotency_key = ?')
                ->execute([$result['status'], $this->encodeJson($body), $principal['installation_id'], $operation, $key]);
            $this->db->commit();
            return $result;
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function createSelection(array $principal, array $input, ?array $geolocation = null): array
    {
        $projectId = strtolower($this->requiredUuid($input, 'project_id'));
        $payload = $this->requiredArray($input, 'selection');
        $versions = $this->validateVersions($this->requiredArray($input, 'versions'));
        $fingerprints = $this->validateFingerprints($this->requiredArray($input, 'fingerprints'));
        $resumeToken = $this->requiredString($input, 'resume_token', 32, 128);
        $resumeTokenHash = hash('sha256', $resumeToken);
        $payloadHash = hash('sha256', $this->canonicalJson($payload));

        $existing = $this->fetchOne('SELECT id, public_reference, latest_revision, resume_token_hash FROM ssw_selections '
            . 'WHERE customer_id = ? AND project_id = ?', [$principal['customer_id'], $projectId]);
        if ($existing !== null) {
            if (!is_string($existing['resume_token_hash'])
                || !hash_equals($existing['resume_token_hash'], $resumeTokenHash)) {
                throw new ApiException(409, 'project_already_registered', 'This project already has a central reference.');
            }
            $latest = $this->fetchOne('SELECT revision_number, snapshot_hash FROM ssw_selection_revisions '
                . 'WHERE selection_id = ? AND revision_number = ?', [(int) $existing['id'], (int) $existing['latest_revision']]);
            if ($latest === null || !is_string($latest['snapshot_hash'])
                || !hash_equals($latest['snapshot_hash'], $fingerprints['snapshot_hash'])) {
                throw new ApiException(409, 'project_state_conflict', 'The registered project has a different latest snapshot.');
            }
            $existingRevision = (int) $latest['revision_number'];
            $this->audit($principal['customer_id'], $principal['installation_id'], (int) $existing['id'],
                'selection.create_recovered', ['revision' => $existingRevision]);
            return ['status' => 200, 'body' => [
                'reference' => PublicReference::display($existing['public_reference'], $existingRevision),
                'reference_digits' => $existing['public_reference'],
                'revision' => $existingRevision,
                'resume_token' => $resumeToken,
                'snapshot_hash' => $fingerprints['snapshot_hash'],
                'change_kind' => 'Reprint',
            ]];
        }

        $selectionId = null;
        $reference = null;
        for ($attempt = 0; $attempt < 8; $attempt++) {
            $reference = PublicReference::generate();
            try {
                $statement = $this->db->prepare('INSERT INTO ssw_selections '
                    . '(customer_id, created_by_installation_id, project_id, public_reference, resume_token_hash, latest_revision) '
                    . 'VALUES (?, ?, ?, ?, ?, 1)');
                $statement->execute([$principal['customer_id'], $principal['installation_id'], $projectId, $reference, $resumeTokenHash]);
                $selectionId = (int) $this->db->lastInsertId();
                break;
            } catch (PDOException $exception) {
                if (!$this->isUniqueViolation($exception)) throw $exception;
                $currentRead = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
                $concurrent = $this->fetchOne('SELECT id FROM ssw_selections WHERE customer_id = ? AND project_id = ?' . $currentRead,
                    [$principal['customer_id'], $projectId]);
                if ($concurrent !== null) {
                    throw new ApiException(409, 'project_already_registered', 'This project already has a central reference.');
                }
            }
        }
        if ($selectionId === null || $reference === null) {
            throw new ApiException(503, 'reference_generation_failed', 'A unique public reference could not be allocated.');
        }
        $this->insertRevision($selectionId, $principal['installation_id'], 1, $payloadHash, $payload, $versions,
            $fingerprints, 'NewSelection', $geolocation);
        $this->audit($principal['customer_id'], $principal['installation_id'], $selectionId, 'selection.created', ['revision' => 1]);
        return ['status' => 201, 'body' => [
            'reference' => PublicReference::display($reference, 1),
            'reference_digits' => $reference,
            'revision' => 1,
            'resume_token' => $resumeToken,
            'payload_hash' => $payloadHash,
            'snapshot_hash' => $fingerprints['snapshot_hash'],
            'change_kind' => 'NewSelection',
        ]];
    }

    public function createRevision(array $principal, string $reference, array $input, ?array $geolocation = null): array
    {
        if (!PublicReference::isValid($reference)) {
            throw new ApiException(404, 'selection_not_found', 'The technical selection does not exist.');
        }
        $payload = $this->requiredArray($input, 'selection');
        $versions = $this->validateVersions($this->requiredArray($input, 'versions'));
        $fingerprints = $this->validateFingerprints($this->requiredArray($input, 'fingerprints'));
        $resumeToken = $this->requiredString($input, 'resume_token', 32, 128);
        $lock = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
        $selection = $this->fetchOne('SELECT id, latest_revision, created_by_installation_id, resume_token_hash '
            . 'FROM ssw_selections WHERE public_reference = ? AND customer_id = ?' . $lock,
            [$reference, $principal['customer_id']]);
        if ($selection === null) {
            throw new ApiException(404, 'selection_not_found', 'The technical selection does not exist.');
        }
        if (is_string($selection['resume_token_hash']) && $selection['resume_token_hash'] !== '') {
            if (!hash_equals($selection['resume_token_hash'], hash('sha256', $resumeToken))) {
                throw new ApiException(403, 'invalid_resume_token', 'The selection resume token is invalid.');
            }
        } elseif ((int) $selection['created_by_installation_id'] !== (int) $principal['installation_id']) {
            throw new ApiException(403, 'resume_token_required', 'This legacy selection can only be revised by its original installation.');
        }
        $latest = $this->fetchOne('SELECT revision_number, technical_input_hash, calculation_output_hash, '
            . 'calculation_basis_hash, snapshot_hash, calculation_engine_version, database_schema_version, '
            . 'database_data_version, database_content_hash, report_template_version FROM ssw_selection_revisions '
            . 'WHERE selection_id = ? AND revision_number = ?',
            [(int) $selection['id'], (int) $selection['latest_revision']]);
        if ($latest === null) {
            throw new ApiException(409, 'revision_state_invalid', 'The latest technical selection revision is unavailable.');
        }
        if (is_string($latest['snapshot_hash']) && hash_equals($latest['snapshot_hash'], $fingerprints['snapshot_hash'])) {
            $existingRevision = (int) $latest['revision_number'];
            $this->audit($principal['customer_id'], $principal['installation_id'], (int) $selection['id'],
                'selection.reprinted', ['revision' => $existingRevision]);
            return ['status' => 200, 'body' => [
                'reference' => PublicReference::display($reference, $existingRevision),
                'reference_digits' => $reference,
                'revision' => $existingRevision,
                'snapshot_hash' => $fingerprints['snapshot_hash'],
                'change_kind' => 'Reprint',
            ]];
        }
        $revision = (int) $selection['latest_revision'] + 1;
        $payloadHash = hash('sha256', $this->canonicalJson($payload));
        $changeKind = $this->classifyChange($latest, $versions, $fingerprints);
        $this->insertRevision((int) $selection['id'], $principal['installation_id'], $revision, $payloadHash, $payload,
            $versions, $fingerprints, $changeKind, $geolocation);
        $this->db->prepare('UPDATE ssw_selections SET latest_revision = ? WHERE id = ?')
            ->execute([$revision, (int) $selection['id']]);
        $this->audit($principal['customer_id'], $principal['installation_id'], (int) $selection['id'], 'selection.revised', ['revision' => $revision]);
        return ['status' => 201, 'body' => [
            'reference' => PublicReference::display($reference, $revision),
            'reference_digits' => $reference,
            'revision' => $revision,
            'payload_hash' => $payloadHash,
            'snapshot_hash' => $fingerprints['snapshot_hash'],
            'change_kind' => $changeKind,
        ]];
    }

    public function recordOffer(array $principal, string $reference, array $input): array
    {
        $status = $this->requiredString($input, 'status', 1, 20);
        if (!in_array($status, ['Provisional', 'Definitive'], true)) {
            throw new ApiException(400, 'invalid_offer_status', 'Offer status must be Provisional or Definitive.');
        }
        $revision = $this->requiredPositiveInt($input, 'revision');
        $resumeToken = $this->requiredString($input, 'resume_token', 32, 128);
        $snapshotHash = $this->requiredPattern($input, 'snapshot_hash', '/^[0-9a-f]{64}$/D');
        $lock = $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
        $selection = $this->fetchOne('SELECT id, latest_revision, resume_token_hash FROM ssw_selections '
            . 'WHERE customer_id = ? AND public_reference = ?' . $lock, [$principal['customer_id'], $reference]);
        if ($selection === null || !PublicReference::isValid($reference)) {
            throw new ApiException(404, 'selection_not_found', 'The technical selection does not exist.');
        }
        if (!is_string($selection['resume_token_hash'])
            || !hash_equals($selection['resume_token_hash'], hash('sha256', $resumeToken))) {
            throw new ApiException(403, 'invalid_resume_token', 'The selection resume token is invalid.');
        }
        $row = $this->fetchOne('SELECT id, snapshot_hash, offer_status, offer_generated_at_utc, offer_definitive_at_utc '
            . 'FROM ssw_selection_revisions WHERE selection_id = ? AND revision_number = ?' . $lock,
            [(int) $selection['id'], $revision]);
        if ($row === null || $revision !== (int) $selection['latest_revision']
            || !hash_equals((string) $row['snapshot_hash'], $snapshotHash)) {
            throw new ApiException(409, 'offer_revision_conflict', 'Only the current registered snapshot can become an offer.');
        }
        // A reprint cannot downgrade or reset the definitive timestamp.
        if ($row['offer_status'] !== 'Definitive' && $row['offer_status'] !== $status) {
            $now = $this->databaseTimestamp(new DateTimeImmutable('now', new DateTimeZone('UTC')));
            $row['offer_generated_at_utc'] = $row['offer_generated_at_utc'] ?? $now;
            $row['offer_definitive_at_utc'] = $status === 'Definitive' ? $now : null;
            $this->db->prepare('UPDATE ssw_selection_revisions SET offer_status = ?, offer_generated_at_utc = ?, '
                . 'offer_definitive_at_utc = ? WHERE id = ?')->execute([
                    $status, $row['offer_generated_at_utc'], $row['offer_definitive_at_utc'], (int) $row['id']]);
            $row['offer_status'] = $status;
            $this->audit($principal['customer_id'], $principal['installation_id'], (int) $selection['id'],
                'selection.offer_recorded', ['revision' => $revision, 'status' => $status]);
        }
        return ['status' => 200, 'body' => [
            'reference' => PublicReference::display($reference, $revision),
            'revision' => $revision,
            'status' => $row['offer_status'],
            'generated_at_utc' => $this->apiTimestamp($row['offer_generated_at_utc']),
            'definitive_at_utc' => $row['offer_definitive_at_utc'] === null ? null : $this->apiTimestamp($row['offer_definitive_at_utc']),
        ]];
    }

    public function syncMultiProject(array $principal, string $projectUuid, array $input): array
    {
        if (!preg_match('/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/D', $projectUuid)) {
            throw new ApiException(400, 'invalid_request', 'project_uuid is invalid.');
        }
        $reference = $this->requiredString($input, 'reference', 1, 255);
        $language = strtolower($this->requiredPattern($input, 'language_code', '/^[a-z]{2}(?:-[a-z]{2})?$/Di'));
        $items = $input['items'] ?? null;
        if (!is_array($items) || !$this->isList($items) || count($items) > 200) {
            throw new ApiException(400, 'invalid_request', 'items must be a list with at most 200 entries.');
        }

        $existing = $this->fetchOne('SELECT id FROM ssw_multi_projects WHERE customer_id = ? AND project_uuid = ?',
            [$principal['customer_id'], $projectUuid]);
        if ($existing === null) {
            $statement = $this->db->prepare('INSERT INTO ssw_multi_projects '
                . '(customer_id, created_by_installation_id, project_uuid, project_reference, language_code) VALUES (?, ?, ?, ?, ?)');
            $statement->execute([$principal['customer_id'], $principal['installation_id'], $projectUuid, $reference, $language]);
            $projectId = (int) $this->db->lastInsertId();
            $status = 201;
        } else {
            $projectId = (int) $existing['id'];
            $this->db->prepare('UPDATE ssw_multi_projects SET project_reference = ?, language_code = ?, '
                . 'created_by_installation_id = ?, updated_at = CURRENT_TIMESTAMP WHERE id = ?')
                ->execute([$reference, $language, $principal['installation_id'], $projectId]);
            $this->db->prepare('DELETE FROM ssw_multi_project_items WHERE multi_project_id = ?')->execute([$projectId]);
            $status = 200;
        }

        $insert = $this->db->prepare('INSERT INTO ssw_multi_project_items '
            . '(multi_project_id, selection_id, item_uuid, selection_project_uuid, position_index, selection_revision, '
            . 'customer_reference, unit_name, airflow_m3h, pressure_pa, pdf_filename, language_code, snapshot_hash) '
            . 'VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)');
        foreach ($items as $position => $item) {
            if (!is_array($item)) throw new ApiException(400, 'invalid_request', 'A project item is invalid.');
            $itemUuid = strtolower($this->requiredUuid($item, 'item_id'));
            $selectionProjectUuid = strtolower($this->requiredUuid($item, 'selection_project_id'));
            $selection = $this->fetchOne('SELECT id, latest_revision FROM ssw_selections WHERE customer_id = ? AND project_id = ?',
                [$principal['customer_id'], $selectionProjectUuid]);
            $revision = $selection === null ? null : (int) $selection['latest_revision'];
            $snapshotHash = $this->optionalString($item, 'snapshot_hash', 64);
            if ($snapshotHash !== null && !preg_match('/^[a-fA-F0-9]{64}$/D', $snapshotHash)) {
                throw new ApiException(400, 'invalid_request', 'snapshot_hash is invalid.');
            }
            $insert->execute([
                $projectId, $selection === null ? null : (int) $selection['id'], $itemUuid, $selectionProjectUuid,
                $position, $revision, $this->optionalString($item, 'customer_reference', 255),
                $this->optionalString($item, 'unit_name', 255), $item['airflow_m3h'] ?? null,
                $item['pressure_pa'] ?? null, $this->optionalString($item, 'pdf_filename', 255),
                strtolower($this->requiredPattern($item, 'language_code', '/^[a-z]{2}(?:-[a-z]{2})?$/Di')),
                $snapshotHash,
            ]);
        }
        $this->audit($principal['customer_id'], $principal['installation_id'], null, 'project.synchronized',
            ['project_uuid' => $projectUuid, 'items' => count($items)]);
        return ['status' => $status, 'body' => ['project_id' => $projectUuid, 'items' => count($items), 'synchronized' => true]];
    }

    public function createFollowUp(array $principal, array $input): array
    {
        $reminderUuid = strtolower($this->requiredUuid($input, 'reminder_id'));
        $targetType = $this->requiredPattern($input, 'target_type', '/^(Selection|Project)$/D');
        $targetUuid = strtolower($this->requiredUuid($input, 'target_id'));
        $displayReference = trim($this->requiredString($input, 'display_reference', 1, 255));
        if ($displayReference === '') {
            throw new ApiException(400, 'invalid_request', 'display_reference is invalid.');
        }
        $prepared = $this->requiredUtcTimestamp($input, 'email_prepared_at_utc');
        $due = $this->requiredUtcTimestamp($input, 'due_at_utc');
        $days = $this->requiredRangeInt($input, 'follow_up_days', 1, 90);
        $this->validateFollowUpInterval($prepared, $due, $days);

        $existing = $this->fetchOne('SELECT id FROM ssw_follow_up_reminders '
            . 'WHERE customer_id = ? AND installation_id = ? AND reminder_uuid = ?',
            [$principal['customer_id'], $principal['installation_id'], $reminderUuid]);
        if ($existing !== null) {
            throw new ApiException(409, 'reminder_already_exists', 'The follow-up reminder already exists.');
        }

        $links = $this->resolveFollowUpTarget($principal, $targetType, $targetUuid, $input);
        $statement = $this->db->prepare('INSERT INTO ssw_follow_up_reminders '
            . '(reminder_uuid, customer_id, installation_id, target_type, selection_id, multi_project_id, '
            . 'target_uuid, display_reference, email_prepared_at_utc, due_at_utc) '
            . 'VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)');
        $statement->execute([
            $reminderUuid, $principal['customer_id'], $principal['installation_id'], $targetType,
            $links['selection_id'], $links['multi_project_id'], $targetUuid, $displayReference,
            $this->databaseTimestamp($prepared), $this->databaseTimestamp($due),
        ]);
        $reminderId = (int) $this->db->lastInsertId();
        $this->insertFollowUpEvent($principal, $reminderId, 'Created', null, $due, null, 'Pending');

        $row = $this->ownedFollowUp($principal, $reminderUuid, false);
        return ['status' => 201, 'body' => ['reminder' => $this->followUpResponse($row)]];
    }

    public function listFollowUps(array $principal, array $query): array
    {
        $scope = strtolower(trim((string) ($query['scope'] ?? 'active')));
        if (!in_array($scope, ['active', 'due', 'upcoming', 'closed', 'all'], true)) {
            throw new ApiException(400, 'invalid_scope', 'scope must be active, due, upcoming, closed or all.');
        }
        $limit = 100;
        if (isset($query['limit']) && $query['limit'] !== '') {
            if (!preg_match('/^[0-9]{1,3}$/D', (string) $query['limit'])) {
                throw new ApiException(400, 'invalid_limit', 'limit must be an integer from 1 through 200.');
            }
            $limit = (int) $query['limit'];
            if ($limit < 1 || $limit > 200) {
                throw new ApiException(400, 'invalid_limit', 'limit must be an integer from 1 through 200.');
            }
        }

        $where = ['customer_id = ?', 'installation_id = ?'];
        $parameters = [$principal['customer_id'], $principal['installation_id']];
        $now = gmdate('Y-m-d H:i:s');
        if ($scope === 'active') {
            $where[] = "status = 'Pending'";
        } elseif ($scope === 'due') {
            $where[] = "status = 'Pending'";
            $where[] = 'due_at_utc <= ?';
            $parameters[] = $now;
        } elseif ($scope === 'upcoming') {
            $where[] = "status = 'Pending'";
            $where[] = 'due_at_utc > ?';
            $parameters[] = $now;
        } elseif ($scope === 'closed') {
            $where[] = "status <> 'Pending'";
        }
        $updatedAfterId = 0;
        if (isset($query['updated_after_id']) && $query['updated_after_id'] !== '') {
            if (!preg_match('/^[0-9]+$/D', (string) $query['updated_after_id'])) {
                throw new ApiException(400, 'invalid_cursor', 'updated_after_id must be a non-negative integer.');
            }
            $updatedAfterId = (int) $query['updated_after_id'];
        }
        if (isset($query['updated_since']) && trim((string) $query['updated_since']) !== '') {
            $updated = $this->parseUtcTimestamp((string) $query['updated_since'], 'updated_since');
            $updatedValue = $this->databaseTimestamp($updated);
            $where[] = '(updated_at > ? OR (updated_at = ? AND id > ?))';
            $parameters[] = $updatedValue;
            $parameters[] = $updatedValue;
            $parameters[] = $updatedAfterId;
        } elseif ($updatedAfterId !== 0) {
            throw new ApiException(400, 'invalid_cursor', 'updated_after_id requires updated_since.');
        }

        $statement = $this->db->prepare('SELECT * FROM ssw_follow_up_reminders WHERE '
            . implode(' AND ', $where) . ' ORDER BY updated_at ASC, id ASC LIMIT ' . ($limit + 1));
        $statement->execute($parameters);
        $rows = $statement->fetchAll();
        $hasMore = count($rows) > $limit;
        if ($hasMore) {
            array_pop($rows);
        }
        $items = [];
        foreach ($rows as $row) {
            $items[] = $this->followUpResponse($row);
        }
        $lastRow = count($rows) === 0 ? null : $rows[count($rows) - 1];
        $nextUpdatedSince = $lastRow === null ? null : $this->apiTimestamp($lastRow['updated_at']);
        return [
            'reminders' => $items,
            'scope' => $scope,
            'has_more' => $hasMore,
            'next_updated_since' => $nextUpdatedSince,
            'next_updated_after_id' => $lastRow === null ? null : (int) $lastRow['id'],
            'server_time_utc' => gmdate('Y-m-d\TH:i:s\Z'),
        ];
    }

    public function rescheduleFollowUp(array $principal, string $reminderUuid, array $input): array
    {
        $reminderUuid = strtolower($this->validateUuidValue($reminderUuid, 'reminder_id'));
        $rescheduled = $this->requiredUtcTimestamp($input, 'rescheduled_at_utc');
        $due = $this->requiredUtcTimestamp($input, 'due_at_utc');
        $days = $this->requiredRangeInt($input, 'follow_up_days', 1, 90);
        $this->validateFollowUpInterval($rescheduled, $due, $days);
        $row = $this->ownedFollowUp($principal, $reminderUuid, true);
        if ($row['status'] !== 'Pending') {
            throw new ApiException(409, 'reminder_not_pending', 'Only a pending follow-up reminder can be rescheduled.');
        }

        $oldDue = $this->parseDatabaseTimestamp($row['due_at_utc']);
        $changedAt = $this->databaseTimestamp(new DateTimeImmutable('now', new DateTimeZone('UTC')));
        $this->db->prepare('UPDATE ssw_follow_up_reminders SET due_at_utc = ?, '
            . 'reschedule_count = reschedule_count + 1, updated_at = ? WHERE id = ?')
            ->execute([$this->databaseTimestamp($due), $changedAt, (int) $row['id']]);
        $this->insertFollowUpEvent($principal, (int) $row['id'], 'Rescheduled', $oldDue, $due, 'Pending', 'Pending');

        $updated = $this->ownedFollowUp($principal, $reminderUuid, false);
        return ['status' => 200, 'body' => ['reminder' => $this->followUpResponse($updated)]];
    }

    public function closeFollowUp(array $principal, string $reminderUuid, array $input): array
    {
        $reminderUuid = strtolower($this->validateUuidValue($reminderUuid, 'reminder_id'));
        $status = $this->requiredPattern($input, 'status', '/^(Succeeded|Unsuccessful|Cancelled)$/D');
        $row = $this->ownedFollowUp($principal, $reminderUuid, true);
        if ($row['status'] !== 'Pending') {
            throw new ApiException(409, 'reminder_not_pending', 'Only a pending follow-up reminder can be closed.');
        }

        $closedAt = $this->databaseTimestamp(new DateTimeImmutable('now', new DateTimeZone('UTC')));
        $this->db->prepare('UPDATE ssw_follow_up_reminders SET status = ?, closed_at_utc = ?, '
            . 'updated_at = ? WHERE id = ?')->execute([$status, $closedAt, $closedAt, (int) $row['id']]);
        $due = $this->parseDatabaseTimestamp($row['due_at_utc']);
        $this->insertFollowUpEvent($principal, (int) $row['id'], 'Closed', $due, $due, 'Pending', $status);

        $updated = $this->ownedFollowUp($principal, $reminderUuid, false);
        return ['status' => 200, 'body' => ['reminder' => $this->followUpResponse($updated)]];
    }

    public function enforceRateLimit(string $subject, string $operation, int $limit): void
    {
        $window = gmdate('Y-m-d H:i:00');
        $hash = hash('sha256', $subject);
        try {
            $statement = $this->db->prepare('INSERT INTO ssw_api_rate_limits '
                . '(subject_hash, operation_name, window_started_at, request_count, expires_at) VALUES (?, ?, ?, 1, ?)');
            $statement->execute([$hash, $operation, $window, gmdate('Y-m-d H:i:s', time() + 3600)]);
            return;
        } catch (PDOException $exception) {
            if (!$this->isUniqueViolation($exception)) throw $exception;
        }
        $this->db->prepare('UPDATE ssw_api_rate_limits SET request_count = request_count + 1 '
            . 'WHERE subject_hash = ? AND operation_name = ? AND window_started_at = ?')
            ->execute([$hash, $operation, $window]);
        $row = $this->fetchOne('SELECT request_count FROM ssw_api_rate_limits '
            . 'WHERE subject_hash = ? AND operation_name = ? AND window_started_at = ?', [$hash, $operation, $window]);
        if ($row !== null && (int) $row['request_count'] > $limit) {
            throw new ApiException(429, 'rate_limit_exceeded', 'Too many requests. Retry later.');
        }
    }

    private function issueToken(int $installationId): array
    {
        $raw = $this->generateOpaqueToken();
        $expires = gmdate('Y-m-d H:i:s', time() + self::TOKEN_LIFETIME_DAYS * 86400);
        $this->db->prepare('INSERT INTO ssw_installation_tokens (installation_id, token_hash, expires_at) VALUES (?, ?, ?)')
            ->execute([$installationId, hash('sha256', $raw), $expires]);
        return ['raw' => $raw, 'expires_at' => str_replace(' ', 'T', $expires) . 'Z'];
    }

    private function generateOpaqueToken(): string
    {
        return rtrim(strtr(base64_encode(random_bytes(32)), '+/', '-_'), '=');
    }

    private function insertRevision(int $selectionId, int $installationId, int $revision, string $payloadHash,
        array $payload, array $versions, array $fingerprints, string $changeKind, ?array $geolocation): void
    {
        $location = $this->normalizeGeolocation($geolocation);
        $statement = $this->db->prepare('INSERT INTO ssw_selection_revisions '
            . '(selection_id, installation_id, revision_number, payload_hash, technical_input_hash, calculation_output_hash, '
            . 'calculation_basis_hash, snapshot_hash, change_kind, geo_country_code, geo_city, geo_source, geo_accuracy_km, '
            . 'selection_payload, software_version, '
            . 'calculation_engine_version, database_schema_version, database_data_version, database_content_hash, '
            . 'selection_format_version, report_template_version, api_contract_version) '
            . 'VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)');
        $statement->execute([
            $selectionId, $installationId, $revision, $payloadHash, $fingerprints['technical_input_hash'],
            $fingerprints['calculation_output_hash'], $fingerprints['calculation_basis_hash'],
            $fingerprints['snapshot_hash'], $changeKind, $location['country_code'], $location['city'],
            $location['source'], $location['accuracy_km'], $this->encodeJson($payload),
            $versions['software_version'], $versions['calculation_engine_version'], $versions['database_schema_version'],
            $versions['database_data_version'], $versions['database_content_hash'], $versions['selection_format_version'],
            $versions['report_template_version'], $versions['api_contract_version'],
        ]);
    }

    private function resolveFollowUpTarget(array $principal, string $targetType, string $targetUuid, array $input): array
    {
        $registeredReference = $this->optionalString($input, 'registered_reference', 255);
        if ($targetType === 'Selection') {
            $selection = $this->fetchOne('SELECT id, public_reference FROM ssw_selections '
                . 'WHERE customer_id = ? AND project_id = ?', [$principal['customer_id'], $targetUuid]);
            if ($registeredReference !== null) {
                $digits = $this->selectionReferenceDigits($registeredReference);
                if ($selection === null || !hash_equals((string) $selection['public_reference'], $digits)) {
                    throw new ApiException(404, 'target_not_found', 'The registered follow-up target is unavailable.');
                }
            }
            return ['selection_id' => $selection === null ? null : (int) $selection['id'], 'multi_project_id' => null];
        }

        $project = $this->fetchOne('SELECT id, project_reference FROM ssw_multi_projects '
            . 'WHERE customer_id = ? AND project_uuid = ?', [$principal['customer_id'], $targetUuid]);
        if ($registeredReference !== null
            && ($project === null || !hash_equals((string) $project['project_reference'], $registeredReference))) {
            throw new ApiException(404, 'target_not_found', 'The registered follow-up target is unavailable.');
        }
        return ['selection_id' => null, 'multi_project_id' => $project === null ? null : (int) $project['id']];
    }

    private function ownedFollowUp(array $principal, string $reminderUuid, bool $forUpdate): array
    {
        $lock = $forUpdate && $this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'mysql' ? ' FOR UPDATE' : '';
        $row = $this->fetchOne('SELECT * FROM ssw_follow_up_reminders '
            . 'WHERE customer_id = ? AND installation_id = ? AND reminder_uuid = ?' . $lock,
            [$principal['customer_id'], $principal['installation_id'], $reminderUuid]);
        if ($row === null) {
            throw new ApiException(404, 'reminder_not_found', 'The follow-up reminder does not exist.');
        }
        return $row;
    }

    private function insertFollowUpEvent(array $principal, int $reminderId, string $eventType,
        ?DateTimeImmutable $oldDue, ?DateTimeImmutable $newDue, ?string $oldStatus, string $newStatus): void
    {
        $this->db->prepare('INSERT INTO ssw_follow_up_reminder_events '
            . '(reminder_id, customer_id, installation_id, event_type, old_due_at_utc, new_due_at_utc, old_status, new_status) '
            . 'VALUES (?, ?, ?, ?, ?, ?, ?, ?)')->execute([
                $reminderId, $principal['customer_id'], $principal['installation_id'], $eventType,
                $oldDue === null ? null : $this->databaseTimestamp($oldDue),
                $newDue === null ? null : $this->databaseTimestamp($newDue), $oldStatus, $newStatus,
            ]);
    }

    private function followUpResponse(array $row): array
    {
        return [
            'reminder_id' => $row['reminder_uuid'],
            'target_type' => $row['target_type'],
            'target_id' => $row['target_uuid'],
            'display_reference' => $row['display_reference'],
            'email_prepared_at_utc' => $this->apiTimestamp($row['email_prepared_at_utc']),
            'due_at_utc' => $this->apiTimestamp($row['due_at_utc']),
            'status' => $row['status'],
            'reschedule_count' => (int) $row['reschedule_count'],
            'closed_at_utc' => $row['closed_at_utc'] === null ? null : $this->apiTimestamp($row['closed_at_utc']),
            'registered_target' => $row['selection_id'] !== null || $row['multi_project_id'] !== null,
            'created_at_utc' => $this->apiTimestamp($row['created_at']),
            'updated_at_utc' => $this->apiTimestamp($row['updated_at']),
        ];
    }

    private function validateFollowUpInterval(DateTimeImmutable $base, DateTimeImmutable $due, int $days): void
    {
        $seconds = $due->getTimestamp() - $base->getTimestamp();
        $expected = $days * 86400;
        if ($seconds <= 0 || abs($seconds - $expected) > 7200) {
            throw new ApiException(400, 'invalid_follow_up_interval',
                'due_at_utc must represent the declared 1-90 day follow-up interval.');
        }
    }

    private function requiredUtcTimestamp(array $input, string $key): DateTimeImmutable
    {
        $value = $input[$key] ?? null;
        if (!is_string($value)) {
            throw new ApiException(400, 'invalid_request', "$key is invalid.");
        }
        return $this->parseUtcTimestamp($value, $key);
    }

    private function parseUtcTimestamp(string $value, string $key): DateTimeImmutable
    {
        if (!preg_match('/^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})(?:\.(\d{1,6}))?Z$/D', $value, $matches)) {
            throw new ApiException(400, 'invalid_timestamp', "$key must be an ISO 8601 UTC timestamp.");
        }
        $fraction = isset($matches[2]) ? str_pad($matches[2], 6, '0') : '000000';
        $date = DateTimeImmutable::createFromFormat('!Y-m-d\TH:i:s.u\Z', $matches[1] . '.' . $fraction . 'Z',
            new DateTimeZone('UTC'));
        $errors = DateTimeImmutable::getLastErrors();
        if ($date === false || (is_array($errors) && ($errors['warning_count'] > 0 || $errors['error_count'] > 0))) {
            throw new ApiException(400, 'invalid_timestamp', "$key must be an ISO 8601 UTC timestamp.");
        }
        return $date;
    }

    private function parseDatabaseTimestamp(string $value): DateTimeImmutable
    {
        return new DateTimeImmutable($value, new DateTimeZone('UTC'));
    }

    private function databaseTimestamp(DateTimeImmutable $value): string
    {
        return $value->setTimezone(new DateTimeZone('UTC'))->format('Y-m-d H:i:s.u');
    }

    private function apiTimestamp(string $value): string
    {
        return $this->parseDatabaseTimestamp($value)->setTimezone(new DateTimeZone('UTC'))->format('Y-m-d\TH:i:s.u\Z');
    }

    private function selectionReferenceDigits(string $reference): string
    {
        if (!preg_match('/^([0-9]{4})-?([0-9]{4})-?([0-9]{4})-?([0-9]{4})(?:-R[0-9]{2,})?$/Di', $reference, $matches)) {
            throw new ApiException(400, 'invalid_request', 'registered_reference is invalid.');
        }
        return $matches[1] . $matches[2] . $matches[3] . $matches[4];
    }

    private function validateUuidValue(string $value, string $key): string
    {
        if (!preg_match('/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/D', $value)) {
            throw new ApiException(400, 'invalid_request', "$key is invalid.");
        }
        return $value;
    }

    private function requiredRangeInt(array $input, string $key, int $minimum, int $maximum): int
    {
        $value = $input[$key] ?? null;
        if (!is_int($value) || $value < $minimum || $value > $maximum) {
            throw new ApiException(400, 'invalid_request', "$key must be between $minimum and $maximum.");
        }
        return $value;
    }

    private function normalizeGeolocation(?array $value): array
    {
        $countryCode = strtoupper(trim((string) ($value['country_code'] ?? '')));
        if (!preg_match('/^[A-Z]{2}$/D', $countryCode)) {
            return ['country_code' => null, 'city' => null, 'source' => null, 'accuracy_km' => null];
        }
        $city = trim((string) ($value['city'] ?? ''));
        $source = trim((string) ($value['source'] ?? ''));
        $accuracy = isset($value['accuracy_km']) && is_numeric($value['accuracy_km'])
            ? max(0.0, min(999999.99, (float) $value['accuracy_km'])) : null;
        return [
            'country_code' => $countryCode,
            'city' => $city === '' ? null : substr($city, 0, 128),
            'source' => $source === '' ? 'local' : substr($source, 0, 64),
            'accuracy_km' => $accuracy,
        ];
    }

    private function validateVersions(array $versions): array
    {
        return [
            'software_version' => $this->requiredString($versions, 'software_version', 1, 32),
            'calculation_engine_version' => $this->requiredString($versions, 'calculation_engine_version', 1, 32),
            'database_schema_version' => $this->requiredPositiveInt($versions, 'database_schema_version'),
            'database_data_version' => $this->optionalString($versions, 'database_data_version', 64),
            'database_content_hash' => $this->requiredString($versions, 'database_content_hash', 1, 128),
            'selection_format_version' => $this->requiredPositiveInt($versions, 'selection_format_version'),
            'report_template_version' => $this->requiredPositiveInt($versions, 'report_template_version'),
            'api_contract_version' => $this->requiredPositiveInt($versions, 'api_contract_version'),
        ];
    }

    private function validateFingerprints(array $fingerprints): array
    {
        $result = [];
        foreach (['technical_input_hash', 'calculation_output_hash', 'calculation_basis_hash', 'snapshot_hash'] as $name) {
            $value = $fingerprints[$name] ?? null;
            if (!is_string($value) || !preg_match('/^[0-9a-f]{64}$/D', $value)) {
                throw new ApiException(400, 'invalid_request', "$name is invalid.");
            }
            $result[$name] = $value;
        }
        return $result;
    }

    private function classifyChange(array $latest, array $versions, array $fingerprints): string
    {
        if (!hash_equals((string) $latest['calculation_engine_version'], $versions['calculation_engine_version'])) {
            return 'AlgorithmChange';
        }
        if ((int) $latest['database_schema_version'] !== $versions['database_schema_version']
            || (string) $latest['database_data_version'] !== (string) ($versions['database_data_version'] ?? '')
            || !hash_equals((string) $latest['database_content_hash'], $versions['database_content_hash'])) {
            return 'DatabaseChange';
        }
        if ((int) $latest['report_template_version'] !== $versions['report_template_version']) {
            return 'TechnicalChange';
        }
        if (!is_string($latest['technical_input_hash'])
            || !hash_equals($latest['technical_input_hash'], $fingerprints['technical_input_hash'])) {
            return 'TechnicalChange';
        }
        if (!is_string($latest['calculation_output_hash'])
            || !hash_equals($latest['calculation_output_hash'], $fingerprints['calculation_output_hash'])) {
            return 'CalculationResultChange';
        }
        return 'TechnicalChange';
    }

    private function audit(?int $customerId, ?int $installationId, ?int $selectionId, string $event, array $data): void
    {
        $this->db->prepare('INSERT INTO ssw_audit_events (customer_id, installation_id, selection_id, event_type, event_data) '
            . 'VALUES (?, ?, ?, ?, ?)')->execute([$customerId, $installationId, $selectionId, $event, $this->encodeJson($data)]);
    }

    private function fetchOne(string $sql, array $parameters): ?array
    {
        $statement = $this->db->prepare($sql);
        $statement->execute($parameters);
        $row = $statement->fetch();
        return $row === false ? null : $row;
    }

    private function canonicalJson($value): string
    {
        if (is_array($value)) {
            if ($this->isList($value)) {
                foreach ($value as $key => $item) {
                    $value[$key] = $this->decodeJson($this->canonicalJson($item));
                }
            } else {
                ksort($value, SORT_STRING);
                foreach ($value as $key => $item) {
                    $value[$key] = $this->decodeJson($this->canonicalJson($item));
                }
            }
        }
        return $this->encodeJson($value, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE | JSON_PRESERVE_ZERO_FRACTION);
    }

    private function isUniqueViolation(PDOException $exception): bool
    {
        return $exception->getCode() === '23000' || $exception->getCode() === '23505'
            || strpos($exception->getMessage(), 'UNIQUE constraint failed') !== false;
    }

    private function isList(array $value): bool
    {
        $expected = 0;
        foreach ($value as $key => $_) {
            if ($key !== $expected++) return false;
        }
        return true;
    }

    private function encodeJson($value, int $flags = 0): string
    {
        $encoded = json_encode($value, $flags);
        if ($encoded === false) {
            throw new RuntimeException('Unable to encode JSON: ' . json_last_error_msg());
        }
        return $encoded;
    }

    private function decodeJson(string $value)
    {
        $decoded = json_decode($value, true, 512);
        if (json_last_error() !== JSON_ERROR_NONE) {
            throw new RuntimeException('Unable to decode JSON: ' . json_last_error_msg());
        }
        return $decoded;
    }

    private function requiredArray(array $input, string $key): array
    {
        if (!isset($input[$key]) || !is_array($input[$key])) throw new ApiException(400, 'invalid_request', "$key must be an object.");
        return $input[$key];
    }

    private function requiredString(array $input, string $key, int $min, int $max): string
    {
        $value = $input[$key] ?? null;
        if (!is_string($value) || strlen($value) < $min || strlen($value) > $max) throw new ApiException(400, 'invalid_request', "$key is invalid.");
        return $value;
    }

    private function requiredEmail(array $input, string $key): string
    {
        $value = strtolower($this->requiredString($input, $key, 3, 254));
        if (filter_var($value, FILTER_VALIDATE_EMAIL) === false) {
            throw new ApiException(400, 'invalid_request', "$key is invalid.");
        }
        return $value;
    }

    private function optionalString(array $input, string $key, int $max): ?string
    {
        if (!array_key_exists($key, $input) || $input[$key] === null) return null;
        return $this->requiredString($input, $key, 1, $max);
    }

    private function requiredUuid(array $input, string $key): string
    {
        return $this->requiredPattern($input, $key, '/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/D');
    }

    private function requiredPattern(array $input, string $key, string $pattern): string
    {
        $value = $input[$key] ?? null;
        if (!is_string($value) || !preg_match($pattern, $value)) throw new ApiException(400, 'invalid_request', "$key is invalid.");
        return $value;
    }

    private function requiredPositiveInt(array $input, string $key): int
    {
        $value = $input[$key] ?? null;
        if (!is_int($value) || $value < 1) throw new ApiException(400, 'invalid_request', "$key is invalid.");
        return $value;
    }

    private function optionalPositiveInt(array $input, string $key): ?int
    {
        if (!array_key_exists($key, $input) || $input[$key] === null) return null;
        return $this->requiredPositiveInt($input, $key);
    }
}
