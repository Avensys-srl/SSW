<?php
declare(strict_types=1);
require __DIR__ . '/technical_selection_service_test.php';

$db = create_database();
$service = new TechnicalSelectionService($db);
$installation = $service->registerInstallation([
    'customer_code' => 'AV', 'installation_id' => 'c1ed4ec5-98ae-4db0-923b-890f8e730301',
    'installation_code' => '7FER',
], 'offer-test-bootstrap', ['AV' => 'offer-test-bootstrap']);
$principal = $service->authenticate($installation['access_token']);
$selection = [
    'project_id' => 'c1ed4ec5-98ae-4db0-923b-890f8e730302',
    'resume_token' => str_repeat('r', 40), 'selection' => ['unit' => 'TEST'],
    'versions' => versions(), 'fingerprints' => fingerprints(),
];
$registered = $service->executeIdempotent($principal, 'selection.create', 'offer-test-create-1', $selection,
    function () use ($service, $principal, $selection) { return $service->createSelection($principal, $selection); });
$reference = $registered['body']['reference_digits'];
$offer = ['status' => 'Provisional', 'revision' => 1, 'resume_token' => $selection['resume_token'],
    'snapshot_hash' => fingerprints()['snapshot_hash']];
assert_true($db->query('SELECT offer_status FROM ssw_selection_revisions')->fetchColumn() === null,
    'Historical/old-client registration must have no offer status.');
$record = function (array $input, string $key) use ($service, $principal, $reference): array {
    return $service->executeIdempotent($principal, 'selection.offer.' . $reference, $key, $input,
        function () use ($service, $principal, $reference, $input) { return $service->recordOffer($principal, $reference, $input); });
};
$provisional = $record($offer, 'offer-test-provisional-1');
assert_true($provisional['body']['status'] === 'Provisional' && $provisional['body']['definitive_at_utc'] === null,
    'Provisional offer started definitive timing.');
$offer['status'] = 'Definitive';
$definitive = $record($offer, 'offer-test-definitive-1');
assert_true($definitive['body']['status'] === 'Definitive' && $definitive['body']['definitive_at_utc'] !== null,
    'Definitive offer was not confirmed.');
assert_true($record($offer, 'offer-test-definitive-1') === $definitive, 'Idempotent confirmation changed.');
assert_true($record($offer, 'offer-test-reprint-1')['body'] === $definitive['body'], 'Reprint reset definitive timing.');
$offer['status'] = 'Provisional';
assert_true($record($offer, 'offer-test-no-downgrade')['body']['status'] === 'Definitive', 'Reprint downgraded the offer.');
assert_true((int) $db->query("SELECT COUNT(*) FROM ssw_audit_events WHERE event_type='selection.offer_recorded'")->fetchColumn() === 2,
    'Replay/reprint duplicated lifecycle events.');
foreach ([['status', 'Invalid', 400], ['resume_token', str_repeat('x', 40), 403],
    ['snapshot_hash', str_repeat('f', 64), 409], ['revision', 2, 409]] as $case) {
    $invalid = $offer; $invalid[$case[0]] = $case[1];
    try { $record($invalid, 'offer-test-invalid-' . $case[0]); throw new RuntimeException('Invalid offer accepted.'); }
    catch (ApiException $ex) { assert_true($ex->status === $case[2], 'Unexpected invalid-offer rejection.'); }
}
$foreign = $principal; $foreign['customer_id'] = 999;
try { $service->recordOffer($foreign, $reference, $offer); throw new RuntimeException('Foreign tenant accepted.'); }
catch (ApiException $ex) { assert_true($ex->status === 404, 'Foreign selection existence leaked.'); }
$revision = $selection; $revision['fingerprints'] = fingerprints('e', 'f', 'a', 'b');
$changed = $service->executeIdempotent($principal, 'selection.revise', 'offer-test-revision-2', $revision,
    function () use ($service, $principal, $reference, $revision) { return $service->createRevision($principal, $reference, $revision); });
assert_true($changed['body']['revision'] === 2 && $db->query('SELECT offer_status FROM ssw_selection_revisions WHERE revision_number=2')->fetchColumn() === null,
    'New technical revision inherited the definitive offer.');
try { $record($offer, 'offer-test-stale'); throw new RuntimeException('Stale revision accepted.'); }
catch (ApiException $ex) { assert_true($ex->status === 409, 'Stale revision was not blocked.'); }
$admin = new TechnicalSelectionAdminService($db);
assert_true($admin->find($reference)['revisions'][1]['offer_status'] === 'Definitive', 'Admin register omitted offer status.');
echo "Offer status tests passed.\n";
