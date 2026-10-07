<?php
declare(strict_types=1);

require_once dirname(__DIR__, 2) . '/api/v1/bootstrap.php';
require_once __DIR__ . '/PortalAuth.php';
require_once __DIR__ . '/SelectionRepository.php';
require_once __DIR__ . '/SelectionPresenter.php';
require_once __DIR__ . '/LicenseRepository.php';
require_once __DIR__ . '/PublishedRelease.php';

function portal_start(): void
{
    if (session_status() !== PHP_SESSION_ACTIVE) {
        session_name('ssw_portal_v2');
        session_set_cookie_params(0, '/', '', true, true);
        session_start();
        if (!headers_sent()) {
            setcookie('ssw_portal', '', time() - 3600, '/');
            setcookie('ssw_portal', '', time() - 3600, '/ssw-portal');
        }
    }
    header('Content-Type: text/html; charset=utf-8');
    header('Cache-Control: no-store, max-age=0');
    header('X-Content-Type-Options: nosniff');
    header('X-Frame-Options: DENY');
    header('Referrer-Policy: no-referrer');
    header("Content-Security-Policy: default-src 'self'; style-src 'self'; script-src 'self'; img-src 'self' data:; form-action 'self'; frame-ancestors 'none'; base-uri 'self'");
    header('X-Robots-Tag: noindex, nofollow, noarchive');
}

function portal_e($value): string
{
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

function portal_url(array $parameters = []): string
{
    return 'index.php' . ($parameters ? '?' . http_build_query($parameters) : '');
}

function portal_asset(string $path): string
{
    $fullPath = dirname(__DIR__) . '/' . ltrim($path, '/');
    $version = is_file($fullPath) ? (string) filemtime($fullPath) : '1';
    return $path . '?v=' . rawurlencode($version);
}

function portal_csrf(): string
{
    if (empty($_SESSION['portal_csrf'])) {
        $_SESSION['portal_csrf'] = bin2hex(random_bytes(24));
    }
    return (string) $_SESSION['portal_csrf'];
}

function portal_verify_csrf(): void
{
    if (!portal_csrf_is_valid()) {
        http_response_code(400);
        throw new RuntimeException('Richiesta non valida. Ricaricare la pagina e riprovare.');
    }
}

function portal_csrf_is_valid(): bool
{
    $token = isset($_POST['csrf']) ? (string) $_POST['csrf'] : '';
    return $token !== '' && hash_equals(portal_csrf(), $token);
}

function portal_refresh_csrf(): string
{
    unset($_SESSION['portal_csrf']);
    return portal_csrf();
}

function portal_render(string $template, array $data = []): void
{
    if (!array_key_exists('latestRelease', $data) && !empty($data['authenticated'])) {
        $configuredDirectory = trim((string) getenv('SSW_RELEASE_DIRECTORY'));
        $data['latestRelease'] = PublishedRelease::latest(array_filter([
            $configuredDirectory,
            'C:/inetpub/ftproot/DOCUMENTS/tools/Selection Software',
            'F:/DOCUMENTS/tools/Selection Software',
        ]));
        if ($data['latestRelease'] === null) {
            $data['latestRelease'] = PublishedRelease::fromUpdateApi('https://www.avensys-srl.com/api/ssw_check_update.php');
        }
    }
    if (!array_key_exists('licenseManagementAvailable', $data)) {
        try {
            $data['licenseManagementAvailable'] = LicenseRepository::isAvailable(selection_database());
        } catch (Throwable $exception) {
            $data['licenseManagementAvailable'] = false;
        }
    }
    if (!array_key_exists('licenseNotificationCount', $data)) {
        try {
            $data['licenseNotificationCount'] = !empty($data['authenticated']) && $data['licenseManagementAvailable']
                ? (new LicenseRepository(selection_database()))->unreadNotificationCount()
                : 0;
        } catch (Throwable $exception) {
            $data['licenseNotificationCount'] = 0;
        }
    }
    $data['licenseNotificationPreview'] = '';
    if (!empty($data['authenticated']) && !empty($data['licenseNotificationCount'])) {
        try {
            $data['licenseNotificationPreview'] = (new LicenseRepository(selection_database()))->notificationPreview();
        } catch (Throwable $exception) {
            $data['licenseNotificationPreview'] = 'Nuove richieste o variazioni di licenza';
        }
    }
    extract($data, EXTR_SKIP);
    require dirname(__DIR__) . '/templates/header.php';
    require dirname(__DIR__) . '/templates/' . $template . '.php';
    require dirname(__DIR__) . '/templates/footer.php';
}
