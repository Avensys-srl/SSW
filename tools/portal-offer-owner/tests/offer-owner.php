<?php
declare(strict_types=1);
require_once dirname(__DIR__) . '/src/SelectionPresenter.php';
$cases = [
    [['license_first_name' => 'Anna', 'license_last_name' => 'Rossi', 'license_email' => 'anna@example.test', 'display_name' => 'Company'], 'Anna Rossi'],
    [['license_email' => 'anna@example.test', 'display_name' => 'Company'], 'anna@example.test'],
    [['display_name' => 'Historical company'], 'Historical company'],
    [[], '-'],
];
foreach ($cases as [$item, $expected]) {
    if (SelectionPresenter::offerOwner($item) !== $expected) throw new RuntimeException('Owner fallback failed');
}
echo "Offer owner: licensed name, email fallback and historical fallback passed.\n";
require_once dirname(__DIR__) . '/src/SelectionRepository.php';
$db = new PDO('sqlite::memory:');
$db->exec('CREATE TABLE ssw_customers (id INTEGER, customer_code TEXT, display_name TEXT);
CREATE TABLE ssw_users (id INTEGER, first_name TEXT, last_name TEXT, email TEXT);
CREATE TABLE ssw_installations (id INTEGER, installation_code TEXT, user_id INTEGER);
CREATE TABLE ssw_selections (id INTEGER, public_reference TEXT, project_id TEXT, latest_revision INTEGER, created_at TEXT, updated_at TEXT, customer_id INTEGER, created_by_installation_id INTEGER);
CREATE TABLE ssw_selection_revisions (selection_id INTEGER, revision_number INTEGER, installation_id INTEGER, selection_payload TEXT, software_version TEXT, change_kind TEXT, geo_country_code TEXT, geo_city TEXT, created_at TEXT);');
$db->exec("INSERT INTO ssw_customers VALUES (1,'AV','Historical company');
INSERT INTO ssw_users VALUES (3,'Anna','Rossi','anna@example.test');
INSERT INTO ssw_installations VALUES (1,'legacy',NULL),(2,'licensed',3);
INSERT INTO ssw_selections VALUES (1,'1111222233334444','project',2,'2026-10-06','2026-10-06',1,1),(2,'5555666677778888','old-project',1,'2026-10-06','2026-10-06',1,1);
INSERT INTO ssw_selection_revisions VALUES (1,2,2,'{}','2.0.0.24','revise',NULL,NULL,'2026-10-06'),(2,1,1,'{}','2.0.0.22','create',NULL,NULL,'2026-10-05');");
$repository = new SelectionRepository($db);
$licensed = $repository->search(['q' => 'Rossi', 'include_unlocated' => true], 1, 30);
if ($licensed['total'] !== 1 || count($licensed['items']) !== 1 || SelectionPresenter::offerOwner($licensed['items'][0]) !== 'Anna Rossi') {
    throw new RuntimeException('Latest-revision owner join or count/search failed');
}
$all = $repository->search(['include_unlocated' => true], 1, 30);
if ($all['total'] !== 2 || SelectionPresenter::offerOwner($all['items'][1]) !== 'Historical company') throw new RuntimeException('Legacy row was lost');
if ($repository->search(['include_unlocated' => false], 1, 30)['total'] !== 0) throw new RuntimeException('Location filter regression');
echo "Offer owner SQL: latest revision attribution, search/count parity, legacy retention and location filter passed.\n";
