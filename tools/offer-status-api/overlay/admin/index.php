<?php
declare(strict_types=1);

require_once dirname(__DIR__) . '/v1/bootstrap.php';
require_once dirname(__DIR__) . '/v1/lib/TechnicalSelectionAdminService.php';

header('X-Content-Type-Options: nosniff');
header('X-Frame-Options: DENY');
header('Referrer-Policy: no-referrer');
header("Content-Security-Policy: default-src 'self'; style-src 'self' 'unsafe-inline'; form-action 'self'; frame-ancestors 'none'");
header('Cache-Control: no-store');
header('X-Robots-Tag: noindex, nofollow');

session_name('SSWADMIN');
session_set_cookie_params(0, '/api/admin', '', !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off', true);
session_start();

function admin_h($value): string
{
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

function admin_csrf(): string
{
    if (empty($_SESSION['csrf'])) $_SESSION['csrf'] = bin2hex(random_bytes(24));
    return (string) $_SESSION['csrf'];
}

function admin_authenticated(): bool
{
    $authenticatedAt = (int) ($_SESSION['authenticated_at'] ?? 0);
    if (empty($_SESSION['admin_user']) || $authenticatedAt < time() - 1800) return false;
    $_SESSION['authenticated_at'] = time();
    return true;
}

$configuredUser = selection_env('SSW_SELECTION_ADMIN_USER', false) ?? 'avensys';
$configuredHash = selection_env('SSW_SELECTION_ADMIN_PASSWORD_HASH', false) ?? '';
$error = '';

if (($_SERVER['REQUEST_METHOD'] ?? '') === 'POST' && isset($_POST['logout'])) {
    if (hash_equals(admin_csrf(), (string) ($_POST['csrf'] ?? ''))) {
        $_SESSION = [];
        session_destroy();
    }
    header('Location: ./');
    exit;
}

if (!admin_authenticated() && ($_SERVER['REQUEST_METHOD'] ?? '') === 'POST') {
    $user = trim((string) ($_POST['user'] ?? ''));
    $password = (string) ($_POST['password'] ?? '');
    if ($configuredHash !== '' && hash_equals($configuredUser, $user) && password_verify($password, $configuredHash)) {
        session_regenerate_id(true);
        $_SESSION['admin_user'] = $configuredUser;
        $_SESSION['authenticated_at'] = time();
        header('Location: ./');
        exit;
    }
    usleep(500000);
    $error = 'Credenziali non valide.';
}

if (!admin_authenticated()) {
    http_response_code($configuredHash === '' ? 503 : 401);
    ?><!doctype html><html lang="it"><head><meta charset="utf-8"><title>Selezioni tecniche Avensys</title>
    <style>body{font:14px Arial;margin:40px;background:#f4f6f8;color:#222}main{max-width:420px;margin:auto;background:#fff;border:1px solid #bbb;padding:24px}label,input,button{display:block;width:100%;box-sizing:border-box;margin:8px 0}input,button{padding:9px}.error{color:#a40000}</style></head><body><main>
    <h1>Selezioni tecniche</h1>
    <?php if ($configuredHash === ''): ?><p class="error">Pannello non configurato.</p>
    <?php else: ?><form method="post"><label>Utente<input name="user" autocomplete="username" required></label>
    <label>Password<input type="password" name="password" autocomplete="current-password" required></label>
    <?php if ($error !== ''): ?><p class="error"><?= admin_h($error) ?></p><?php endif; ?><button>Accedi</button></form><?php endif; ?>
    </main></body></html><?php
    exit;
}

$service = new TechnicalSelectionAdminService(selection_database());
$reference = trim((string) ($_GET['reference'] ?? ''));

if (isset($_GET['download'], $_GET['revision'])) {
    $revision = (int) $_GET['revision'];
    $export = $service->exportRevision($reference, $revision);
    if ($export === null) { http_response_code(404); exit('Revisione non trovata.'); }
    $service->audit('admin.selection_downloaded', (string) $_SESSION['admin_user'], $reference, $revision);
    header('Content-Type: application/json; charset=utf-8');
    header('Content-Disposition: attachment; filename="SSW_' . preg_replace('/\D+/', '', $reference) . '_R' . sprintf('%02d', $revision) . '.json"');
    echo json_encode($export, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
    exit;
}

$selection = $reference === '' ? null : $service->find($reference);
if ($selection !== null) $service->audit('admin.selection_viewed', (string) $_SESSION['admin_user'], $reference);
?><!doctype html><html lang="it"><head><meta charset="utf-8"><title>Selezioni tecniche Avensys</title>
<style>body{font:13px Arial;margin:20px;background:#f4f6f8;color:#222}header,main{max-width:1180px;margin:auto}header{display:flex;justify-content:space-between;align-items:center}form.search{display:flex;gap:8px;margin:18px 0}.search input{flex:1;padding:9px}.search button,header button{padding:8px 14px}section{background:#fff;border:1px solid #aaa;padding:16px;margin-bottom:16px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #ccc;padding:7px;text-align:left}th{background:#ddd}.muted{color:#666}.error{color:#a40000}</style></head><body>
<header><h1>Selezioni tecniche Avensys</h1><form method="post"><input type="hidden" name="csrf" value="<?= admin_h(admin_csrf()) ?>"><button name="logout">Esci</button></form></header>
<main><form class="search" method="get"><input name="reference" value="<?= admin_h($reference) ?>" placeholder="Riferimento tecnico" required><button>Cerca</button></form>
<?php if ($reference !== '' && $selection === null): ?><p class="error">Selezione non trovata.</p><?php endif; ?>
<?php if ($selection !== null): ?><section><h2><?= admin_h(PublicReference::display($selection['public_reference'], (int) $selection['latest_revision'])) ?></h2>
<p>Cliente: <strong><?= admin_h($selection['customer_code'] . ' - ' . $selection['display_name']) ?></strong> | Installazione: <?= admin_h($selection['installation_code']) ?> | Progetto: <?= admin_h($selection['project_id']) ?></p>
<table><thead><tr><th>Rev.</th><th>Data</th><th>Modifica</th><th>Software</th><th>Motore</th><th>SDF</th><th>Località</th><th>Snapshot</th><th></th></tr></thead><tbody>
<?php foreach ($selection['revisions'] as $row): ?><tr><td>R<?= sprintf('%02d', (int) $row['revision_number']) ?></td><td><?= admin_h($row['created_at']) ?></td><td><?= admin_h($row['change_kind']) ?></td><td><?= admin_h($row['software_version']) ?></td><td><?= admin_h($row['calculation_engine_version']) ?></td><td><?= admin_h($row['database_schema_version'] . ' / ' . $row['database_data_version']) ?></td><td><?= admin_h(trim(($row['geo_country_code'] ?? '') . ' ' . ($row['geo_city'] ?? ''))) ?></td><td><?= admin_h(substr((string) $row['snapshot_hash'], 0, 12)) ?>…</td><td><a href="?reference=<?= rawurlencode($selection['public_reference']) ?>&revision=<?= (int) $row['revision_number'] ?>&download=1">JSON</a></td></tr><?php endforeach; ?>
</tbody></table>
<h3>Stato offerte per revisione</h3>
<table><thead><tr><th>Rev.</th><th>Stato offerta</th><th>Prima generazione (UTC)</th><th>Conferma definitiva (UTC)</th></tr></thead><tbody>
<?php foreach ($selection['revisions'] as $row): ?><tr>
<td>R<?= sprintf('%02d', (int) $row['revision_number']) ?></td>
<td><?= admin_h(['Provisional' => 'Provvisoria', 'Definitive' => 'Definitiva'][$row['offer_status'] ?? ''] ?? 'Non registrata') ?></td>
<td><?= admin_h($row['offer_generated_at_utc'] ?? '-') ?></td>
<td><?= admin_h($row['offer_definitive_at_utc'] ?? '-') ?></td>
</tr><?php endforeach; ?>
</tbody></table></section><?php endif; ?>
<p class="muted"><a href="https://db-ip.com" rel="noopener noreferrer">IP Geolocation by DB-IP</a>. La località è approssimativa; l'indirizzo IP non viene conservato.</p>
</main></body></html>
