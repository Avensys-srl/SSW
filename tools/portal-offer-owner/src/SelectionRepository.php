<?php
declare(strict_types=1);

final class SelectionRepository
{
    private $db;

    public function __construct(PDO $db)
    {
        $this->db = $db;
        $this->db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $this->db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
    }

    public function stats(bool $includeUnlocated = false): array
    {
        $selectionWhere = $includeUnlocated ? '' : ' WHERE ' . self::locationCondition('r');
        $row = $this->db->query(
            "SELECT COUNT(*) AS selections, COALESCE(SUM(latest_revision), 0) AS revisions, "
            . "SUM(s.created_at >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY)) AS recent "
            . 'FROM ssw_selections s JOIN ssw_selection_revisions r '
            . 'ON r.selection_id = s.id AND r.revision_number = s.latest_revision'
            . $selectionWhere
        )->fetch();
        $row['customers'] = (int) $this->db->query("SELECT COUNT(*) FROM ssw_customers WHERE status = 'active'")->fetchColumn();
        $row['installations'] = (int) $this->db->query("SELECT COUNT(*) FROM ssw_installations WHERE status = 'active'")->fetchColumn();
        $projectWhere = $includeUnlocated ? '' : ' WHERE ' . self::projectHasLocationCondition('p');
        $row['projects'] = (int) $this->db->query('SELECT COUNT(*) FROM ssw_multi_projects p' . $projectWhere)->fetchColumn();
        $downloadWhere = $includeUnlocated ? '' : ' WHERE ' . self::locationCondition('d');
        $row['downloads'] = (int) $this->db->query('SELECT COUNT(*) FROM ssw_download_events d' . $downloadWhere)->fetchColumn();
        return $row;
    }

    public function projects(bool $includeUnlocated = false): array
    {
        $where = $includeUnlocated ? '' : ' WHERE ' . self::projectHasLocationCondition('p');
        return $this->db->query('SELECT p.project_uuid, p.project_reference, p.language_code, p.created_at, p.updated_at, '
            . 'c.customer_code, c.display_name, COUNT(i.id) AS item_count, SUM(i.selection_id IS NOT NULL) AS linked_count, '
            . "(SELECT GROUP_CONCAT(DISTINCT TRIM(CONCAT_WS(' ', NULLIF(lr.geo_country_code, ''), NULLIF(lr.geo_city, ''))) SEPARATOR ', ') "
            . 'FROM ssw_multi_project_items li JOIN ssw_selections ls ON ls.id = li.selection_id '
            . 'JOIN ssw_selection_revisions lr ON lr.selection_id = ls.id AND lr.revision_number = ls.latest_revision '
            . 'WHERE li.multi_project_id = p.id AND ' . self::locationCondition('lr') . ') AS locations '
            . 'FROM ssw_multi_projects p JOIN ssw_customers c ON c.id = p.customer_id '
            . 'LEFT JOIN ssw_multi_project_items i ON i.multi_project_id = p.id '
            . $where . ' GROUP BY p.id ORDER BY p.updated_at DESC')->fetchAll();
    }

    public function projectDetail(string $uuid): ?array
    {
        if (!preg_match('/^[0-9a-fA-F-]{36}$/D', $uuid)) return null;
        $statement = $this->db->prepare('SELECT p.*, c.customer_code, c.display_name, i.installation_code '
            . 'FROM ssw_multi_projects p JOIN ssw_customers c ON c.id = p.customer_id '
            . 'JOIN ssw_installations i ON i.id = p.created_by_installation_id WHERE p.project_uuid = ?');
        $statement->execute([strtolower($uuid)]);
        $project = $statement->fetch();
        if (!is_array($project)) return null;
        $items = $this->db->prepare('SELECT pi.*, s.public_reference, s.latest_revision '
            . 'FROM ssw_multi_project_items pi LEFT JOIN ssw_selections s ON s.id = pi.selection_id '
            . 'WHERE pi.multi_project_id = ? ORDER BY pi.position_index');
        $items->execute([(int) $project['id']]);
        $project['items'] = $items->fetchAll();
        return $project;
    }

    public function downloadSummary(bool $includeUnlocated = false): array
    {
        $where = $includeUnlocated ? '' : ' WHERE ' . self::locationCondition('d');
        $totals = $this->db->query("SELECT COUNT(*) AS total, SUM(download_source = 'update') AS updates, "
            . "SUM(download_source = 'first_download') AS first_downloads, "
            . 'SUM(created_at >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 30 DAY)) AS recent FROM ssw_download_events d' . $where)->fetch();
        $totals['countries'] = $this->db->query('SELECT geo_country_code, COUNT(*) AS event_count '
            . 'FROM ssw_download_events d WHERE geo_country_code IS NOT NULL'
            . ($includeUnlocated ? '' : ' AND ' . self::locationCondition('d')) . ' GROUP BY geo_country_code '
            . 'ORDER BY event_count DESC LIMIT 12')->fetchAll();
        $totals['versions'] = $this->db->query('SELECT selected_version, COUNT(*) AS event_count '
            . 'FROM ssw_download_events d WHERE selected_version IS NOT NULL'
            . ($includeUnlocated ? '' : ' AND ' . self::locationCondition('d')) . ' GROUP BY selected_version '
            . 'ORDER BY MAX(created_at) DESC LIMIT 12')->fetchAll();
        return $totals;
    }

    public function downloads(int $limit = 200, bool $includeUnlocated = false): array
    {
        $where = $includeUnlocated ? '' : ' WHERE ' . self::locationCondition('d');
        return $this->db->query('SELECT d.*, u.email AS download_email, i.device_number AS download_device_number FROM ssw_download_events d LEFT JOIN ssw_users u ON u.id=d.user_id LEFT JOIN ssw_installations i ON i.id=d.installation_id' . $where
            . ' ORDER BY created_at DESC LIMIT ' . max(1, min(500, $limit)))->fetchAll();
    }

    public function filters(): array
    {
        return [
            'customers' => $this->db->query('SELECT customer_code, display_name FROM ssw_customers ORDER BY display_name')->fetchAll(),
            'versions' => $this->db->query('SELECT DISTINCT software_version FROM ssw_selection_revisions ORDER BY software_version DESC')->fetchAll(PDO::FETCH_COLUMN),
            'countries' => $this->db->query("SELECT DISTINCT geo_country_code FROM ssw_selection_revisions WHERE geo_country_code IS NOT NULL ORDER BY geo_country_code")->fetchAll(PDO::FETCH_COLUMN),
        ];
    }

    public function search(array $filters, int $page, int $pageSize): array
    {
        $where = [];
        $params = [];
        if (empty($filters['include_unlocated'])) $where[] = self::locationCondition('r');
        $query = trim((string) ($filters['q'] ?? ''));
        if ($query !== '') {
            $where[] = '(s.public_reference LIKE ? OR s.project_id LIKE ? OR c.customer_code LIKE ? OR c.display_name LIKE ? '
                . 'OR r.selection_payload LIKE ? OR u.first_name LIKE ? OR u.last_name LIKE ? OR u.email LIKE ?)';
            $needle = '%' . $query . '%';
            array_push($params, $needle, $needle, $needle, $needle, $needle, $needle, $needle, $needle);
        }
        foreach (['customer' => 'c.customer_code', 'version' => 'r.software_version', 'country' => 'r.geo_country_code'] as $key => $column) {
            $value = trim((string) ($filters[$key] ?? ''));
            if ($value !== '') { $where[] = $column . ' = ?'; $params[] = $value; }
        }
        if (!empty($filters['from'])) { $where[] = 's.created_at >= ?'; $params[] = $filters['from'] . ' 00:00:00'; }
        if (!empty($filters['to'])) { $where[] = 's.created_at < DATE_ADD(?, INTERVAL 1 DAY)'; $params[] = $filters['to'] . ' 00:00:00'; }
        $whereSql = $where ? ' WHERE ' . implode(' AND ', $where) : '';
        $count = $this->db->prepare('SELECT COUNT(*) FROM ssw_selections s JOIN ssw_customers c ON c.id = s.customer_id '
            . 'JOIN ssw_selection_revisions r ON r.selection_id = s.id AND r.revision_number = s.latest_revision '
            . 'LEFT JOIN ssw_installations owner_installation ON owner_installation.id = r.installation_id '
            . 'LEFT JOIN ssw_users u ON u.id = owner_installation.user_id' . $whereSql);
        $count->execute($params);
        $total = (int) $count->fetchColumn();
        $offset = max(0, ($page - 1) * $pageSize);
        $sql = 'SELECT s.id, s.public_reference, s.project_id, s.latest_revision, s.created_at, s.updated_at, '
            . 'c.customer_code, c.display_name, i.installation_code, r.selection_payload, r.software_version, '
            . 'u.id AS license_user_id, u.first_name AS license_first_name, u.last_name AS license_last_name, u.email AS license_email, '
            . 'r.change_kind, r.geo_country_code, r.geo_city, r.created_at AS revision_created_at '
            . 'FROM ssw_selections s JOIN ssw_customers c ON c.id = s.customer_id '
            . 'JOIN ssw_installations i ON i.id = s.created_by_installation_id '
            . 'JOIN ssw_selection_revisions r ON r.selection_id = s.id AND r.revision_number = s.latest_revision'
            . ' LEFT JOIN ssw_installations owner_installation ON owner_installation.id = r.installation_id'
            . ' LEFT JOIN ssw_users u ON u.id = owner_installation.user_id'
            . $whereSql . ' ORDER BY r.created_at DESC LIMIT ' . (int) $pageSize . ' OFFSET ' . (int) $offset;
        $statement = $this->db->prepare($sql);
        $statement->execute($params);
        return ['items' => $statement->fetchAll(), 'total' => $total, 'page' => $page, 'pages' => max(1, (int) ceil($total / $pageSize))];
    }

    private static function locationCondition(string $alias): string
    {
        return "((NULLIF(TRIM({$alias}.geo_country_code), '') IS NOT NULL) "
            . "OR (NULLIF(TRIM({$alias}.geo_city), '') IS NOT NULL))";
    }

    private static function projectHasLocationCondition(string $alias): string
    {
        return 'EXISTS (SELECT 1 FROM ssw_multi_project_items location_item '
            . 'JOIN ssw_selections location_selection ON location_selection.id = location_item.selection_id '
            . 'JOIN ssw_selection_revisions location_revision ON location_revision.selection_id = location_selection.id '
            . 'AND location_revision.revision_number = location_selection.latest_revision '
            . 'WHERE location_item.multi_project_id = ' . $alias . '.id AND '
            . self::locationCondition('location_revision') . ')';
    }

    public function detail(string $reference, ?int $revision = null): ?array
    {
        $digits = preg_replace('/\D+/', '', preg_replace('/-R\d+$/i', '', trim($reference)));
        if (!is_string($digits) || strlen($digits) !== 16) return null;
        $head = $this->db->prepare('SELECT s.id, s.public_reference, s.project_id, s.latest_revision, s.created_at, s.updated_at, '
            . 'c.customer_code, c.display_name, i.installation_code FROM ssw_selections s '
            . 'JOIN ssw_customers c ON c.id = s.customer_id JOIN ssw_installations i ON i.id = s.created_by_installation_id '
            . 'WHERE s.public_reference = ?');
        $head->execute([$digits]);
        $selection = $head->fetch();
        if (!is_array($selection)) return null;
        $revisions = $this->db->prepare('SELECT revision_number, change_kind, software_version, calculation_engine_version, '
            . 'database_schema_version, database_data_version, database_content_hash, selection_format_version, '
            . 'report_template_version, api_contract_version, geo_country_code, geo_city, geo_source, geo_accuracy_km, '
            . 'technical_input_hash, calculation_output_hash, calculation_basis_hash, snapshot_hash, created_at '
            . 'FROM ssw_selection_revisions WHERE selection_id = ? ORDER BY revision_number DESC');
        $revisions->execute([(int) $selection['id']]);
        $selection['revisions'] = $revisions->fetchAll();
        $selectedRevision = $revision ?: (int) $selection['latest_revision'];
        $payload = $this->db->prepare('SELECT * FROM ssw_selection_revisions WHERE selection_id = ? AND revision_number = ?');
        $payload->execute([(int) $selection['id'], $selectedRevision]);
        $row = $payload->fetch();
        if (!is_array($row)) return null;
        $decoded = json_decode((string) $row['selection_payload'], true, 128);
        $row['payload'] = is_array($decoded) ? $decoded : [];
        unset($row['selection_payload']);
        $selection['selected'] = $row;
        return $selection;
    }

    public function audit(string $event, string $user, ?int $selectionId, array $data = []): void
    {
        $data['admin_user'] = $user;
        $statement = $this->db->prepare('INSERT INTO ssw_audit_events (selection_id, event_type, event_data) VALUES (?, ?, ?)');
        $statement->execute([$selectionId, $event, json_encode($data, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE)]);
    }
}
