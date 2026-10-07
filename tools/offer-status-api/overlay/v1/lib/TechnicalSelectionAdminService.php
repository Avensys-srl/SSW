<?php
declare(strict_types=1);

require_once __DIR__ . '/PublicReference.php';

final class TechnicalSelectionAdminService
{
    private $db;

    public function __construct(PDO $db)
    {
        $this->db = $db;
        $this->db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $this->db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
    }

    public function find(string $reference): ?array
    {
        $digits = $this->referenceDigits($reference);
        if (!is_string($digits) || !PublicReference::isValid($digits)) return null;

        $statement = $this->db->prepare(
            'SELECT s.id, s.public_reference, s.project_id, s.latest_revision, s.created_at, s.updated_at, '
            . 'c.customer_code, c.display_name, i.installation_code '
            . 'FROM ssw_selections s '
            . 'JOIN ssw_customers c ON c.id = s.customer_id '
            . 'JOIN ssw_installations i ON i.id = s.created_by_installation_id '
            . 'WHERE s.public_reference = ?'
        );
        $statement->execute([$digits]);
        $selection = $statement->fetch();
        if (!is_array($selection)) return null;

        $revisions = $this->db->prepare(
            'SELECT revision_number, change_kind, software_version, calculation_engine_version, '
            . 'database_schema_version, database_data_version, database_content_hash, '
            . 'selection_format_version, report_template_version, api_contract_version, '
            . 'geo_country_code, geo_city, geo_source, geo_accuracy_km, snapshot_hash, created_at, '
            . 'offer_status, offer_generated_at_utc, offer_definitive_at_utc '
            . 'FROM ssw_selection_revisions WHERE selection_id = ? ORDER BY revision_number DESC'
        );
        $revisions->execute([(int) $selection['id']]);
        $selection['revisions'] = $revisions->fetchAll();
        return $selection;
    }

    public function exportRevision(string $reference, int $revision): ?array
    {
        $digits = $this->referenceDigits($reference);
        if (!is_string($digits) || !PublicReference::isValid($digits) || $revision < 1) return null;
        $statement = $this->db->prepare(
            'SELECT s.public_reference, r.revision_number, r.change_kind, r.selection_payload, '
            . 'r.software_version, r.calculation_engine_version, r.database_schema_version, '
            . 'r.database_data_version, r.database_content_hash, r.selection_format_version, '
            . 'r.report_template_version, r.api_contract_version, r.geo_country_code, r.geo_city, '
            . 'r.geo_source, r.geo_accuracy_km, r.snapshot_hash, r.created_at, '
            . 'r.offer_status, r.offer_generated_at_utc, r.offer_definitive_at_utc '
            . 'FROM ssw_selections s JOIN ssw_selection_revisions r ON r.selection_id = s.id '
            . 'WHERE s.public_reference = ? AND r.revision_number = ?'
        );
        $statement->execute([$digits, $revision]);
        $row = $statement->fetch();
        if (!is_array($row)) return null;
        $payload = json_decode((string) $row['selection_payload'], true, 128);
        if (!is_array($payload)) $payload = [];
        unset($row['selection_payload']);
        return [
            'reference' => PublicReference::display($digits, $revision),
            'metadata' => $row,
            'selection' => $payload,
        ];
    }

    public function audit(string $event, string $adminUser, string $reference, ?int $revision = null): void
    {
        $digits = $this->referenceDigits($reference);
        $selectionId = null;
        if (is_string($digits) && PublicReference::isValid($digits)) {
            $statement = $this->db->prepare('SELECT id FROM ssw_selections WHERE public_reference = ?');
            $statement->execute([$digits]);
            $value = $statement->fetchColumn();
            if ($value !== false) $selectionId = (int) $value;
        }
        $data = json_encode(['admin_user' => $adminUser, 'revision' => $revision], JSON_UNESCAPED_SLASHES);
        $this->db->prepare('INSERT INTO ssw_audit_events (selection_id, event_type, event_data) VALUES (?, ?, ?)')
            ->execute([$selectionId, $event, $data]);
    }

    private function referenceDigits(string $reference): string
    {
        $withoutRevision = preg_replace('/-R\d+\s*$/i', '', trim($reference));
        $digits = preg_replace('/\D+/', '', (string) $withoutRevision);
        return is_string($digits) ? $digits : '';
    }
}
