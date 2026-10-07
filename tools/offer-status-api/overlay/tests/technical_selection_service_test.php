<?php
declare(strict_types=1);

require_once dirname(__DIR__) . '/v1/bootstrap.php';
require_once dirname(__DIR__) . '/v1/lib/TechnicalSelectionAdminService.php';

function assert_true(bool $condition, string $message): void
{
    if (!$condition) throw new RuntimeException($message);
}

$originalServer = $_SERVER;
putenv('SSW_SELECTION_TRUSTED_PROXIES=192.168.1.16');
$_SERVER['REMOTE_ADDR'] = '192.168.1.16';
$_SERVER['HTTP_X_FORWARDED_FOR'] = '24.48.0.1:49336';
assert_true(selection_client_ip() === '24.48.0.1', 'Forwarded IPv4 address with port was not normalized.');
$_SERVER['HTTP_X_FORWARDED_FOR'] = '[2001:4860:4860::8888]:443';
assert_true(selection_client_ip() === '2001:4860:4860::8888', 'Forwarded IPv6 address with port was not normalized.');
$_SERVER = $originalServer;
putenv('SSW_SELECTION_TRUSTED_PROXIES');

function create_database(): PDO
{
    $db = new PDO('sqlite::memory:');
    $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    $db->exec(<<<'SQL'
CREATE TABLE ssw_customers (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_code TEXT UNIQUE NOT NULL, display_name TEXT NOT NULL, status TEXT NOT NULL DEFAULT 'active');
CREATE TABLE ssw_installations (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER NOT NULL, user_id INTEGER, installation_uuid TEXT UNIQUE NOT NULL, installation_code TEXT NOT NULL, device_number INTEGER, legacy_license_eligible INTEGER NOT NULL DEFAULT 1, status TEXT NOT NULL DEFAULT 'active', software_version TEXT, database_schema_version INTEGER, database_content_hash TEXT, api_contract_version INTEGER, registered_at TEXT DEFAULT CURRENT_TIMESTAMP, last_seen_at TEXT DEFAULT CURRENT_TIMESTAMP, license_valid_until TEXT, license_last_check_at TEXT, revoked_at TEXT, archived_at TEXT);
CREATE TABLE ssw_installation_tokens (id INTEGER PRIMARY KEY AUTOINCREMENT, installation_id INTEGER NOT NULL, token_hash TEXT UNIQUE NOT NULL, issued_at TEXT DEFAULT CURRENT_TIMESTAMP, expires_at TEXT NOT NULL, revoked_at TEXT);
CREATE TABLE ssw_users (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER NOT NULL, email TEXT UNIQUE NOT NULL, company_name TEXT NOT NULL, first_name TEXT, last_name TEXT, status TEXT NOT NULL DEFAULT 'pending', activated_at TEXT, revoked_at TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP, updated_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_activation_codes (id INTEGER PRIMARY KEY AUTOINCREMENT, user_id INTEGER NOT NULL, code_hash TEXT NOT NULL, attempts INTEGER NOT NULL DEFAULT 0, expires_at TEXT NOT NULL, consumed_at TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_usage_daily (installation_id INTEGER NOT NULL, usage_date TEXT NOT NULL, launch_count INTEGER NOT NULL DEFAULT 0, last_session_uuid TEXT, first_seen_at TEXT DEFAULT CURRENT_TIMESTAMP, last_seen_at TEXT DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (installation_id, usage_date));
CREATE TABLE ssw_license_events (id INTEGER PRIMARY KEY AUTOINCREMENT, user_id INTEGER, installation_id INTEGER, event_type TEXT NOT NULL, event_data TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_selections (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER NOT NULL, created_by_installation_id INTEGER NOT NULL, project_id TEXT NOT NULL, public_reference TEXT UNIQUE NOT NULL, resume_token_hash TEXT, latest_revision INTEGER NOT NULL DEFAULT 0, created_at TEXT DEFAULT CURRENT_TIMESTAMP, updated_at TEXT DEFAULT CURRENT_TIMESTAMP, UNIQUE(customer_id, project_id));
CREATE TABLE ssw_selection_revisions (id INTEGER PRIMARY KEY AUTOINCREMENT, selection_id INTEGER NOT NULL, installation_id INTEGER NOT NULL, revision_number INTEGER NOT NULL, payload_hash TEXT NOT NULL, technical_input_hash TEXT, calculation_output_hash TEXT, calculation_basis_hash TEXT, snapshot_hash TEXT, change_kind TEXT, geo_country_code TEXT, geo_city TEXT, geo_source TEXT, geo_accuracy_km REAL, selection_payload TEXT NOT NULL, software_version TEXT NOT NULL, calculation_engine_version TEXT NOT NULL, database_schema_version INTEGER NOT NULL, database_data_version TEXT, database_content_hash TEXT NOT NULL, selection_format_version INTEGER NOT NULL, report_template_version INTEGER NOT NULL, api_contract_version INTEGER NOT NULL, created_at TEXT DEFAULT CURRENT_TIMESTAMP, UNIQUE(selection_id, revision_number));
CREATE TABLE ssw_geoip_ranges (id INTEGER PRIMARY KEY AUTOINCREMENT, address_family INTEGER NOT NULL, range_start BLOB NOT NULL, range_end BLOB NOT NULL, country_code TEXT NOT NULL, city TEXT, source TEXT NOT NULL, accuracy_km REAL);
CREATE TABLE ssw_multi_projects (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER NOT NULL, created_by_installation_id INTEGER NOT NULL, project_uuid TEXT NOT NULL, project_reference TEXT NOT NULL, language_code TEXT NOT NULL, created_at TEXT DEFAULT CURRENT_TIMESTAMP, updated_at TEXT DEFAULT CURRENT_TIMESTAMP, UNIQUE(customer_id, project_uuid));
CREATE TABLE ssw_multi_project_items (id INTEGER PRIMARY KEY AUTOINCREMENT, multi_project_id INTEGER NOT NULL, selection_id INTEGER, item_uuid TEXT NOT NULL, selection_project_uuid TEXT NOT NULL, position_index INTEGER NOT NULL, selection_revision INTEGER, customer_reference TEXT, unit_name TEXT, airflow_m3h REAL, pressure_pa REAL, pdf_filename TEXT, language_code TEXT NOT NULL, snapshot_hash TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP, updated_at TEXT DEFAULT CURRENT_TIMESTAMP, UNIQUE(multi_project_id, item_uuid), UNIQUE(multi_project_id, selection_project_uuid));
CREATE TABLE ssw_download_events (id INTEGER PRIMARY KEY AUTOINCREMENT, download_source TEXT NOT NULL, selected_filename TEXT NOT NULL, selected_version TEXT, previous_version TEXT, geo_country_code TEXT, geo_city TEXT, geo_source TEXT, geo_accuracy_km REAL, user_agent TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_follow_up_reminders (id INTEGER PRIMARY KEY AUTOINCREMENT, reminder_uuid TEXT NOT NULL, customer_id INTEGER NOT NULL, installation_id INTEGER NOT NULL, target_type TEXT NOT NULL, selection_id INTEGER, multi_project_id INTEGER, target_uuid TEXT NOT NULL, display_reference TEXT NOT NULL, email_prepared_at_utc TEXT NOT NULL, due_at_utc TEXT NOT NULL, status TEXT NOT NULL DEFAULT 'Pending', reschedule_count INTEGER NOT NULL DEFAULT 0, closed_at_utc TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP, updated_at TEXT DEFAULT CURRENT_TIMESTAMP, UNIQUE(installation_id, reminder_uuid));
CREATE INDEX ix_ssw_follow_up_poll ON ssw_follow_up_reminders (installation_id, status, due_at_utc);
CREATE INDEX ix_ssw_follow_up_incremental ON ssw_follow_up_reminders (installation_id, updated_at, id);
CREATE TABLE ssw_follow_up_reminder_events (id INTEGER PRIMARY KEY AUTOINCREMENT, reminder_id INTEGER NOT NULL, customer_id INTEGER NOT NULL, installation_id INTEGER NOT NULL, event_type TEXT NOT NULL, old_due_at_utc TEXT, new_due_at_utc TEXT, old_status TEXT, new_status TEXT NOT NULL, created_at TEXT DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE ssw_api_idempotency (id INTEGER PRIMARY KEY AUTOINCREMENT, installation_id INTEGER NOT NULL, operation_name TEXT NOT NULL, idempotency_key TEXT NOT NULL, request_hash TEXT NOT NULL, response_status INTEGER, response_body TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP, expires_at TEXT NOT NULL, UNIQUE(installation_id, operation_name, idempotency_key));
CREATE TABLE ssw_api_rate_limits (subject_hash TEXT NOT NULL, operation_name TEXT NOT NULL, window_started_at TEXT NOT NULL, request_count INTEGER NOT NULL, expires_at TEXT NOT NULL, PRIMARY KEY(subject_hash, operation_name, window_started_at));
CREATE TABLE ssw_audit_events (id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER, installation_id INTEGER, selection_id INTEGER, event_type TEXT NOT NULL, event_data TEXT, created_at TEXT DEFAULT CURRENT_TIMESTAMP);
SQL);
    $db->exec('ALTER TABLE ssw_selection_revisions ADD COLUMN offer_status TEXT');
    $db->exec('ALTER TABLE ssw_selection_revisions ADD COLUMN offer_generated_at_utc TEXT');
    $db->exec('ALTER TABLE ssw_selection_revisions ADD COLUMN offer_definitive_at_utc TEXT');
    $db->exec("INSERT INTO ssw_customers (customer_code, display_name) VALUES ('AV', 'Avensys')");
    return $db;
}

function versions(): array
{
    return [
        'software_version' => '1.3.0.44',
        'calculation_engine_version' => '1.3.0.44',
        'database_schema_version' => 1,
        'database_data_version' => '2026.07.14',
        'database_content_hash' => 'A280D8C',
        'selection_format_version' => 1,
        'report_template_version' => 1,
        'api_contract_version' => 1,
    ];
}

function fingerprints(string $technical = 'a', string $output = 'b', string $basis = 'c', string $snapshot = 'd'): array
{
    return [
        'technical_input_hash' => str_repeat($technical, 64),
        'calculation_output_hash' => str_repeat($output, 64),
        'calculation_basis_hash' => str_repeat($basis, 64),
        'snapshot_hash' => str_repeat($snapshot, 64),
    ];
}

$references = [];
for ($index = 0; $index < 5000; $index++) {
    $reference = PublicReference::generate();
    assert_true(PublicReference::isValid($reference), 'Generated reference failed checksum validation.');
    assert_true(!isset($references[$reference]), 'Generated reference collision in test sample.');
    $references[$reference] = true;
}

$db = create_database();
$service = new TechnicalSelectionService($db);
$geoInsert = $db->prepare('INSERT INTO ssw_geoip_ranges (address_family, range_start, range_end, country_code, city, source, accuracy_km) VALUES (?, ?, ?, ?, ?, ?, ?)');
$geoInsert->bindValue(1, 4, PDO::PARAM_INT);
$geoInsert->bindValue(2, str_repeat("\0", 12) . inet_pton('203.0.113.0'), PDO::PARAM_LOB);
$geoInsert->bindValue(3, str_repeat("\0", 12) . inet_pton('203.0.113.255'), PDO::PARAM_LOB);
$geoInsert->bindValue(4, 'IT');
$geoInsert->bindValue(5, 'Napoli');
$geoInsert->bindValue(6, 'test-local');
$geoInsert->bindValue(7, 50.0);
$geoInsert->execute();
$geolocation = (new GeoIpResolver($db))->resolve('203.0.113.42');
assert_true($geolocation !== null && $geolocation['country_code'] === 'IT' && $geolocation['city'] === 'Napoli',
    'Local GeoIP lookup failed.');
$registration = $service->registerInstallation([
    'customer_code' => 'AV',
    'installation_id' => 'f6b315b9-dfa2-4e8b-b609-c228ad1fd278',
    'installation_code' => '7K3P',
    'software_version' => '1.3.0.44',
    'database_schema_version' => 1,
    'database_content_hash' => 'A280D8C',
    'api_contract_version' => 1,
], 'test-bootstrap-secret', ['AV' => 'test-bootstrap-secret']);
assert_true(strlen($registration['access_token']) >= 40, 'Registration did not return a strong token.');
$storedHash = $db->query('SELECT token_hash FROM ssw_installation_tokens')->fetchColumn();
assert_true($storedHash === hash('sha256', $registration['access_token']), 'Server did not persist the expected token hash.');
assert_true($storedHash !== $registration['access_token'], 'Raw token was persisted.');

$publicRegistration = $service->registerInstallation([
    'customer_code' => 'AV',
    'installation_id' => '870282b5-c517-44e7-b149-d970484f225b',
    'installation_code' => '8M4R',
    'software_version' => '1.3.0.50',
    'database_schema_version' => 1,
    'database_content_hash' => 'PUBLIC-ENROLLMENT',
    'api_contract_version' => 1,
], '', ['AV' => 'test-bootstrap-secret'], 'AV');
assert_true(strlen($publicRegistration['access_token']) >= 40,
    'Public AV installation registration did not return a strong token.');

$otherCustomerRejected = false;
try {
    $service->registerInstallation([
        'customer_code' => 'OTHER',
        'installation_id' => '19b4c9ab-a53c-4910-8f22-a4bb06268320',
        'installation_code' => '9N5T',
    ], '', ['AV' => 'test-bootstrap-secret'], 'AV');
} catch (ApiException $exception) {
    $otherCustomerRejected = $exception->status === 401
        && $exception->errorCode === 'invalid_bootstrap_credentials';
}
assert_true($otherCustomerRejected, 'Public enrollment was not restricted to AV.');

$principal = $service->authenticate($registration['access_token']);
$resumeToken = 'resume-token-with-at-least-thirty-two-characters';
$request = [
    'project_id' => '4596c7ee-98bd-4a1f-934e-c4b1a471e60c',
    'resume_token' => $resumeToken,
    'selection' => ['unit' => 'CLRC 06A OSC', 'airflow' => 100],
    'versions' => versions(),
    'fingerprints' => fingerprints(),
];
$first = $service->executeIdempotent($principal, 'selection.create', 'create-4596c7ee-98bd-4a1f', $request,
    function () use ($service, $principal, $request, $geolocation) {
        return $service->createSelection($principal, $request, $geolocation);
    });
$retry = $service->executeIdempotent($principal, 'selection.create', 'create-4596c7ee-98bd-4a1f', $request,
    function () use ($service, $principal, $request) {
        return $service->createSelection($principal, $request);
    });
assert_true($first === $retry, 'Idempotent retry returned a different response.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_selections')->fetchColumn() === 1, 'Retry created a duplicate selection.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_selection_revisions')->fetchColumn() === 1, 'Retry created a duplicate initial revision.');
$storedLocation = $db->query('SELECT geo_country_code, geo_city, geo_source, geo_accuracy_km FROM ssw_selection_revisions')->fetch(PDO::FETCH_ASSOC);
assert_true($storedLocation['geo_country_code'] === 'IT' && $storedLocation['geo_city'] === 'Napoli'
    && $storedLocation['geo_source'] === 'test-local' && (float) $storedLocation['geo_accuracy_km'] === 50.0,
    'Approximate geolocation was not stored on the revision.');
assert_true(preg_match('/^[0-9]{4}(?:-[0-9]{4}){3}-R01$/D', $first['body']['reference']) === 1, 'Public reference format is invalid.');

$reference = $first['body']['reference_digits'];
$multiProjectRequest = [
    'reference' => 'Progetto 01', 'language_code' => 'it',
    'items' => [[
        'item_id' => 'bd0a3307-da02-4969-9668-5401e0a457d2',
        'selection_project_id' => $request['project_id'], 'customer_reference' => 'Piano terra',
        'unit_name' => 'CLRC 06A OSC', 'airflow_m3h' => 100, 'pressure_pa' => 473,
        'pdf_filename' => 'Piano_terra.pdf', 'language_code' => 'it',
        'snapshot_hash' => str_repeat('d', 64),
    ]],
];
$projectSync = $service->executeIdempotent($principal, 'project.sync.67fc3026-c0b9-44fc-bb30-0cc0c1778497',
    'project-sync-67fc3026-c0b9-44fc', $multiProjectRequest,
    function () use ($service, $principal, $multiProjectRequest) {
        return $service->syncMultiProject($principal, '67fc3026-c0b9-44fc-bb30-0cc0c1778497', $multiProjectRequest);
    });
assert_true($projectSync['status'] === 201 && $projectSync['body']['items'] === 1,
    'Multi-selection project was not created.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_multi_project_items WHERE selection_id IS NOT NULL')->fetchColumn() === 1,
    'Multi-selection project item was not linked to its registered selection.');
$multiProjectRequest['reference'] = 'Progetto aggiornato';
$updatedProject = $service->syncMultiProject($principal, '67fc3026-c0b9-44fc-bb30-0cc0c1778497', $multiProjectRequest);
assert_true($updatedProject['status'] === 200
    && $db->query('SELECT project_reference FROM ssw_multi_projects')->fetchColumn() === 'Progetto aggiornato'
    && (int) $db->query('SELECT COUNT(*) FROM ssw_multi_project_items')->fetchColumn() === 1,
    'Multi-selection project replacement failed.');

$otherInstallationPrincipal = $service->authenticate($publicRegistration['access_token']);
$now = time();
$db->exec("INSERT INTO ssw_customers (customer_code, display_name) VALUES ('OTHER', 'Other customer')");
$foreignRegistration = $service->registerInstallation([
    'customer_code' => 'OTHER',
    'installation_id' => '981943ba-edf3-42a5-a780-d453d3c85856',
    'installation_code' => '6P7T',
], 'other-bootstrap-secret', ['OTHER' => 'other-bootstrap-secret']);
$foreignPrincipal = $service->authenticate($foreignRegistration['access_token']);
$foreignProjectUuid = '7c381e79-cbd1-4d21-91d3-ae8804d0bd77';
$foreignReference = PublicReference::generate();
$db->prepare('INSERT INTO ssw_selections '
    . '(customer_id, created_by_installation_id, project_id, public_reference, resume_token_hash, latest_revision) '
    . 'VALUES (?, ?, ?, ?, ?, 1)')->execute([
        $foreignPrincipal['customer_id'], $foreignPrincipal['installation_id'], $foreignProjectUuid,
        $foreignReference, hash('sha256', 'foreign-resume-token-with-at-least-thirty-two-characters'),
    ]);
$followUpRequest = [
    'reminder_id' => '41d49ec5-98ae-4db0-923b-890f8e730301',
    'target_type' => 'Selection',
    'target_id' => $request['project_id'],
    'registered_reference' => $first['body']['reference'],
    'display_reference' => 'Piano terra',
    'email_prepared_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now),
    'due_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now + 7 * 86400),
    'follow_up_days' => 7,
];
$followCreate = $service->executeIdempotent($principal, 'follow-up.create',
    'follow-create-41d49ec5-98ae-4db0', $followUpRequest,
    function () use ($service, $principal, $followUpRequest) {
        return $service->createFollowUp($principal, $followUpRequest);
    });
$followReplay = $service->executeIdempotent($principal, 'follow-up.create',
    'follow-create-41d49ec5-98ae-4db0', $followUpRequest,
    function () use ($service, $principal, $followUpRequest) {
        return $service->createFollowUp($principal, $followUpRequest);
    });
assert_true($followCreate === $followReplay && $followCreate['status'] === 201,
    'Follow-up create replay returned a different response.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_follow_up_reminders')->fetchColumn() === 1
    && (int) $db->query('SELECT COUNT(*) FROM ssw_follow_up_reminder_events')->fetchColumn() === 1,
    'Follow-up create replay produced duplicate rows or events.');
assert_true((int) $db->query('SELECT selection_id FROM ssw_follow_up_reminders')->fetchColumn() > 0,
    'Registered selection was not linked to its follow-up reminder.');
assert_true($service->listFollowUps($principal, ['scope' => 'active'])['reminders'][0]['display_reference'] === 'Piano terra',
    'Owning installation could not list its active follow-up reminder.');
assert_true(count($service->listFollowUps($otherInstallationPrincipal, ['scope' => 'all'])['reminders']) === 0,
    'Another active installation could list a private follow-up reminder.');

$foreignTargetErrors = [];
foreach ([$foreignProjectUuid, '8d492f80-dce2-4e32-a2e4-bf9915e1ce88'] as $foreignTargetUuid) {
    try {
        $foreignTargetRequest = $followUpRequest;
        $foreignTargetRequest['reminder_id'] = $foreignTargetUuid === $foreignProjectUuid
            ? '96c943ba-edf3-43a5-a780-d453d3c85856' : 'a7da54cb-fe04-44b6-b891-e564e4d96967';
        $foreignTargetRequest['target_id'] = $foreignTargetUuid;
        $foreignTargetRequest['registered_reference'] = $foreignReference;
        $service->createFollowUp($principal, $foreignTargetRequest);
        throw new RuntimeException('A foreign or missing registered follow-up target was accepted.');
    } catch (ApiException $exception) {
        $foreignTargetErrors[] = [$exception->status, $exception->errorCode, $exception->getMessage()];
    }
}
assert_true($foreignTargetErrors[0] === $foreignTargetErrors[1]
    && $foreignTargetErrors[0][0] === 404 && $foreignTargetErrors[0][1] === 'target_not_found',
    'Foreign target rejection revealed whether the target exists.');

try {
    $service->rescheduleFollowUp($otherInstallationPrincipal, $followUpRequest['reminder_id'], [
        'rescheduled_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now),
        'due_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now + 7 * 86400),
        'follow_up_days' => 7,
    ]);
    throw new RuntimeException('Another installation rescheduled a private follow-up reminder.');
} catch (ApiException $exception) {
    assert_true($exception->status === 404 && $exception->errorCode === 'reminder_not_found',
        'Private reminder mutation exposed an unexpected response.');
}

$rescheduleRequest = [
    'rescheduled_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now),
    'due_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now + 14 * 86400),
    'follow_up_days' => 14,
];
$reschedule = $service->executeIdempotent($principal,
    'follow-up.reschedule.' . $followUpRequest['reminder_id'], 'follow-reschedule-41d49ec5-98ae', $rescheduleRequest,
    function () use ($service, $principal, $followUpRequest, $rescheduleRequest) {
        return $service->rescheduleFollowUp($principal, $followUpRequest['reminder_id'], $rescheduleRequest);
    });
$rescheduleReplay = $service->executeIdempotent($principal,
    'follow-up.reschedule.' . $followUpRequest['reminder_id'], 'follow-reschedule-41d49ec5-98ae', $rescheduleRequest,
    function () use ($service, $principal, $followUpRequest, $rescheduleRequest) {
        return $service->rescheduleFollowUp($principal, $followUpRequest['reminder_id'], $rescheduleRequest);
    });
assert_true($reschedule === $rescheduleReplay
    && $reschedule['body']['reminder']['reschedule_count'] === 1
    && (int) $db->query("SELECT COUNT(*) FROM ssw_follow_up_reminder_events WHERE event_type = 'Rescheduled'")->fetchColumn() === 1,
    'Follow-up reschedule was not idempotent.');
try {
    $conflictingReschedule = $rescheduleRequest;
    $conflictingReschedule['due_at_utc'] = gmdate('Y-m-d\TH:i:s\Z', $now + 15 * 86400);
    $conflictingReschedule['follow_up_days'] = 15;
    $service->executeIdempotent($principal, 'follow-up.reschedule.' . $followUpRequest['reminder_id'],
        'follow-reschedule-41d49ec5-98ae', $conflictingReschedule,
        function () use ($service, $principal, $followUpRequest, $conflictingReschedule) {
            return $service->rescheduleFollowUp($principal, $followUpRequest['reminder_id'], $conflictingReschedule);
        });
    throw new RuntimeException('A follow-up idempotency key was reused with another payload.');
} catch (ApiException $exception) {
    assert_true($exception->status === 409 && $exception->errorCode === 'idempotency_conflict',
        'Follow-up idempotency conflict returned the wrong response.');
}

$dueReminder = [
    'reminder_id' => '52e50fd6-a9bf-4ec1-a34c-901f9f841412',
    'target_type' => 'Selection',
    'target_id' => '6b270d68-bac0-4c10-80c2-fd7793c9ac66',
    'display_reference' => 'Sollecito scaduto',
    'email_prepared_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now - 2 * 86400),
    'due_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now - 86400),
    'follow_up_days' => 1,
];
$service->createFollowUp($principal, $dueReminder);
assert_true(count($service->listFollowUps($principal, ['scope' => 'due'])['reminders']) === 1,
    'Due follow-up filtering failed.');

$projectReminder = [
    'reminder_id' => '63f610e7-bac0-4fd2-b45d-a120a0952523',
    'target_type' => 'Project',
    'target_id' => '67fc3026-c0b9-44fc-bb30-0cc0c1778497',
    'registered_reference' => 'Progetto aggiornato',
    'display_reference' => 'Progetto aggiornato',
    'email_prepared_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now),
    'due_at_utc' => gmdate('Y-m-d\TH:i:s\Z', $now + 90 * 86400),
    'follow_up_days' => 90,
];
$service->createFollowUp($principal, $projectReminder);
assert_true((int) $db->query("SELECT COUNT(*) FROM ssw_follow_up_reminders WHERE multi_project_id IS NOT NULL")->fetchColumn() === 1,
    'Registered multi-selection project was not linked to its follow-up reminder.');

foreach ([
    [$followUpRequest['reminder_id'], 'Succeeded', 'follow-close-succeeded-41d49ec5'],
    [$dueReminder['reminder_id'], 'Unsuccessful', 'follow-close-unsuccessful-52e50fd6'],
    [$projectReminder['reminder_id'], 'Cancelled', 'follow-close-cancelled-63f610e7'],
] as $closeCase) {
    $closeRequest = ['status' => $closeCase[1]];
    $closed = $service->executeIdempotent($principal, 'follow-up.close.' . $closeCase[0], $closeCase[2], $closeRequest,
        function () use ($service, $principal, $closeCase, $closeRequest) {
            return $service->closeFollowUp($principal, $closeCase[0], $closeRequest);
        });
    $closedReplay = $service->executeIdempotent($principal, 'follow-up.close.' . $closeCase[0], $closeCase[2], $closeRequest,
        function () use ($service, $principal, $closeCase, $closeRequest) {
            return $service->closeFollowUp($principal, $closeCase[0], $closeRequest);
        });
    assert_true($closed === $closedReplay && $closed['body']['reminder']['status'] === $closeCase[1],
        'Follow-up closure outcome or replay failed for ' . $closeCase[1] . '.');
}
assert_true(count($service->listFollowUps($principal, ['scope' => 'active'])['reminders']) === 0
    && count($service->listFollowUps($principal, ['scope' => 'closed'])['reminders']) === 3
    && (int) $db->query("SELECT COUNT(*) FROM ssw_follow_up_reminder_events WHERE event_type = 'Closed'")->fetchColumn() === 3,
    'Closed follow-up filtering or event history failed.');
$firstPage = $service->listFollowUps($principal, ['scope' => 'all', 'limit' => '1']);
$secondPage = $service->listFollowUps($principal, [
    'scope' => 'all',
    'limit' => '1',
    'updated_since' => $firstPage['next_updated_since'],
    'updated_after_id' => (string) $firstPage['next_updated_after_id'],
]);
assert_true($firstPage['has_more'] === true
    && count($secondPage['reminders']) === 1
    && $firstPage['reminders'][0]['reminder_id'] !== $secondPage['reminders'][0]['reminder_id'],
    'Bounded incremental follow-up pagination did not advance.');

foreach ([0, 91] as $invalidDays) {
    try {
        $invalidFollowUp = $projectReminder;
        $invalidFollowUp['reminder_id'] = $invalidDays === 0
            ? '74a721f8-cbd1-40e3-856e-b231b1a63634' : '85b832a9-dce2-41f4-967f-c342c2b74745';
        $invalidFollowUp['follow_up_days'] = $invalidDays;
        $service->createFollowUp($principal, $invalidFollowUp);
        throw new RuntimeException('An out-of-range follow-up delay was accepted.');
    } catch (ApiException $exception) {
        assert_true($exception->status === 400, 'An out-of-range follow-up delay returned the wrong status.');
    }
}
try {
    $service->listFollowUps($principal, ['scope' => 'active', 'updated_since' => 'not-a-date']);
    throw new RuntimeException('A malformed follow-up timestamp was accepted.');
} catch (ApiException $exception) {
    assert_true($exception->status === 400 && $exception->errorCode === 'invalid_timestamp',
        'Malformed follow-up timestamp returned the wrong response.');
}
try {
    $service->listFollowUps($principal, ['scope' => 'all', 'limit' => '201']);
    throw new RuntimeException('An oversized follow-up list limit was accepted.');
} catch (ApiException $exception) {
    assert_true($exception->status === 400 && $exception->errorCode === 'invalid_limit',
        'Oversized follow-up list limit returned the wrong response.');
}
try {
    $service->closeFollowUp($principal, $followUpRequest['reminder_id'], ['status' => 'Deleted']);
    throw new RuntimeException('An invalid follow-up closure outcome was accepted.');
} catch (ApiException $exception) {
    assert_true($exception->status === 400, 'Invalid follow-up closure outcome returned the wrong response.');
}

assert_true($first['body']['resume_token'] === $resumeToken, 'Create response changed the project resume token.');
assert_true(strlen($resumeToken) >= 40, 'Selection resume token is too weak.');
assert_true($db->query('SELECT resume_token_hash FROM ssw_selections')->fetchColumn() === hash('sha256', $resumeToken),
    'Selection resume token was not stored as a hash.');
$recoveredCreate = $service->createSelection($principal, $request);
assert_true($recoveredCreate['status'] === 200 && $recoveredCreate['body']['revision'] === 1
    && $recoveredCreate['body']['change_kind'] === 'Reprint', 'Create recovery after idempotency expiry failed.');
$reprintRequest = ['selection' => ['airflow' => 100, 'unit' => 'CLRC 06A OSC'], 'versions' => versions(),
    'fingerprints' => fingerprints(), 'resume_token' => $resumeToken];
$reprint = $service->executeIdempotent($principal, 'selection.revise.' . $reference, 'reprint-4596c7ee-98bd-4a1f', $reprintRequest,
    function () use ($service, $principal, $reference, $reprintRequest) {
        return $service->createRevision($principal, $reference, $reprintRequest);
    });
assert_true($reprint['status'] === 200 && $reprint['body']['revision'] === 1 && $reprint['body']['change_kind'] === 'Reprint', 'Unchanged snapshot consumed a revision.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_selection_revisions')->fetchColumn() === 1, 'Reprint created a revision row.');

$revisionRequest = ['selection' => ['airflow' => 120, 'unit' => 'CLRC 06A OSC'], 'versions' => versions(),
    'fingerprints' => fingerprints('e', 'b', 'f', '1'), 'resume_token' => $resumeToken];
$revision = $service->executeIdempotent($principal, 'selection.revise.' . $reference, 'revise-4596c7ee-98bd-4a1f', $revisionRequest,
    function () use ($service, $principal, $reference, $revisionRequest) {
        return $service->createRevision($principal, $reference, $revisionRequest);
    });
$revisionRetry = $service->executeIdempotent($principal, 'selection.revise.' . $reference, 'revise-4596c7ee-98bd-4a1f', $revisionRequest,
    function () use ($service, $principal, $reference, $revisionRequest) {
        return $service->createRevision($principal, $reference, $revisionRequest);
    });
assert_true($revision === $revisionRetry, 'Revision retry returned a different response.');
assert_true($revision['body']['revision'] === 2, 'Second revision number is invalid.');
assert_true($revision['body']['change_kind'] === 'TechnicalChange', 'Technical change was classified incorrectly.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_selection_revisions')->fetchColumn() === 2, 'Revision retry created a duplicate.');

$outputRequest = ['selection' => $revisionRequest['selection'], 'versions' => versions(),
    'fingerprints' => fingerprints('e', 'c', 'f', '2'), 'resume_token' => $resumeToken];
$outputRevision = $service->createRevision($principal, $reference, $outputRequest);
assert_true($outputRevision['body']['revision'] === 3
    && $outputRevision['body']['change_kind'] === 'CalculationResultChange', 'Result-only change was classified incorrectly.');

$databaseVersions = versions();
$databaseVersions['database_data_version'] = '2026.07.15';
$databaseVersions['database_content_hash'] = 'B390E9D';
$databaseRequest = ['selection' => $revisionRequest['selection'], 'versions' => $databaseVersions,
    'fingerprints' => fingerprints('e', 'c', '3', '4'), 'resume_token' => $resumeToken];
$databaseRevision = $service->createRevision($principal, $reference, $databaseRequest);
assert_true($databaseRevision['body']['revision'] === 4
    && $databaseRevision['body']['change_kind'] === 'DatabaseChange', 'Database change was classified incorrectly.');

$algorithmVersions = $databaseVersions;
$algorithmVersions['calculation_engine_version'] = '1.3.0.45';
$algorithmRequest = ['selection' => $revisionRequest['selection'], 'versions' => $algorithmVersions,
    'fingerprints' => fingerprints('e', 'c', '5', '6'), 'resume_token' => $resumeToken];
$algorithmRevision = $service->createRevision($principal, $reference, $algorithmRequest);
assert_true($algorithmRevision['body']['revision'] === 5
    && $algorithmRevision['body']['change_kind'] === 'AlgorithmChange', 'Algorithm change was classified incorrectly.');
$reportVersions = $algorithmVersions;
$reportVersions['report_template_version'] = 2;
$reportRequest = ['selection' => $revisionRequest['selection'], 'versions' => $reportVersions,
    'fingerprints' => fingerprints('e', 'c', '7', '8'), 'resume_token' => $resumeToken];
$reportRevision = $service->createRevision($principal, $reference, $reportRequest);
assert_true($reportRevision['body']['revision'] === 6
    && $reportRevision['body']['change_kind'] === 'TechnicalChange', 'Report template change was classified incorrectly.');
assert_true((int) $db->query('SELECT COUNT(*) FROM ssw_selection_revisions')->fetchColumn() === 6,
    'Classification tests produced an unexpected revision count.');

$transferRegistration = $service->registerInstallation([
    'customer_code' => 'AV',
    'installation_id' => '346ea706-a012-47ef-8302-2c0ffdb575df',
    'installation_code' => '9M4R',
    'software_version' => '1.3.0.44',
    'database_schema_version' => 1,
    'database_content_hash' => 'A280D8C',
    'api_contract_version' => 1,
], 'test-bootstrap-secret', ['AV' => 'test-bootstrap-secret']);
$transferPrincipal = $service->authenticate($transferRegistration['access_token']);
$transferReprint = $service->createRevision($transferPrincipal, $reference, $reportRequest);
assert_true($transferReprint['status'] === 200 && $transferReprint['body']['revision'] === 6,
    'Transferred project could not resume from another customer installation.');

try {
    $invalidResumeRequest = $reportRequest;
    $invalidResumeRequest['resume_token'] = str_repeat('x', 43);
    $service->createRevision($transferPrincipal, $reference, $invalidResumeRequest);
    throw new RuntimeException('A revision with an invalid resume token was accepted.');
} catch (ApiException $exception) {
    assert_true($exception->status === 403 && $exception->errorCode === 'invalid_resume_token',
        'Invalid resume token failed with an unexpected response.');
}

try {
    $conflict = $request;
    $conflict['selection']['airflow'] = 999;
    $service->executeIdempotent($principal, 'selection.create', 'create-4596c7ee-98bd-4a1f', $conflict,
        function () use ($service, $principal, $conflict) {
            return $service->createSelection($principal, $conflict);
        });
    throw new RuntimeException('Reused idempotency key with another payload was accepted.');
} catch (ApiException $exception) {
    assert_true($exception->status === 409 && $exception->errorCode === 'idempotency_conflict', 'Wrong idempotency conflict response.');
}

$renewed = $service->renewToken($principal);
try {
    $service->authenticate($registration['access_token']);
    throw new RuntimeException('Old token remained valid after renewal.');
} catch (ApiException $exception) {
    assert_true($exception->status === 401, 'Old token failed with an unexpected status.');
}
assert_true($service->authenticate($renewed['access_token'])['installation_id'] === $principal['installation_id'], 'Renewed token is invalid.');

$adminService = new TechnicalSelectionAdminService($db);
$adminSelection = $adminService->find(PublicReference::display($reference, 6));
assert_true($adminSelection !== null && (int) $adminSelection['latest_revision'] === 6
    && count($adminSelection['revisions']) === 6, 'Admin selection lookup failed.');
$adminExport = $adminService->exportRevision($reference, 6);
assert_true($adminExport !== null && $adminExport['metadata']['snapshot_hash'] === str_repeat('8', 64)
    && $adminExport['selection']['unit'] === 'CLRC 06A OSC', 'Admin revision export failed.');
$adminService->audit('admin.selection_downloaded', 'test-admin', $reference, 6);
assert_true((int) $db->query("SELECT COUNT(*) FROM ssw_audit_events WHERE event_type = 'admin.selection_downloaded'")->fetchColumn() === 1,
    'Admin download audit was not recorded.');

fwrite(STDOUT, "Technical selection API service tests passed.\n");
