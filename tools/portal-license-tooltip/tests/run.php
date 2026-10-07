<?php
declare(strict_types=1);

require_once dirname(__DIR__) . '/src/SelectionPresenter.php';
require_once dirname(__DIR__) . '/src/LicenseRepository.php';
require_once dirname(__DIR__) . '/src/PublishedRelease.php';
function selection_env(string $name, bool $required = true): ?string { return $name === 'SSW_SELECTION_RATE_SECRET' ? 'test-registration-secret-32-chars' : null; }
function portal_e($value): string { return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8'); }
function portal_url(array $parameters = []): string { return 'index.php' . ($parameters ? '?' . http_build_query($parameters) : ''); }
function portal_asset(string $path): string { return $path; }
function portal_csrf(): string { return 'test-csrf-token'; }
$fixture = [
    'CustomerReference' => 'Offerta 42',
    'Unit' => ['Code' => 'CLRC 06A OSC', 'Name' => 'CLRC 06A OSC'],
    'Winter' => ['Enabled' => true, 'SupplyAirflowM3h' => 100],
    'Accessories' => [[
        'Code' => 'KTS EXTRA',
        'Quantity' => 1,
        'Availability' => 'Standard',
        'InstallationType' => 'Internal',
        'LocalizedDisplayName' => 'KTS Extra touch screen controller',
        'LocalizedFunctionNames' => ['Constant airflow control'],
    ]],
    'WaterCoil' => [
        'Enabled' => true,
        'CalculationMode' => 'HCD',
        'InstallationType' => 'External',
        'Coil' => ['Code' => 'HCD-06A', 'Name' => 'iHCD Serie A 06A'],
    ],
    'ElectricHeater' => [
        'EHD' => [
            'Enabled' => true,
            'InstallationType' => 'Internal',
            'Heater' => ['Code' => 'EH-0.75-230', 'Name' => 'EH-0.75-230'],
        ],
        'PEHD' => ['Enabled' => false],
    ],
];
$selected = [
    'payload' => $fixture,
    'revision_number' => 1,
    'change_kind' => 'create',
    'created_at' => '2026-07-21 12:00:00',
    'software_version' => '1.3.0.52',
    'calculation_engine_version' => '1.3.0.52',
    'database_schema_version' => 3,
    'database_data_version' => 'test',
    'selection_format_version' => 2,
    'report_template_version' => 3,
    'api_contract_version' => 1,
];
$detail = [
    'selected' => $selected,
    'public_reference' => '1234567890123456',
    'display_name' => 'Avensys S.r.l.',
    'customer_code' => 'AV',
    'latest_revision' => 1,
    'revisions' => [$selected],
];
ob_start();
require dirname(__DIR__) . '/templates/detail.php';
$detailHtml = (string) ob_get_clean();
$project = [
    'project_reference' => 'Progetto 01', 'display_name' => 'Avensys S.r.l.', 'language_code' => 'it',
    'updated_at' => '2026-07-21 15:00:00', 'locations' => 'IT Milano',
    'items' => [[
        'position_index' => 0, 'customer_reference' => 'Piano terra', 'unit_name' => 'CLRC 06A OSC',
        'airflow_m3h' => 100, 'pressure_pa' => 473, 'pdf_filename' => 'Piano_terra.pdf',
        'public_reference' => '1234567890123456', 'latest_revision' => 1,
    ]],
];
$includeUnlocated = false;
ob_start(); require dirname(__DIR__) . '/templates/project.php'; $projectHtml = (string) ob_get_clean();
$projects = [[
    'project_reference' => 'Progetto 01', 'project_uuid' => 'project-uuid',
    'display_name' => 'Avensys S.r.l.', 'customer_code' => 'AV', 'language_code' => 'it',
    'item_count' => 1, 'linked_count' => 1, 'locations' => 'IT Milano',
    'updated_at' => '2026-07-21 15:00:00',
]];
ob_start(); require dirname(__DIR__) . '/templates/projects.php'; $projectsHtml = (string) ob_get_clean();
$summary = ['total' => 2, 'updates' => 1, 'first_downloads' => 1, 'recent' => 2,
    'countries' => [['geo_country_code' => 'IT', 'event_count' => 2]],
    'versions' => [['selected_version' => '1.3.0.52', 'event_count' => 2]]];
$downloads = [['created_at' => '2026-07-21 16:00:00', 'download_source' => 'update',
    'selected_version' => '1.3.0.52', 'previous_version' => '1.3.0.51',
    'selected_filename' => 'AV_SSW_13052.exe', 'geo_country_code' => 'IT', 'geo_city' => 'Milano']];
ob_start(); require dirname(__DIR__) . '/templates/downloads.php'; $downloadsHtml = (string) ob_get_clean();
$stats = ['selections' => 1, 'revisions' => 1, 'recent' => 1, 'customers' => 1,
    'installations' => 1, 'projects' => 1, 'downloads' => 1];
$availableFilters = ['customers' => [], 'versions' => [], 'countries' => []];
$filters = ['q' => '', 'customer' => '', 'version' => '', 'country' => '', 'from' => '', 'to' => '',
    'include_unlocated' => false];
$results = ['items' => [], 'total' => 0, 'page' => 1, 'pages' => 1];
ob_start(); require dirname(__DIR__) . '/templates/dashboard.php'; $dashboardHtml = (string) ob_get_clean();
ob_start(); require dirname(__DIR__) . '/templates/footer.php'; $footerHtml = (string) ob_get_clean();
$authenticated = true;
$licenseManagementAvailable = true;
$licenseNotificationCount = 0;
$latestRelease = ['version' => '2.0.0.6', 'published_at_utc' => '2026-09-17T09:50:13Z', 'filename' => 'SSW_Setup_2_0_0_6.exe'];
$user = 'test-admin';
ob_start(); require dirname(__DIR__) . '/templates/header.php'; $headerHtml = (string) ob_get_clean();
$registrationToken = '1.test-token';
$registrationError = null;
$pageLanguage = 'en';
$registration = ['status' => 'invited', 'email' => 'invite@example.com', 'company_name' => 'Example GmbH'];
ob_start(); require dirname(__DIR__) . '/templates/registration.php'; $registrationFormHtml = (string) ob_get_clean();
$registration = ['status' => 'submitted'];
ob_start(); require dirname(__DIR__) . '/templates/registration.php'; $registrationWaitingHtml = (string) ob_get_clean();
$registration = ['status' => 'approved'];
ob_start(); require dirname(__DIR__) . '/templates/registration.php'; $registrationApprovedHtml = (string) ob_get_clean();
$registrationRequests = [];
$activationRequests = [];
$activation = null;
$registrationInvitation = null;
$customers = [['id' => 1]];
$registrationInvitations = [];
$updateInvitationUsers = [];
$licenseUserChanges = [41 => [[
    'event_type' => 'license.activated', 'label' => 'Dispositivo attivato · ABCD',
    'created_at' => '2026-09-21 12:00:00',
]]];
$licenseUsers = [[
    'id' => 41, 'first_name' => 'Pending', 'last_name' => 'User', 'email' => 'pending@example.com',
    'company_name' => 'Example', 'status' => 'pending', 'launches' => 0, 'active_days' => 0,
    'last_used_at' => null, 'active_devices' => 1, 'devices' => [[
        'id' => 101, 'device_number' => 1, 'installation_code' => 'ABCD', 'status' => 'active',
        'software_version' => '2.0.0.5', 'last_used_at' => null, 'launches' => 1, 'active_days' => 1,
    ]], 'pending_activation_code_id' => 11,
    'pending_activation_code' => '123456', 'pending_activation_expires_at' => '2026-09-26 12:00:00',
], [
    'id' => 42, 'first_name' => 'Legacy', 'last_name' => 'User', 'email' => 'legacy@example.com',
    'company_name' => 'Example', 'status' => 'pending', 'launches' => 0, 'active_days' => 0,
    'last_used_at' => null, 'active_devices' => 0, 'devices' => [], 'pending_activation_code_id' => 12,
    'pending_activation_code' => null, 'pending_activation_expires_at' => '2026-09-26 12:00:00',
]];
ob_start(); require dirname(__DIR__) . '/templates/licenses.php'; $licensesHtml = (string) ob_get_clean();
$updateInvitationUsers = [[
    'id' => 50, 'first_name' => 'Old', 'last_name' => 'Version', 'email' => 'old@example.com',
    'company_name' => 'Example', 'minimum_update_version' => '2.0.0.15',
    'update_workflow_state' => 'update_required', 'update_request_event_id' => 0,
    'outdated_devices' => [['device_number' => 1, 'software_version' => '2.0.0.14']],
]];
$registrationInvitations = [[
    'id' => 60, 'email' => 'invited@example.com', 'company_name' => 'Example', 'status' => 'invited',
    'expires_in' => 'Scade tra 6 giorni', 'expires_at' => '2026-09-28 12:00:00', 'expired' => false,
]];
ob_start(); require dirname(__DIR__) . '/templates/licenses.php'; $unifiedActionsHtml = (string) ob_get_clean();
$registrationInvitations = [[
    'id' => 61, 'email' => 'expired@example.com', 'company_name' => 'Example', 'status' => 'invited',
    'expires_in' => 'Scaduto da 2 giorni', 'expires_at' => '2026-09-20 12:00:00', 'expired' => true,
]];
$pendingUsers = [[
    'id' => 62, 'first_name' => 'Expired', 'last_name' => 'PIN', 'email' => 'pin@example.com',
    'company_name' => 'Example', 'expires_at' => '2026-09-20 12:00:00',
    'expires_in' => 'Scaduto da 2 giorni', 'activation_expired' => true,
]];
ob_start(); require dirname(__DIR__) . '/templates/licenses.php'; $expiredActionsHtml = (string) ob_get_clean();
$tests = [
    PublishedRelease::fromUpdateApi('https://example.com/not-allowed') === null,
    SelectionPresenter::customerReference($fixture) === 'Offerta 42',
    SelectionPresenter::unit($fixture) === 'CLRC 06A OSC',
    SelectionPresenter::get(SelectionPresenter::scenario($fixture, 'winter'), 'supplyAirflowM3h') === 100,
    SelectionPresenter::reference('1234567890123456', 2) === '1234-5678-9012-3456-R02',
    SelectionPresenter::selection(['selection' => $fixture]) === $fixture,
    SelectionPresenter::comparisonRows($fixture)['Inverno · portata mandata'] === '100 m³/h',
    count(SelectionPresenter::accessories($fixture)) === 3,
    SelectionPresenter::accessoryName(SelectionPresenter::accessories($fixture)[0]) === 'KTS Extra touch screen controller',
    SelectionPresenter::accessoryFunctions(SelectionPresenter::accessories($fixture)[0]) === ['Constant airflow control'],
    SelectionPresenter::accessoryStatusClass(SelectionPresenter::accessories($fixture)[0]) === 'standard',
    SelectionPresenter::accessoryStatus(SelectionPresenter::accessories($fixture)[0]) === 'Di serie',
    SelectionPresenter::accessoryStatus(SelectionPresenter::accessories($fixture)[1]) === 'Esterna',
    SelectionPresenter::accessoryStatus(SelectionPresenter::accessories($fixture)[2]) === 'Interna',
    SelectionPresenter::accessoryName(SelectionPresenter::accessories($fixture)[1]) === 'Batteria ad acqua a 2 tubi per riscaldamento e raffreddamento',
    SelectionPresenter::accessoryName(SelectionPresenter::accessories($fixture)[2]) === 'Batteria elettrica di post-riscaldamento',
    SelectionPresenter::comparisonRows($fixture)['Accessori'] === 'KTS EXTRA, HCD, EHD',
    strpos($detailHtml, 'Accessori e funzioni') !== false,
    strpos($detailHtml, 'KTS EXTRA') !== false,
    strpos($detailHtml, 'Constant airflow control') !== false,
    strpos($detailHtml, 'Di serie') !== false,
    strpos($detailHtml, '>HCD<') !== false,
    strpos($detailHtml, '>EHD<') !== false,
    strpos($detailHtml, 'Optional') === false,
    strpos($projectHtml, 'Progetto 01') !== false && strpos($projectHtml, 'Piano terra') !== false
        && strpos($projectHtml, '1234-5678-9012-3456-R01') !== false,
    strpos($downloadsHtml, '1.3.0.52') !== false && strpos($downloadsHtml, 'IT · Milano') !== false,
    strpos($projectsHtml, 'Mostra località non disponibile') !== false,
    strpos($projectsHtml, 'IT Milano') !== false,
    strpos($downloadsHtml, 'Mostra località non disponibile') !== false,
    strpos($dashboardHtml, 'Mostra località non disponibile') !== false,
    strpos($footerHtml, 'https://db-ip.com') !== false && strpos($footerHtml, 'IP geolocation by DB-IP') !== false,
    strpos($headerHtml, 'Ultima release disponibile') !== false && strpos($headerHtml, '>2.0.0.6<') !== false,
    strpos($registrationFormHtml, 'First name') !== false && strpos($registrationFormHtml, 'Prénom') !== false && strpos($registrationFormHtml, 'Vorname') !== false,
    substr_count($registrationFormHtml, '<span class="field-label">Email</span>') === 1
        && strpos($registrationFormHtml, '>E-mail<') === false
        && strpos($registrationFormHtml, '>E-Mail<') === false,
    substr_count($registrationFormHtml, 'language-flag') >= 20,
    strpos($registrationWaitingHtml, 'Your request has been sent') !== false && strpos($registrationWaitingHtml, 'Votre demande a été envoyée') !== false && strpos($registrationWaitingHtml, 'Ihre Anfrage wurde gesendet') !== false,
    strpos($registrationApprovedHtml, 'Download SSW') !== false && strpos($registrationApprovedHtml, 'Télécharger SSW') !== false && strpos($registrationApprovedHtml, 'SSW herunterladen') !== false,
    strpos($licensesHtml, 'Scarica email con PIN') !== false
        && strpos($licensesHtml, 'Genera nuovo PIN e scarica email') !== false
        && strpos($licensesHtml, 'value="new_code_email"') !== false,
    substr_count($licensesHtml, 'Ultima release disponibile') === 2
        && strpos($licensesHtml, 'Versione installata') !== false
        && strpos($licensesHtml, '2.0.0.5') !== false
        && strpos($licensesHtml, 'AGGIORNAMENTO DISPONIBILE') !== false,
    strpos($licensesHtml, 'license-user-changed') !== false
        && strpos($licensesHtml, 'Variazioni recenti per questo utente') !== false
        && strpos($licensesHtml, 'Dispositivo attivato · ABCD') !== false,
    strpos($licensesHtml, '<details class="license-user') !== false
        && strpos($licensesHtml, 'class="license-user-summary"') !== false
        && strpos($licensesHtml, 'class="license-user-body"') !== false,
    substr_count($unifiedActionsHtml, 'Azioni ancora da completare') === 1
        && strpos($unifiedActionsHtml, 'Inviti di registrazione') === false
        && strpos($unifiedActionsHtml, 'Invito creato e scaricato') !== false
        && strpos($unifiedActionsHtml, 'Riscarica invito .eml') !== false
        && strpos($unifiedActionsHtml, 'Scade tra 6 giorni') !== false
        && strpos($unifiedActionsHtml, 'Aggiornamento SSW: disinstallazione da richiedere') !== false
        && strpos($unifiedActionsHtml, '2.0.0.14') !== false
        && strpos($unifiedActionsHtml, 'Crea e scarica richiesta di disinstallazione') !== false
        && strpos($unifiedActionsHtml, '<b>Fatto:</b>') !== false
        && strpos($unifiedActionsHtml, '<b>Da fare:</b>') !== false,
    strpos($expiredActionsHtml, 'Invito scaduto') !== false
        && strpos($expiredActionsHtml, 'Rigenera e scarica invito') !== false
        && strpos($expiredActionsHtml, 'Elimina invito') !== false
        && strpos($expiredActionsHtml, 'PIN scaduto · attivazione da completare') !== false
        && strpos($expiredActionsHtml, 'Rigenera PIN e scarica email') !== false
        && substr_count($expiredActionsHtml, 'Scaduto da 2 giorni') >= 2,
];

$licenseDb = new PDO('sqlite::memory:');
$licenseDb->exec("CREATE TABLE ssw_customers (id INTEGER PRIMARY KEY,customer_code TEXT,display_name TEXT,status TEXT);
CREATE TABLE ssw_users (id INTEGER PRIMARY KEY AUTOINCREMENT,customer_id INTEGER,email TEXT UNIQUE,company_name TEXT NOT NULL,first_name TEXT,last_name TEXT,status TEXT,activated_at TEXT,revoked_at TEXT,created_at TEXT DEFAULT CURRENT_TIMESTAMP,updated_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_activation_codes (id INTEGER PRIMARY KEY AUTOINCREMENT,user_id INTEGER,code_hash TEXT,display_code TEXT,attempts INTEGER DEFAULT 0,expires_at TEXT,consumed_at TEXT,created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_installations (id INTEGER PRIMARY KEY AUTOINCREMENT,customer_id INTEGER,user_id INTEGER,installation_uuid TEXT,installation_code TEXT,device_number INTEGER,status TEXT,software_version TEXT,database_schema_version INTEGER,database_content_hash TEXT,api_contract_version INTEGER,registered_at TEXT DEFAULT CURRENT_TIMESTAMP,last_seen_at TEXT DEFAULT CURRENT_TIMESTAMP,license_valid_until TEXT,license_last_check_at TEXT,revoked_at TEXT,archived_at TEXT);
CREATE TABLE ssw_usage_daily (installation_id INTEGER,usage_date TEXT,launch_count INTEGER,last_session_uuid TEXT,first_seen_at TEXT DEFAULT CURRENT_TIMESTAMP,last_seen_at TEXT DEFAULT CURRENT_TIMESTAMP,PRIMARY KEY(installation_id,usage_date));
CREATE TABLE ssw_license_events (id INTEGER PRIMARY KEY AUTOINCREMENT,user_id INTEGER,installation_id INTEGER,event_type TEXT,event_data TEXT,created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_installation_tokens (id INTEGER PRIMARY KEY AUTOINCREMENT,installation_id INTEGER,token_hash TEXT,issued_at TEXT,expires_at TEXT,revoked_at TEXT);
CREATE TABLE ssw_registration_invitations (id INTEGER PRIMARY KEY AUTOINCREMENT,customer_id INTEGER,email TEXT UNIQUE,company_name TEXT,first_name TEXT,last_name TEXT,status TEXT DEFAULT 'invited',privacy_accepted_at TEXT,submitted_at TEXT,approved_at TEXT,created_by TEXT,approved_by TEXT,created_at TEXT DEFAULT CURRENT_TIMESTAMP,updated_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_deleted_users (id INTEGER PRIMARY KEY AUTOINCREMENT,original_user_id INTEGER,email TEXT,company_name TEXT,first_name TEXT,last_name TEXT,device_count INTEGER DEFAULT 0,deleted_by TEXT,deleted_at TEXT DEFAULT CURRENT_TIMESTAMP);");
$licenseDb->exec("INSERT INTO ssw_customers VALUES (1,'AV','Avensys','active')");
$licenseRepository = new LicenseRepository($licenseDb);
$tests[] = LicenseRepository::isAvailable($licenseDb);
$registrationInvitation = $licenseRepository->createRegistrationInvitation('Invited Company A/S', 'Invite@Example.com', 'test-admin');
$tests[] = preg_match('/^1\.[a-f0-9]{64}$/', $registrationInvitation['token']) === 1;
$tests[] = preg_match('/^REG-[A-F0-9]{6}$/', $registrationInvitation['activity_code']) === 1
    && $licenseRepository->registrationInvitationForAdmin((int) $registrationInvitation['id'])['activity_code'] === $registrationInvitation['activity_code'];
$submittedInvitation = $licenseRepository->submitRegistrationInvitation($registrationInvitation['token'], 'Lars', 'Jensen', 'invite@example.com', 'Invited Company Europe', true);
$tests[] = $submittedInvitation['status'] === 'submitted' && count($licenseRepository->registrationRequests()) === 1;
$approvedInvitation = $licenseRepository->approveRegistrationInvitation((int) $submittedInvitation['id'], 'test-admin');
$tests[] = $approvedInvitation['invitation']['status'] === 'approved' && preg_match('/^[0-9]{6}$/', $approvedInvitation['activation']['pin']) === 1 && count($licenseRepository->registrationRequests()) === 0;
$tests[] = preg_match('/^PIN-[A-F0-9]{6}$/', $approvedInvitation['activation']['activity_code']) === 1
    && $approvedInvitation['activation']['activity_code'] !== $registrationInvitation['activity_code'];
$tests[] = $licenseRepository->registrationInvitations() === [];
$actualApprovedCompany = $licenseDb->query("SELECT company_name FROM ssw_users WHERE email='invite@example.com'")->fetchColumn();
$tests[] = $actualApprovedCompany === 'Invited Company Europe';
$licenseRepository->markNotificationsRead('test-admin');
$createdLicense = $licenseRepository->createUser('Example S.p.A.', 'User@Example.com', 'test-admin');
$licenseUserId = (int) $licenseDb->query("SELECT id FROM ssw_users WHERE email='user@example.com'")->fetchColumn();
$licenseRepository->updateCompany($licenseUserId, 'Example Italia S.p.A.', 'test-admin');
$licenseDb->exec("INSERT INTO ssw_installations (customer_id,user_id,installation_uuid,installation_code,device_number,status) VALUES (1,$licenseUserId,'00000000-0000-4000-8000-000000000001','ABCD',1,'active')");
$licenseDeviceId = (int) $licenseDb->lastInsertId();
$licenseDb->exec("INSERT INTO ssw_installation_tokens (installation_id,token_hash,issued_at,expires_at) VALUES ($licenseDeviceId,'hash',CURRENT_TIMESTAMP,'2030-01-01')");
$licenseRepository->setDeviceStatus($licenseDeviceId, 'revoked', 'test-admin');
$licenseRepository->archiveDevice($licenseDeviceId, 'test-admin');
$licenseDb->prepare('INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)')->execute([$licenseUserId,$licenseDeviceId,'license.checked','{}']);
$licenseDb->prepare('INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)')->execute([$licenseUserId,$licenseDeviceId,'license.activated','{}']);
$licenseDb->prepare('INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)')->execute([$licenseUserId,null,'license.activation_requested','{"first_name":"Mario","last_name":"Rossi","email":"user@example.com","company_name":"Example S.p.A.","installation_code":"5KMN"}']);
$unreadUserChanges = $licenseRepository->unreadUserChanges();
$tests[] = isset($unreadUserChanges[$licenseUserId])
    && count($unreadUserChanges[$licenseUserId]) >= 2
    && strpos($unreadUserChanges[$licenseUserId][0]['label'], 'Richiesta di attivazione ricevuta') === 0;
$tests[] = count($licenseRepository->activationRequests()) === 1;
$tests[] = $licenseRepository->unreadNotificationCount() === 3;
$preview = $licenseRepository->notificationPreview();
$tests[] = strpos($preview, 'Mario Rossi: Richiesta di attivazione') !== false;
$tests[] = strpos($preview, 'user@example.com: Attivazione da completare') !== false;
$tests[] = strpos($preview, 'license.checked') === false;
$approvedRequest = $licenseRepository->approveActivationRequest((int) $licenseDb->query("SELECT id FROM ssw_license_events WHERE event_type='license.activation_requested'")->fetchColumn(), 'test-admin');
$tests[] = preg_match('/^[0-9]{6}$/', $approvedRequest['pin']) === 1;
$tests[] = count($licenseRepository->activationRequests()) === 0;
$licenseRepository->markNotificationsRead('test-admin');
$tests[] = $licenseRepository->unreadNotificationCount() === 2;
$tests[] = $licenseRepository->unreadUserChanges() === [];
$tests[] = strpos($licenseRepository->notificationPreview(), 'Dispositivo attivato') === false;
$licenseRepository->savePushSubscription(['endpoint'=>'https://push.example.test/id','keys'=>['p256dh'=>'public','auth'=>'token'],'contentEncoding'=>'aes128gcm']);
$tests[] = (int) $licenseDb->query('SELECT COUNT(*) FROM ssw_push_subscriptions WHERE revoked_at IS NULL')->fetchColumn() === 1;
$licenseRepository->removePushSubscription('https://push.example.test/id');
$tests[] = (int) $licenseDb->query('SELECT COUNT(*) FROM ssw_push_subscriptions WHERE revoked_at IS NOT NULL')->fetchColumn() === 1;
$tests[] = $createdLicense['email'] === 'user@example.com' && preg_match('/^[0-9]{6}$/', $createdLicense['pin']) === 1;
$tests[] = $licenseDb->query("SELECT display_code FROM ssw_activation_codes WHERE consumed_at IS NULL ORDER BY id DESC LIMIT 1")->fetchColumn() === $approvedRequest['pin'];
$tests[] = $licenseRepository->pendingActivation($licenseUserId)['pin'] === $approvedRequest['pin'];
$tests[] = $licenseDb->query("SELECT company_name FROM ssw_users WHERE id=$licenseUserId")->fetchColumn() === 'Example S.p.A.';
$tests[] = password_verify($createdLicense['pin'], (string) $licenseDb->query("SELECT code_hash FROM ssw_activation_codes WHERE user_id=$licenseUserId ORDER BY id ASC LIMIT 1")->fetchColumn());
$tests[] = $licenseDb->query("SELECT archived_at IS NOT NULL FROM ssw_installations WHERE id=$licenseDeviceId")->fetchColumn() == 1;
$tests[] = $licenseDb->query("SELECT revoked_at IS NULL FROM ssw_installation_tokens WHERE installation_id=$licenseDeviceId")->fetchColumn() == 1;
$listedUsers = $licenseRepository->users();
$listedExampleUser = array_values(array_filter($listedUsers, function (array $row): bool { return $row['email'] === 'user@example.com'; }))[0];
$tests[] = count($listedExampleUser['devices']) === 0;
$recreatedUserId = (int) $licenseDb->query("SELECT id FROM ssw_users WHERE email='invite@example.com'")->fetchColumn();
$licenseDb->exec("UPDATE ssw_users SET status='active' WHERE id=$recreatedUserId");
$licenseDb->exec("INSERT INTO ssw_installations (customer_id,user_id,installation_uuid,installation_code,device_number,status) VALUES (1,$recreatedUserId,'00000000-0000-4000-8000-000000000002','EFGH',1,'active')");
$reactivatedDeviceId = (int) $licenseDb->lastInsertId();
$licenseRepository->setUserStatus($recreatedUserId, 'revoked', 'test-admin');
$tests[] = $licenseDb->query("SELECT status FROM ssw_installations WHERE id=$reactivatedDeviceId")->fetchColumn() === 'revoked';
$licenseRepository->setUserStatus($recreatedUserId, 'active', 'test-admin');
$tests[] = $licenseDb->query("SELECT status FROM ssw_installations WHERE id=$reactivatedDeviceId")->fetchColumn() === 'active';
$licenseRepository->deleteUser($recreatedUserId, 'test-admin');
$tests[] = count($licenseRepository->deletedUsers()) === 1 && $licenseDb->query("SELECT status FROM ssw_users WHERE id=$recreatedUserId")->fetchColumn() === 'deleted';
$licenseDb->exec("INSERT INTO ssw_users (customer_id,email,company_name,first_name,last_name,status) VALUES (1,'invite@example.com','Invited Company Europe','Lars','Jensen','active')");
$currentReplacementId = (int) $licenseDb->lastInsertId();
$tests[] = $licenseRepository->deletedUsers() === [];
$tests[] = $licenseRepository->registrationInvitations() === [];
$licenseDb->prepare("INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)")
    ->execute([$recreatedUserId, null, 'portal.user_deleted', json_encode(['original_user_id'=>$recreatedUserId,'email'=>'deleted+'.$recreatedUserId.'+123@invalid.local','company_name'=>'Invited Company Europe','first_name'=>'Lars','last_name'=>'Jensen','device_count'=>1,'deleted_by'=>'test-admin'])]);
$tests[] = $licenseRepository->deletedUsers() === [];
$licenseDb->exec("UPDATE ssw_users SET email='deleted+replacement+123@invalid.local',status='deleted' WHERE id=$currentReplacementId");
$tests[] = count($licenseRepository->deletedUsers()) === 1;
$licenseDb->prepare("INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)")
    ->execute([$recreatedUserId + 100, null, 'portal.user_deleted', json_encode(['original_user_id'=>$recreatedUserId + 100,'email'=>'INVITE@example.com','company_name'=>'Older duplicate','first_name'=>'Lars','last_name'=>'Jensen','device_count'=>0,'deleted_by'=>'test-admin'])]);
$tests[] = count($licenseRepository->deletedUsers()) === 1;
$tests[] = $licenseRepository->removeDuplicateDeletedUserEvents() === 1;
$tests[] = $licenseRepository->removeDuplicateDeletedUserEvents() === 0;
$licenseDb->exec("UPDATE ssw_users SET status='' WHERE id=$currentReplacementId");
$tests[] = count(array_filter($licenseRepository->users(), static function (array $row): bool { return strpos((string) $row['email'], 'deleted+') === 0; })) === 0;
$licenseDb->exec("UPDATE ssw_users SET status='deleted' WHERE id=$currentReplacementId");
$archiveWarningRaised = false;
try { $licenseRepository->createRegistrationInvitation('Invited Company A/S', 'invite@example.com', 'test-admin'); } catch (RuntimeException $exception) { $archiveWarningRaised = strpos($exception->getMessage(), 'archivio') !== false; }
$tests[] = $archiveWarningRaised;
$reinvitation = $licenseRepository->createRegistrationInvitation('Invited Company A/S', 'invite@example.com', 'test-admin', true);
$tests[] = $reinvitation['status'] === 'invited';
$licenseTemplate = file_get_contents(dirname(__DIR__) . '/templates/licenses.php');
$portalScript = file_get_contents(dirname(__DIR__) . '/assets/app.js');
$portalIndex = file_get_contents(dirname(__DIR__) . '/index.php');
$tests[] = strpos($licenseTemplate, 'onsubmit=') === false
    && substr_count($licenseTemplate, 'data-confirm=') === 3
    && strpos($licenseTemplate, 'Riscarica email con PIN') !== false
    && strpos($licenseTemplate, 'Rigenera invito download') === false
    && strpos($licenseTemplate, 'Crea e scarica invito .eml') !== false
    && strpos($portalScript, "form.getAttribute('data-confirm')") !== false
    && strpos($portalScript, 'window.confirm(confirmation)') !== false;
$tests[] = strpos($portalIndex, 'refresh an existing installation with the latest software improvements') !== false
    && strpos($portalIndex, 'actualiser une installation existante avec les dernières améliorations') !== false
    && strpos($portalIndex, 'eine bestehende Installation mit den neuesten Verbesserungen zu aktualisieren') !== false;
$expiringInvitation = $licenseRepository->createRegistrationInvitation('Expiry Company', 'expiry@example.com', 'test-admin');
$listedExpiringInvitation = array_values(array_filter($licenseRepository->registrationInvitations(), static function (array $row): bool { return $row['email'] === 'expiry@example.com'; }))[0];
$tests[] = strpos($listedExpiringInvitation['expires_in'], 'Scade tra ') === 0 && substr($listedExpiringInvitation['expires_at'], 0, 10) >= gmdate('Y-m-d');
$licenseDb->exec("UPDATE ssw_registration_invitations SET created_at='2020-01-01 00:00:00' WHERE email='expiry@example.com'");
$expiredInvitationRejected = false;
try { $licenseRepository->registrationInvitation($expiringInvitation['token']); } catch (RuntimeException $exception) { $expiredInvitationRejected = strpos($exception->getMessage(), 'scaduto') !== false; }
$tests[] = $expiredInvitationRejected;
$renewedInvitation = $licenseRepository->createRegistrationInvitation('Expiry Company', 'expiry@example.com', 'test-admin');
$tests[] = $licenseRepository->registrationInvitation($renewedInvitation['token'])['status'] === 'invited';
$licenseDb->exec("UPDATE ssw_registration_invitations SET created_at='2020-01-01 00:00:00' WHERE email='expiry@example.com'");
$expiredAdminInvitation = array_values(array_filter($licenseRepository->registrationInvitations(), static function (array $row): bool { return $row['email'] === 'expiry@example.com'; }))[0];
$tests[] = $expiredAdminInvitation['expired'] === true && strpos($expiredAdminInvitation['expires_in'], 'Scaduto da ') === 0;
$renewedByAction = $licenseRepository->renewRegistrationInvitation((int) $expiredAdminInvitation['id'], 'test-admin');
$tests[] = $renewedByAction['status'] === 'invited';
$removableInvitation = $licenseRepository->createRegistrationInvitation('Remove Company', 'remove@example.com', 'test-admin');
$licenseRepository->revokeRegistrationInvitation((int) $removableInvitation['id'], 'test-admin');
$tests[] = count(array_filter($licenseRepository->registrationInvitations(), static function (array $row): bool { return $row['email'] === 'remove@example.com'; })) === 0;
$licenseDb->exec("INSERT INTO ssw_users (customer_id,email,company_name,first_name,last_name,status) VALUES (1,'expired-pin@example.com','PIN Company','Expired','PIN','pending')");
$expiredPinUserId = (int) $licenseDb->lastInsertId();
$licenseDb->prepare("INSERT INTO ssw_activation_codes (user_id,code_hash,display_code,expires_at) VALUES (?,?,?,'2020-01-01 00:00:00')")
    ->execute([$expiredPinUserId, password_hash('112233', PASSWORD_DEFAULT), '112233']);
$expiredPendingUser = array_values(array_filter($licenseRepository->pendingUsers(), static function (array $row): bool { return $row['email'] === 'expired-pin@example.com'; }))[0];
$tests[] = $expiredPendingUser['activation_expired'] === true && strpos($expiredPendingUser['expires_in'], 'Scaduto da ') === 0;
$licenseRepository->newCode($expiredPinUserId, 'test-admin');
$renewedPendingUser = array_values(array_filter($licenseRepository->pendingUsers(), static function (array $row): bool { return $row['email'] === 'expired-pin@example.com'; }))[0];
$tests[] = $renewedPendingUser['activation_expired'] === false && strpos($renewedPendingUser['expires_in'], 'Scade tra ') === 0;
$licenseDb->exec("INSERT INTO ssw_users (customer_id,email,company_name,first_name,last_name,status) VALUES (1,'update@example.com','Update Company','Update','User','active')");
$updateUserId = (int) $licenseDb->lastInsertId();
$licenseDb->exec("INSERT INTO ssw_installations (customer_id,user_id,installation_uuid,installation_code,device_number,status,software_version) VALUES (1,$updateUserId,'00000000-0000-4000-8000-000000000010','OLD1',1,'active','2.0.0.14')");
$licenseDb->exec("INSERT INTO ssw_installations (customer_id,user_id,installation_uuid,installation_code,device_number,status,software_version) VALUES (1,$updateUserId,'00000000-0000-4000-8000-000000000011','OK15',2,'active','2.0.0.15')");
$updateUsers = $licenseRepository->updateInvitationUsers();
$listedUpdateUser = array_values(array_filter($updateUsers, static function (array $row): bool { return $row['email'] === 'update@example.com'; }));
$tests[] = count($listedUpdateUser) === 1
    && count($listedUpdateUser[0]['outdated_devices']) === 1
    && $listedUpdateUser[0]['outdated_devices'][0]['software_version'] === '2.0.0.14'
    && $listedUpdateUser[0]['minimum_update_version'] === '2.0.0.15';
$uninstallRequest = $licenseRepository->requestUpdateUninstall($updateUserId, 'test-admin');
$tests[] = preg_match('/^[1-9][0-9]*\.[1-9][0-9]*\.[a-f0-9]{64}$/D', $uninstallRequest['token']) === 1
    && preg_match('/^DIS-[A-F0-9]{6}$/', $uninstallRequest['activity_code']) === 1
    && $licenseRepository->updateUninstallRequestForAdmin($updateUserId)['activity_code'] === $uninstallRequest['activity_code']
    && $licenseRepository->updateUninstallConfirmation($uninstallRequest['token'])['confirmed'] === false;
$requestedUpdateUser = array_values(array_filter($licenseRepository->updateInvitationUsers(), static function (array $row): bool { return $row['email'] === 'update@example.com'; }))[0];
$tests[] = $requestedUpdateUser['update_workflow_state'] === 'uninstall_requested';
$confirmedUninstall = $licenseRepository->updateUninstallConfirmation($uninstallRequest['token'], true);
$tests[] = $confirmedUninstall['confirmed'] === true;
$confirmedUpdateUser = array_values(array_filter($licenseRepository->updateInvitationUsers(), static function (array $row): bool { return $row['email'] === 'update@example.com'; }))[0];
$tests[] = $confirmedUpdateUser['update_workflow_state'] === 'uninstall_confirmed';
$reinstallActivityCode = $licenseRepository->markReinstallPrepared($updateUserId, (int) $uninstallRequest['event_id'], 'test-admin');
$reinstallUpdateUser = array_values(array_filter($licenseRepository->updateInvitationUsers(), static function (array $row): bool { return $row['email'] === 'update@example.com'; }))[0];
$tests[] = $reinstallUpdateUser['update_workflow_state'] === 'reinstall_sent'
    && preg_match('/^REI-[A-F0-9]{6}$/', $reinstallActivityCode) === 1
    && $reinstallUpdateUser['activity_code'] === $reinstallActivityCode
    && $reinstallActivityCode !== $uninstallRequest['activity_code'];
$confirmationTemplate = file_get_contents(dirname(__DIR__) . '/templates/uninstall-confirmation.php');
$tests[] = strpos($confirmationTemplate, 'I confirm that SSW has been uninstalled') !== false
    && strpos($confirmationTemplate, 'Je confirme que SSW a été désinstallé') !== false
    && strpos($confirmationTemplate, 'Ich bestätige, dass SSW deinstalliert wurde') !== false
    && strpos($portalIndex, 'Use the PIN only if SSW requests activation on first launch') !== false;
foreach ($tests as $index => $passed) {
    if (!$passed) { fwrite(STDERR, 'Test ' . ($index + 1) . " failed\n"); exit(1); }
}
echo count($tests) . " tests passed\n";
