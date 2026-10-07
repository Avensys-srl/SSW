<?php
declare(strict_types=1);

final class LicenseRepository
{
    public const MINIMUM_UPDATE_VERSION = '2.0.0.15';

    private $db;

    public function __construct(PDO $db)
    {
        $this->db = $db;
        $this->db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $this->db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
    }

    public static function isAvailable(PDO $db): bool
    {
        try {
            $db->query('SELECT 1 FROM ssw_users LIMIT 1');
            return true;
        } catch (PDOException $exception) {
            return false;
        }
    }

    public function customers(): array
    {
        return $this->db->query("SELECT id, customer_code, display_name FROM ssw_customers WHERE status = 'active' AND customer_code = 'AV'")->fetchAll();
    }

    public function users(): array
    {
        $users = $this->db->query('SELECT u.*, c.customer_code, c.display_name, '
            . '(SELECT COUNT(*) FROM ssw_installations ai WHERE ai.user_id = u.id AND ai.status = \'active\') AS active_devices, '
            . '(SELECT COALESCE(SUM(d.launch_count),0) FROM ssw_usage_daily d JOIN ssw_installations di ON di.id=d.installation_id WHERE di.user_id=u.id) AS launches, '
            . '(SELECT COUNT(*) FROM ssw_usage_daily d JOIN ssw_installations di ON di.id=d.installation_id WHERE di.user_id=u.id) AS active_days, '
            . '(SELECT MAX(d.last_seen_at) FROM ssw_usage_daily d JOIN ssw_installations di ON di.id=d.installation_id WHERE di.user_id=u.id) AS last_used_at '
            . ',(SELECT ac.id FROM ssw_activation_codes ac WHERE ac.user_id=u.id AND ac.consumed_at IS NULL AND ac.expires_at>CURRENT_TIMESTAMP ORDER BY ac.id DESC LIMIT 1) AS pending_activation_code_id '
            . ',(SELECT ac.display_code FROM ssw_activation_codes ac WHERE ac.user_id=u.id AND ac.consumed_at IS NULL AND ac.expires_at>CURRENT_TIMESTAMP ORDER BY ac.id DESC LIMIT 1) AS pending_activation_code '
            . ',(SELECT ac.expires_at FROM ssw_activation_codes ac WHERE ac.user_id=u.id AND ac.consumed_at IS NULL AND ac.expires_at>CURRENT_TIMESTAMP ORDER BY ac.id DESC LIMIT 1) AS pending_activation_expires_at '
            . "FROM ssw_users u JOIN ssw_customers c ON c.id=u.customer_id WHERE u.status<>'deleted' AND u.email NOT LIKE 'deleted+%@invalid.local' ORDER BY u.created_at DESC")->fetchAll();
        $devices = $this->db->query('SELECT i.*, COALESCE(SUM(d.launch_count),0) AS launches, COUNT(d.usage_date) AS active_days, MAX(d.last_seen_at) AS last_used_at '
            . 'FROM ssw_installations i LEFT JOIN ssw_usage_daily d ON d.installation_id=i.id WHERE i.user_id IS NOT NULL AND i.archived_at IS NULL GROUP BY i.id ORDER BY i.user_id,i.device_number')->fetchAll();
        $byUser = [];
        foreach ($devices as $device) $byUser[(int) $device['user_id']][] = $device;
        foreach ($users as &$user) $user['devices'] = $byUser[(int) $user['id']] ?? [];
        return $users;
    }

    public function unreadNotificationCount(): int
    {
        $lastSeenId = (int) $this->db->query("SELECT COALESCE(MAX(id),0) FROM ssw_license_events WHERE event_type='portal.notifications_seen'")->fetchColumn();
        $statement = $this->db->prepare("SELECT COUNT(*) FROM ssw_license_events WHERE id>? AND event_type LIKE 'license.%' AND event_type<>'license.checked'");
        $statement->execute([$lastSeenId]);
        $unreadEvents = (int) $statement->fetchColumn();
        $submittedRegistrations = (int) $this->db->query("SELECT COUNT(*) FROM ssw_registration_invitations WHERE status='submitted'")->fetchColumn();
        $pendingUsers = (int) $this->db->query("SELECT COUNT(*) FROM ssw_users WHERE status='pending'")->fetchColumn();
        $openActions = $submittedRegistrations + $pendingUsers + count($this->activationRequests())
            + count($this->updateInvitationUsers());
        return max($unreadEvents, $openActions);
    }

    public function notificationPreview(): string
    {
        $lines = [];
        $add = static function (array $person, string $reason) use (&$lines): void {
            $name = trim(($person['first_name'] ?? '') . ' ' . ($person['last_name'] ?? ''));
            if ($name === '') $name = trim((string) ($person['email'] ?? ''));
            if ($name === '') $name = 'Utente non identificato';
            $lines[$name . ': ' . $reason] = true;
        };
        foreach ($this->registrationRequests() as $person) $add($person, 'Registrazione da approvare');
        foreach ($this->pendingUsers() as $person) $add($person, 'Attivazione da completare');
        foreach ($this->activationRequests() as $person) $add($person, 'Richiesta di attivazione');
        foreach ($this->updateInvitationUsers() as $person) $add($person, 'Aggiornamento da completare');
        $changes = $this->unreadUserChanges();
        foreach ($this->users() as $person) {
            foreach ($changes[(int) $person['id']] ?? [] as $change) $add($person, $change['label']);
        }
        $preview = array_slice(array_keys($lines), 0, 8);
        if (count($lines) > 8) $preview[] = 'Altre ' . (count($lines) - 8) . ' notifiche: apri Licenze';
        return $preview ? implode("\n", $preview) : 'Nuove richieste o variazioni di licenza';
    }

    public function updateInvitationUsers(string $minimumVersion = self::MINIMUM_UPDATE_VERSION): array
    {
        $workflowEvents = $this->updateWorkflowEvents();
        $users = [];
        foreach ($this->users() as $user) {
            if (($user['status'] ?? '') !== 'active') continue;
            $outdatedDevices = [];
            foreach ($user['devices'] as $device) {
                if (($device['status'] ?? '') !== 'active') continue;
                $version = trim((string) ($device['software_version'] ?? ''));
                if (preg_match('/^\d+(?:\.\d+){1,3}$/D', $version) !== 1) continue;
                if (version_compare($version, $minimumVersion, '<')) $outdatedDevices[] = $device;
            }
            if (!$outdatedDevices) continue;
            $user['outdated_devices'] = $outdatedDevices;
            $user['minimum_update_version'] = $minimumVersion;
            $workflow = $workflowEvents[(int) $user['id']] ?? [];
            $request = $workflow['request'] ?? null;
            $confirmed = $workflow['confirmed'] ?? null;
            $reinstall = $workflow['reinstall'] ?? null;
            $requestData = $request ? json_decode((string) ($request['event_data'] ?? ''), true) : [];
            $requestData = is_array($requestData) ? $requestData : [];
            $requestExpiry = !empty($requestData['expires_at'])
                ? new DateTimeImmutable((string) $requestData['expires_at'], new DateTimeZone('UTC')) : null;
            $requestExpired = $requestExpiry !== null && $requestExpiry <= new DateTimeImmutable('now', new DateTimeZone('UTC'));
            $user['update_workflow_state'] = !$request ? 'update_required'
                : (!$confirmed ? ($requestExpired ? 'uninstall_request_expired' : 'uninstall_requested')
                    : (!$reinstall ? 'uninstall_confirmed' : 'reinstall_sent'));
            $user['update_request_event_id'] = (int) ($request['id'] ?? 0);
            $user['update_reinstall_event_id'] = (int) ($reinstall['id'] ?? 0);
            $user['update_requested_at'] = (string) ($request['created_at'] ?? '');
            $user['update_request_expires_at'] = $requestExpiry ? $requestExpiry->format('Y-m-d H:i:s') : '';
            $user['update_request_expires_in'] = $requestExpiry ? $this->formatRemainingTime($requestExpiry) : '';
            $user['update_confirmed_at'] = (string) ($confirmed['created_at'] ?? '');
            $user['reinstall_sent_at'] = (string) ($reinstall['created_at'] ?? '');
            $user['activity_code'] = $reinstall
                ? $this->activityCode('REI', (string) $reinstall['id'])
                : ($request ? $this->activityCode('DIS', (string) $request['id']) : '');
            $users[] = $user;
        }
        return $users;
    }

    public function requestUpdateUninstall(int $userId, string $admin): array
    {
        $user = $this->user($userId);
        if ($user['status'] !== 'active') throw new RuntimeException('L’utente deve essere attivo.');
        $eligible = array_filter($this->updateInvitationUsers(), static function (array $candidate) use ($userId): bool {
            return (int) $candidate['id'] === $userId;
        });
        if (!$eligible) throw new RuntimeException('Questo utente non richiede l’aggiornamento guidato.');
        $expires = time() + (7 * 86400);
        $this->event($userId, null, 'portal.update_uninstall_requested', [
            'admin_user' => $admin,
            'expires_at' => gmdate('Y-m-d H:i:s', $expires),
        ]);
        $eventId = (int) $this->db->lastInsertId();
        return $this->updateUninstallRequestResult($eventId, $expires);
    }

    public function updateUninstallRequestForAdmin(int $userId): array
    {
        $statement = $this->db->prepare("SELECT id,event_data FROM ssw_license_events WHERE user_id=? AND event_type='portal.update_uninstall_requested' ORDER BY id DESC LIMIT 1");
        $statement->execute([$userId]);
        $row = $statement->fetch();
        if (!is_array($row)) throw new RuntimeException('Nessuna richiesta di disinstallazione disponibile.');
        $data = json_decode((string) $row['event_data'], true);
        $expires = is_array($data) ? strtotime((string) ($data['expires_at'] ?? '') . ' UTC') : false;
        if ($expires === false || $expires <= time()) throw new RuntimeException('La richiesta di disinstallazione è scaduta: generarne una nuova.');
        return $this->updateUninstallRequestResult((int) $row['id'], $expires);
    }

    public function updateUninstallConfirmation(string $token, bool $confirm = false): array
    {
        if (!preg_match('/^([1-9][0-9]*)\.([1-9][0-9]*)\.([a-f0-9]{64})$/D', trim($token), $matches)) {
            throw new RuntimeException('Questo link di conferma non è valido.');
        }
        $eventId = (int) $matches[1];
        $expires = (int) $matches[2];
        if (!hash_equals($this->updateUninstallToken($eventId, $expires), trim($token)) || $expires <= time()) {
            throw new RuntimeException('Questo link di conferma è scaduto o non è valido.');
        }
        $statement = $this->db->prepare("SELECT e.user_id,e.created_at,u.email,u.company_name,u.first_name,u.last_name FROM ssw_license_events e JOIN ssw_users u ON u.id=e.user_id WHERE e.id=? AND e.event_type='portal.update_uninstall_requested'");
        $statement->execute([$eventId]);
        $request = $statement->fetch();
        if (!is_array($request)) throw new RuntimeException('Richiesta di disinstallazione non trovata.');
        $confirmedStatement = $this->db->prepare("SELECT event_data,created_at FROM ssw_license_events WHERE user_id=? AND event_type='portal.update_uninstall_confirmed' ORDER BY id DESC");
        $confirmedStatement->execute([(int) $request['user_id']]);
        $confirmedAt = false;
        foreach ($confirmedStatement->fetchAll() as $confirmationRow) {
            $confirmationData = json_decode((string) $confirmationRow['event_data'], true);
            if ((int) ($confirmationData['request_event_id'] ?? 0) !== $eventId) continue;
            $confirmedAt = (string) $confirmationRow['created_at'];
            break;
        }
        if ($confirm && $confirmedAt === false) {
            $this->event((int) $request['user_id'], null, 'portal.update_uninstall_confirmed', ['request_event_id' => $eventId]);
            $confirmedAt = gmdate('Y-m-d H:i:s');
        }
        $request['confirmed'] = $confirmedAt !== false;
        $request['confirmed_at'] = $confirmedAt !== false ? (string) $confirmedAt : '';
        return $request;
    }

    public function markReinstallPrepared(int $userId, int $requestEventId, string $admin): string
    {
        $this->assertReinstallReady($userId, $requestEventId);
        $this->event($userId, null, 'portal.update_reinstall_prepared', [
            'request_event_id' => $requestEventId,
            'admin_user' => $admin,
        ]);
        return $this->activityCode('REI', (string) $this->db->lastInsertId());
    }

    public function assertReinstallReady(int $userId, int $requestEventId): void
    {
        foreach ($this->updateInvitationUsers() as $candidate) {
            if ((int) $candidate['id'] !== $userId) continue;
            if ((int) ($candidate['update_request_event_id'] ?? 0) !== $requestEventId
                || !in_array(($candidate['update_workflow_state'] ?? ''), ['uninstall_confirmed', 'reinstall_sent'], true)) {
                throw new RuntimeException('La disinstallazione non risulta ancora confermata.');
            }
            return;
        }
        throw new RuntimeException('Aggiornamento guidato non disponibile per questo utente.');
    }

    public function unreadUserChanges(): array
    {
        $lastSeenId = (int) $this->db->query("SELECT COALESCE(MAX(id),0) FROM ssw_license_events WHERE event_type='portal.notifications_seen'")->fetchColumn();
        $types = [
            'license.activation_requested' => 'Richiesta di attivazione ricevuta',
            'license.activated' => 'Dispositivo attivato',
            'license.legacy_claimed' => 'Dispositivo associato',
            'portal.registration_approved' => 'Registrazione approvata e PIN generato',
            'portal.activation_code_created' => 'Nuovo PIN generato',
            'portal.user_revoked' => 'Utente e dispositivi revocati',
            'portal.user_active' => 'Utente e dispositivi riattivati',
            'portal.device_revoked' => 'Dispositivo revocato',
            'portal.device_active' => 'Dispositivo riattivato',
            'portal.device_archived' => 'Dispositivo archiviato',
            'portal.user_company_updated' => 'Società aggiornata',
        ];
        $quotedTypes = implode(',', array_map(static function (string $type): string {
            return "'" . str_replace("'", "''", $type) . "'";
        }, array_keys($types)));
        $statement = $this->db->prepare("SELECT id,user_id,event_type,event_data,created_at FROM ssw_license_events WHERE id>? AND event_type IN ($quotedTypes) ORDER BY id DESC LIMIT 100");
        $statement->execute([$lastSeenId]);

        $usersByEmail = [];
        foreach ($this->db->query("SELECT id,email FROM ssw_users WHERE status<>'deleted'")->fetchAll() as $user) {
            $usersByEmail[strtolower(trim((string) $user['email']))] = (int) $user['id'];
        }
        $changes = [];
        foreach ($statement->fetchAll() as $row) {
            $data = json_decode((string) $row['event_data'], true);
            $data = is_array($data) ? $data : [];
            $userId = (int) ($row['user_id'] ?? 0);
            if ($userId <= 0 && !empty($data['email'])) {
                $userId = $usersByEmail[strtolower(trim((string) $data['email']))] ?? 0;
            }
            if ($userId <= 0) continue;
            $label = $types[(string) $row['event_type']];
            if (!empty($data['device_number'])) $label .= ' · dispositivo ' . (int) $data['device_number'];
            elseif (!empty($data['installation_code'])) $label .= ' · ' . trim((string) $data['installation_code']);
            $changes[$userId][] = [
                'event_type' => (string) $row['event_type'],
                'label' => $label,
                'created_at' => (string) $row['created_at'],
            ];
        }
        return $changes;
    }

    public function problemLog(int $userId): array
    {
        $statement = $this->db->prepare("SELECT id,event_data,created_at FROM ssw_license_events WHERE user_id=? AND event_type='portal.user_problem_logged' ORDER BY id DESC");
        $statement->execute([$userId]);
        $entries = [];
        foreach ($statement->fetchAll() as $row) {
            $data = json_decode((string) $row['event_data'], true);
            if (!is_array($data)) continue;
            $data['id'] = (int) $row['id'];
            $data['created_at'] = $row['created_at'];
            $entries[] = $data;
        }
        return $entries;
    }

    public function addProblemLog(int $userId, string $problem, string $solution, string $status, string $admin): void
    {
        $problem = trim($problem);
        $solution = trim($solution);
        if ($problem === '' || strlen($problem) > 2000) throw new RuntimeException('Descrivere il problema in massimo 2000 caratteri.');
        if (strlen($solution) > 2000) throw new RuntimeException('La soluzione proposta può contenere massimo 2000 caratteri.');
        if (!in_array($status, ['open', 'proposed', 'resolved'], true)) throw new RuntimeException('Stato della problematica non valido.');
        $this->user($userId);
        $this->event($userId, null, 'portal.user_problem_logged', [
            'problem' => $problem,
            'solution' => $solution,
            'status' => $status,
            'admin_user' => $admin,
        ]);
    }

    public function pushPublicKey(): string
    {
        $path = 'C:/ProgramData/Avensys/SSW/push-vapid.json';
        if (!is_file($path)) return '';
        $config = json_decode((string) file_get_contents($path), true);
        return is_array($config) ? (string) ($config['publicKey'] ?? '') : '';
    }

    public function savePushSubscription(array $subscription): void
    {
        $endpoint = trim((string) ($subscription['endpoint'] ?? ''));
        $keys = isset($subscription['keys']) && is_array($subscription['keys']) ? $subscription['keys'] : [];
        $publicKey = trim((string) ($keys['p256dh'] ?? ''));
        $authToken = trim((string) ($keys['auth'] ?? ''));
        $encoding = trim((string) ($subscription['contentEncoding'] ?? 'aesgcm'));
        if (filter_var($endpoint, FILTER_VALIDATE_URL) === false || stripos($endpoint, 'https://') !== 0 || $publicKey === '' || $authToken === '') {
            throw new RuntimeException('Sottoscrizione notifiche non valida.');
        }
        if (!in_array($encoding, ['aesgcm', 'aes128gcm'], true)) $encoding = 'aesgcm';
        $this->ensurePushSchema();
        $hash = hash('sha256', $endpoint);
        if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') {
            $sql = 'INSERT OR REPLACE INTO ssw_push_subscriptions (endpoint_hash,endpoint,public_key,auth_token,content_encoding,created_at,revoked_at) VALUES (?,?,?,?,?,CURRENT_TIMESTAMP,NULL)';
        } else {
            $sql = 'INSERT INTO ssw_push_subscriptions (endpoint_hash,endpoint,public_key,auth_token,content_encoding,revoked_at) VALUES (?,?,?,?,?,NULL) ON DUPLICATE KEY UPDATE endpoint=VALUES(endpoint),public_key=VALUES(public_key),auth_token=VALUES(auth_token),content_encoding=VALUES(content_encoding),revoked_at=NULL';
        }
        $this->db->prepare($sql)->execute([$hash, $endpoint, $publicKey, $authToken, $encoding]);
    }

    public function removePushSubscription(string $endpoint): void
    {
        if ($endpoint === '') return;
        $this->ensurePushSchema();
        $this->db->prepare('UPDATE ssw_push_subscriptions SET revoked_at=CURRENT_TIMESTAMP WHERE endpoint_hash=?')
            ->execute([hash('sha256', $endpoint)]);
    }

    private function ensurePushSchema(): void
    {
        try {
            if ($this->db->query('SELECT 1 FROM ssw_push_subscriptions LIMIT 1') !== false) return;
        } catch (Throwable $exception) {
        }
        if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') {
            $this->db->exec("CREATE TABLE IF NOT EXISTS ssw_push_subscriptions (id INTEGER PRIMARY KEY AUTOINCREMENT,endpoint_hash TEXT NOT NULL UNIQUE,endpoint TEXT NOT NULL,public_key TEXT NOT NULL,auth_token TEXT NOT NULL,content_encoding TEXT NOT NULL DEFAULT 'aesgcm',created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,revoked_at TEXT)");
            return;
        }
        $this->db->exec("CREATE TABLE IF NOT EXISTS ssw_push_subscriptions (id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,endpoint_hash CHAR(64) NOT NULL UNIQUE,endpoint LONGTEXT NOT NULL,public_key VARCHAR(255) NOT NULL,auth_token VARCHAR(255) NOT NULL,content_encoding VARCHAR(32) NOT NULL DEFAULT 'aesgcm',created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,revoked_at TIMESTAMP NULL DEFAULT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
    }

    public function activationRequests(): array
    {
        $handled = [];
        foreach ($this->db->query("SELECT event_data FROM ssw_license_events WHERE event_type='portal.activation_request_handled'")->fetchAll(PDO::FETCH_COLUMN) as $json) {
            $data = json_decode((string) $json, true);
            if (is_array($data) && isset($data['request_event_id'])) $handled[(int) $data['request_event_id']] = true;
        }
        $rows = $this->db->query("SELECT id,event_data,created_at FROM ssw_license_events WHERE event_type='license.activation_requested' ORDER BY id DESC LIMIT 50")->fetchAll();
        $requests = [];
        foreach ($rows as $row) {
            $data = json_decode((string) $row['event_data'], true);
            if (!is_array($data) || isset($handled[(int) $row['id']])) continue;
            $data['event_id'] = (int) $row['id'];
            $data['created_at'] = $row['created_at'];
            $requests[] = $data;
        }
        return $requests;
    }

    public function registrationRequests(): array
    {
        $requests = $this->db->query("SELECT id,email,company_name,first_name,last_name,submitted_at FROM ssw_registration_invitations WHERE status='submitted' ORDER BY submitted_at ASC,id ASC")->fetchAll();
        $deletedUsers = $this->deletedUsers();
        foreach ($requests as &$request) {
            $request['archived_matches'] = count(array_filter($deletedUsers, function (array $deleted) use ($request): bool {
                $sameEmail = strtolower((string) $deleted['email']) === strtolower((string) $request['email']);
                $sameName = $this->personComparisonKey((string) ($deleted['first_name'] ?? '')) === $this->personComparisonKey((string) ($request['first_name'] ?? ''))
                    && $this->personComparisonKey((string) ($deleted['last_name'] ?? '')) === $this->personComparisonKey((string) ($request['last_name'] ?? ''));
                return $sameEmail || $sameName;
            }));
        }
        return $requests;
    }

    public function pendingUsers(): array
    {
        $rows = $this->db->query("SELECT u.id,u.email,u.company_name,u.first_name,u.last_name,u.created_at,ac.id AS activation_code_id,ac.display_code,ac.expires_at "
            . "FROM ssw_users u LEFT JOIN ssw_activation_codes ac ON ac.id=(SELECT a.id FROM ssw_activation_codes a WHERE a.user_id=u.id AND a.consumed_at IS NULL ORDER BY a.id DESC LIMIT 1) "
            . "WHERE u.status='pending' ORDER BY u.created_at ASC")->fetchAll();
        foreach ($rows as &$row) {
            $row['activation_expired'] = empty($row['expires_at']) || strtotime((string) $row['expires_at'] . ' UTC') <= time();
            $row['activity_code'] = !empty($row['activation_code_id'])
                ? $this->activityCode('PIN', (string) $row['activation_code_id']) : '';
            $row['expires_in'] = !empty($row['expires_at'])
                ? $this->formatRemainingTime(new DateTimeImmutable((string) $row['expires_at'], new DateTimeZone('UTC')))
                : '';
        }
        unset($row);
        return $rows;
    }

    public function deletedUsers(): array
    {
        $currentEmails = [];
        foreach ($this->db->query("SELECT email FROM ssw_users WHERE status<>'deleted'")->fetchAll(PDO::FETCH_COLUMN) as $email) {
            $currentEmails[strtolower(trim((string) $email))] = true;
        }
        $rows = $this->db->query("SELECT event_data,created_at FROM ssw_license_events WHERE event_type='portal.user_deleted' ORDER BY id DESC LIMIT 100")->fetchAll();
        $deletedUsers = [];
        $seenUserIds = [];
        $seenEmails = [];
        foreach ($rows as $row) {
            $data = json_decode((string) $row['event_data'], true);
            if (!is_array($data) || empty($data['email'])) continue;
            if (preg_match('/^deleted\+[0-9]+\+[0-9]+@invalid\.local$/D', (string) $data['email']) === 1) continue;
            $emailKey = strtolower(trim((string) $data['email']));
            if (isset($currentEmails[$emailKey]) || isset($seenEmails[$emailKey])) continue;
            $seenEmails[$emailKey] = true;
            $originalUserId = (int) ($data['original_user_id'] ?? 0);
            if ($originalUserId > 0 && isset($seenUserIds[$originalUserId])) continue;
            if ($originalUserId > 0) $seenUserIds[$originalUserId] = true;
            $data['deleted_at'] = $row['created_at'];
            $deletedUsers[] = $data;
        }
        return $deletedUsers;
    }

    public function removeDuplicateDeletedUserEvents(): int
    {
        $rows = $this->db->query("SELECT id,event_data FROM ssw_license_events WHERE event_type='portal.user_deleted' ORDER BY id DESC")->fetchAll();
        $seenEmails = [];
        $duplicateIds = [];
        foreach ($rows as $row) {
            $data = json_decode((string) $row['event_data'], true);
            $emailKey = is_array($data) ? strtolower(trim((string) ($data['email'] ?? ''))) : '';
            if ($emailKey === '' || preg_match('/^deleted\+[0-9]+\+[0-9]+@invalid\.local$/D', $emailKey) === 1) continue;
            if (isset($seenEmails[$emailKey])) {
                $duplicateIds[] = (int) $row['id'];
            } else {
                $seenEmails[$emailKey] = true;
            }
        }
        if (!$duplicateIds) return 0;
        $placeholders = implode(',', array_fill(0, count($duplicateIds), '?'));
        $statement = $this->db->prepare("DELETE FROM ssw_license_events WHERE event_type='portal.user_deleted' AND id IN ($placeholders)");
        $statement->execute($duplicateIds);
        return $statement->rowCount();
    }

    public function registrationInvitations(): array
    {
        $rows = $this->db->query("SELECT r.id,r.email,r.company_name,r.first_name,r.last_name,r.status,r.submitted_at,r.approved_at,r.created_at "
            . "FROM ssw_registration_invitations r "
            . "WHERE r.status IN ('invited','submitted') "
            . 'ORDER BY r.created_at DESC LIMIT 100')->fetchAll();
        foreach ($rows as &$row) {
            $expiry = $this->registrationInvitationExpiry((string) $row['created_at']);
            $row['expires_at'] = $expiry->format('Y-m-d H:i:s');
            $row['expires_in'] = $this->formatRemainingTime($expiry);
            $row['expired'] = $expiry <= new DateTimeImmutable('now', new DateTimeZone('UTC'));
            $row['activity_code'] = $this->activityCode('REG', $row['id'] . '|' . $row['created_at']);
        }
        unset($row);
        return $rows;
    }

    public function renewRegistrationInvitation(int $invitationId, string $admin): array
    {
        $statement = $this->db->prepare("SELECT email,company_name FROM ssw_registration_invitations WHERE id=? AND status='invited'");
        $statement->execute([$invitationId]);
        $invitation = $statement->fetch();
        if (!is_array($invitation)) throw new RuntimeException('Invito non trovato o non rinnovabile.');
        return $this->createRegistrationInvitation((string) $invitation['company_name'], (string) $invitation['email'], $admin, true);
    }

    public function revokeRegistrationInvitation(int $invitationId, string $admin): void
    {
        $statement = $this->db->prepare("UPDATE ssw_registration_invitations SET status='revoked',updated_at=CURRENT_TIMESTAMP WHERE id=? AND status='invited'");
        $statement->execute([$invitationId]);
        if ($statement->rowCount() === 0) throw new RuntimeException('Invito non trovato o già gestito.');
        $this->event(0, null, 'portal.registration_invitation_revoked', [
            'admin_user' => $admin,
            'invitation_id' => $invitationId,
        ]);
    }

    public function registrationInvitationForAdmin(int $invitationId): array
    {
        return $this->registrationInvitationResult($invitationId);
    }

    public function createRegistrationInvitation(string $companyName, string $email, string $admin, bool $allowArchived = false): array
    {
        $customers = $this->customers();
        if (count($customers) !== 1) throw new RuntimeException('Il cliente tecnico AV non è disponibile.');
        $companyName = $this->normalizeCompanyName($companyName);
        $email = $this->normalizeEmail($email);
        $archiveMatches = array_filter($this->deletedUsers(), static function (array $deleted) use ($email): bool {
            return strtolower((string) $deleted['email']) === $email;
        });
        if ($archiveMatches && !$allowArchived) {
            throw new RuntimeException('Attenzione: questa email è presente nell’archivio degli utenti eliminati. Se il reinserimento è intenzionale, selezionare “Procedi comunque” e preparare nuovamente l’invito.');
        }
        $statement = $this->db->prepare('SELECT * FROM ssw_registration_invitations WHERE email=?');
        $statement->execute([$email]);
        $existing = $statement->fetch();
        if (is_array($existing)) {
            if (in_array($existing['status'], ['invited', 'submitted', 'revoked'], true)) {
                $this->db->prepare("UPDATE ssw_registration_invitations SET company_name=?,status='invited',first_name=NULL,last_name=NULL,submitted_at=NULL,created_at=CURRENT_TIMESTAMP,created_by=?,approved_by=NULL,approved_at=NULL WHERE id=?")
                    ->execute([$companyName, $admin, (int) $existing['id']]);
            } else {
                $this->db->prepare('UPDATE ssw_registration_invitations SET company_name=? WHERE id=?')->execute([$companyName, (int) $existing['id']]);
            }
            return $this->registrationInvitationResult((int) $existing['id']);
        }
        $statement = $this->db->prepare("INSERT INTO ssw_registration_invitations (customer_id,email,company_name,status,created_by) VALUES (?,?,?,'invited',?)");
        $statement->execute([(int) $customers[0]['id'], $email, $companyName, $admin]);
        return $this->registrationInvitationResult((int) $this->db->lastInsertId());
    }

    public function registrationInvitation(string $token): array
    {
        $id = $this->invitationIdFromToken($token);
        $statement = $this->db->prepare('SELECT * FROM ssw_registration_invitations WHERE id=?');
        $statement->execute([$id]);
        $invitation = $statement->fetch();
        if (!is_array($invitation) || $invitation['status'] === 'revoked') throw new RuntimeException('Questo invito non è valido.');
        if ($this->registrationInvitationExpiry((string) $invitation['created_at']) <= new DateTimeImmutable('now', new DateTimeZone('UTC'))) {
            throw new RuntimeException('Questo invito è scaduto. Richiedere un nuovo invito ad Avensys.');
        }
        $invitation['token'] = $token;
        return $invitation;
    }

    public function submitRegistrationInvitation(string $token, string $firstName, string $lastName, string $email, string $companyName, bool $privacyAccepted): array
    {
        $invitation = $this->registrationInvitation($token);
        if ($invitation['status'] === 'approved') return $invitation;
        $firstName = $this->normalizePersonName($firstName, 'Il nome');
        $lastName = $this->normalizePersonName($lastName, 'Il cognome');
        $email = $this->normalizeEmail($email);
        $companyName = $this->normalizeCompanyName($companyName);
        if (!hash_equals((string) $invitation['email'], $email)) throw new RuntimeException('L’indirizzo email non corrisponde all’invito ricevuto.');
        if (!$privacyAccepted) throw new RuntimeException('È necessario accettare i termini d’uso e l’informativa privacy.');
        $statement = $this->db->prepare("UPDATE ssw_registration_invitations SET first_name=?,last_name=?,company_name=?,status='submitted',privacy_accepted_at=COALESCE(privacy_accepted_at,CURRENT_TIMESTAMP),submitted_at=CURRENT_TIMESTAMP WHERE id=? AND status IN ('invited','submitted')");
        $statement->execute([$firstName, $lastName, $companyName, (int) $invitation['id']]);
        $data = ['invitation_id' => (int) $invitation['id'], 'first_name' => $firstName, 'last_name' => $lastName,
            'email' => $email, 'company_name' => $companyName, 'requested_at_utc' => gmdate('c')];
        $this->event(0, null, 'license.registration_requested', $data);
        if (class_exists('LicensePushNotifier')) LicensePushNotifier::send($this->db, $data);
        return $this->registrationInvitation($token);
    }

    public function approveRegistrationInvitation(int $invitationId, string $admin): array
    {
        $statement = $this->db->prepare("SELECT * FROM ssw_registration_invitations WHERE id=? AND status='submitted'");
        $statement->execute([$invitationId]);
        $invitation = $statement->fetch();
        if (!is_array($invitation)) throw new RuntimeException('Richiesta di registrazione non trovata o già gestita.');
        $this->db->beginTransaction();
        try {
            $userStatement = $this->db->prepare('SELECT id,company_name,status FROM ssw_users WHERE email=?');
            $userStatement->execute([$invitation['email']]);
            $user = $userStatement->fetch();
            if (is_array($user)) {
                if ($user['status'] === 'revoked') throw new RuntimeException('L’utente associato a questa email è revocato.');
                $userId = (int) $user['id'];
                $this->db->prepare('UPDATE ssw_users SET first_name=?,last_name=?,company_name=? WHERE id=?')
                    ->execute([$invitation['first_name'], $invitation['last_name'], $invitation['company_name'], $userId]);
            } else {
                $this->db->prepare("INSERT INTO ssw_users (customer_id,email,company_name,first_name,last_name,status) VALUES (?,?,?,?,?,'pending')")
                    ->execute([(int) $invitation['customer_id'], $invitation['email'], $invitation['company_name'], $invitation['first_name'], $invitation['last_name']]);
                $userId = (int) $this->db->lastInsertId();
            }
            $this->db->prepare("UPDATE ssw_registration_invitations SET status='approved',approved_at=CURRENT_TIMESTAMP,approved_by=? WHERE id=?")
                ->execute([$admin, $invitationId]);
            $this->event($userId, null, 'portal.registration_approved', ['admin_user' => $admin, 'invitation_id' => $invitationId]);
            $this->db->commit();
            $activation = $this->newCode($userId, $admin);
            return ['invitation' => $this->registrationInvitationResult($invitationId), 'activation' => $activation];
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function invitationToken(int $invitationId): string
    {
        if ($invitationId < 1) throw new RuntimeException('Invito non valido.');
        return $invitationId . '.' . hash_hmac('sha256', 'ssw-registration|' . $invitationId, $this->invitationSecret());
    }

    public function pendingActivation(int $userId): ?array
    {
        $statement = $this->db->prepare('SELECT u.id AS user_id,u.email,ac.id AS activation_code_id,ac.display_code AS pin,ac.expires_at FROM ssw_activation_codes ac JOIN ssw_users u ON u.id=ac.user_id WHERE ac.user_id=? AND ac.consumed_at IS NULL AND ac.expires_at>CURRENT_TIMESTAMP ORDER BY ac.id DESC LIMIT 1');
        $statement->execute([$userId]);
        $activation = $statement->fetch();
        if (!is_array($activation) || empty($activation['pin'])) return null;
        $activation['activity_code'] = $this->activityCode('PIN', (string) $activation['activation_code_id']);
        return $activation;
    }

    public function downloadInvitation(int $userId, bool $reinstall = false): ?array
    {
        $statement = $this->db->prepare("SELECT id AS user_id,email,company_name,(SELECT COUNT(*) FROM ssw_installations i WHERE i.user_id=ssw_users.id AND i.status='active' AND i.archived_at IS NULL) AS active_devices FROM ssw_users WHERE id=? AND status='active' LIMIT 1");
        $statement->execute([$userId]);
        $user = $statement->fetch();
        if (!is_array($user)) return null;
        if ($reinstall) {
            foreach ($this->updateInvitationUsers() as $candidate) {
                if ((int) $candidate['id'] === $userId && ($candidate['update_workflow_state'] ?? '') === 'reinstall_sent') {
                    $user['activity_code'] = (string) ($candidate['activity_code'] ?? '');
                    break;
                }
            }
        }
        return $user;
    }

    public function approveActivationRequest(int $eventId, string $admin): array
    {
        $statement = $this->db->prepare("SELECT event_data FROM ssw_license_events WHERE id=? AND event_type='license.activation_requested'");
        $statement->execute([$eventId]);
        $data = json_decode((string) $statement->fetchColumn(), true);
        if (!is_array($data)) throw new RuntimeException('Richiesta di attivazione non trovata.');
        $email = $this->normalizeEmail((string) ($data['email'] ?? ''));
        $company = $this->normalizeCompanyName((string) ($data['company_name'] ?? ''));
        $userStatement = $this->db->prepare('SELECT id,status FROM ssw_users WHERE email=?');
        $userStatement->execute([$email]);
        $user = $userStatement->fetch();
        if (!is_array($user)) {
            $activation = $this->createUser($company, $email, $admin);
        } else {
            if ($user['status'] === 'revoked') throw new RuntimeException('L’utente è revocato: riattivarlo prima di generare il PIN.');
            $this->updateCompany((int) $user['id'], $company, $admin);
            $activation = $this->newCode((int) $user['id'], $admin);
        }
        $this->event(0, null, 'portal.activation_request_handled', ['admin_user' => $admin, 'request_event_id' => $eventId]);
        return $activation;
    }

    public function markNotificationsRead(string $admin): void
    {
        if ($this->unreadNotificationCount() === 0) return;
        $this->db->prepare('INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (NULL,NULL,?,?)')
            ->execute(['portal.notifications_seen', json_encode(['admin_user' => $admin], JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE)]);
    }

    public function createUser(string $companyName, string $email, string $admin): array
    {
        $customers = $this->customers();
        if (count($customers) !== 1) throw new RuntimeException('Il cliente tecnico AV non è disponibile.');
        $customerId = (int) $customers[0]['id'];
        $companyName = $this->normalizeCompanyName($companyName);
        $email = $this->normalizeEmail($email);
        $statement = $this->db->prepare("INSERT INTO ssw_users (customer_id,email,company_name,status) VALUES (?,?,?,'pending')");
        try {
            $statement->execute([$customerId, $email, $companyName]);
        } catch (PDOException $exception) {
            if ((string) $exception->getCode() === '23000') {
                throw new RuntimeException('Questa email è già presente nel portale.');
            }
            throw $exception;
        }
        return $this->newCode((int) $this->db->lastInsertId(), $admin);
    }

    public function updateCompany(int $userId, string $companyName, string $admin): void
    {
        $companyName = $this->normalizeCompanyName($companyName);
        $statement = $this->db->prepare('UPDATE ssw_users SET company_name=? WHERE id=?');
        $statement->execute([$companyName, $userId]);
        if ($statement->rowCount() === 0) $this->user($userId);
        $this->event($userId, null, 'portal.user_company_updated', [
            'admin_user' => $admin,
            'company_name' => $companyName,
        ]);
    }

    public function newCode(int $userId, string $admin): array
    {
        $user = $this->user($userId);
        if ($user['status'] === 'revoked') throw new RuntimeException('Riattivare prima l’utente.');
        if ((int) $user['active_devices'] >= 2) throw new RuntimeException('L’utente ha già due dispositivi attivi.');
        $pin = str_pad((string) random_int(0, 999999), 6, '0', STR_PAD_LEFT);
        $this->db->beginTransaction();
        try {
            $this->db->prepare('UPDATE ssw_activation_codes SET consumed_at=CURRENT_TIMESTAMP,display_code=NULL WHERE user_id=? AND consumed_at IS NULL')->execute([$userId]);
            $expires = gmdate('Y-m-d H:i:s', time() + 7 * 86400);
            $this->db->prepare('INSERT INTO ssw_activation_codes (user_id,code_hash,display_code,expires_at) VALUES (?,?,?,?)')
                ->execute([$userId, password_hash($pin, PASSWORD_DEFAULT), $pin, $expires]);
            $activationCodeId = (int) $this->db->lastInsertId();
            $this->event($userId, null, 'portal.activation_code_created', ['admin_user' => $admin, 'expires_at' => $expires]);
            $this->db->commit();
            return ['user_id' => $userId, 'activation_code_id' => $activationCodeId, 'activity_code' => $this->activityCode('PIN', (string) $activationCodeId), 'pin' => $pin, 'email' => $user['email'], 'company_name' => $user['company_name'], 'expires_at' => $expires];
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function setUserStatus(int $userId, string $status, string $admin): void
    {
        if (!in_array($status, ['active', 'revoked'], true)) throw new RuntimeException('Stato utente non valido.');
        $user = $this->user($userId);
        if ($user['status'] === $status) return;
        $this->db->beginTransaction();
        try {
            if ($status === 'revoked') {
                $deviceStatement = $this->db->prepare("SELECT id FROM ssw_installations WHERE user_id=? AND status='active' AND archived_at IS NULL");
                $deviceStatement->execute([$userId]);
                $deviceIds = array_map('intval', $deviceStatement->fetchAll(PDO::FETCH_COLUMN));
                $this->db->prepare("UPDATE ssw_installations SET status='revoked',revoked_at=CURRENT_TIMESTAMP WHERE user_id=? AND status='active' AND archived_at IS NULL")->execute([$userId]);
                $this->db->prepare("UPDATE ssw_users SET status='revoked',revoked_at=CURRENT_TIMESTAMP WHERE id=?")->execute([$userId]);
                $this->event($userId, null, 'portal.user_revoked', ['admin_user' => $admin, 'device_ids' => $deviceIds]);
            } else {
                $eventStatement = $this->db->prepare("SELECT event_data FROM ssw_license_events WHERE user_id=? AND event_type='portal.user_revoked' ORDER BY id DESC LIMIT 1");
                $eventStatement->execute([$userId]);
                $eventData = json_decode((string) $eventStatement->fetchColumn(), true);
                $deviceIds = is_array($eventData) && isset($eventData['device_ids']) ? array_map('intval', (array) $eventData['device_ids']) : [];
                if ($deviceIds) {
                    $placeholders = implode(',', array_fill(0, count($deviceIds), '?'));
                    $countStatement = $this->db->prepare("SELECT COUNT(*) FROM ssw_installations WHERE user_id=? AND archived_at IS NULL AND (status='active' OR id IN ($placeholders))");
                    $countStatement->execute(array_merge([$userId], $deviceIds));
                    if ((int) $countStatement->fetchColumn() > 2) throw new RuntimeException('La riattivazione supererebbe il limite di due dispositivi per email.');
                    $this->db->prepare("UPDATE ssw_installations SET status='active',revoked_at=NULL WHERE user_id=? AND archived_at IS NULL AND id IN ($placeholders)")
                        ->execute(array_merge([$userId], $deviceIds));
                }
                $this->db->prepare("UPDATE ssw_users SET status='active',revoked_at=NULL WHERE id=?")->execute([$userId]);
                $this->event($userId, null, 'portal.user_active', ['admin_user' => $admin, 'device_ids' => $deviceIds]);
            }
            $this->db->commit();
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function deleteUser(int $userId, string $admin): void
    {
        $user = $this->user($userId);
        if ($user['status'] === 'deleted' || preg_match('/^deleted\+[0-9]+\+[0-9]+@invalid\.local$/D', (string) $user['email']) === 1) return;
        $this->db->beginTransaction();
        try {
            $countStatement = $this->db->prepare('SELECT COUNT(*) FROM ssw_installations WHERE user_id=?');
            $countStatement->execute([$userId]);
            $deviceCount = (int) $countStatement->fetchColumn();
            $this->db->prepare("UPDATE ssw_installations SET status='revoked',revoked_at=COALESCE(revoked_at,CURRENT_TIMESTAMP),archived_at=COALESCE(archived_at,CURRENT_TIMESTAMP) WHERE user_id=?")->execute([$userId]);
            $this->db->prepare('UPDATE ssw_activation_codes SET consumed_at=COALESCE(consumed_at,CURRENT_TIMESTAMP),display_code=NULL WHERE user_id=?')->execute([$userId]);
            $this->db->prepare("UPDATE ssw_registration_invitations SET status='revoked' WHERE email=?")->execute([$user['email']]);
            $anonymousEmail = 'deleted+' . $userId . '+' . time() . '@invalid.local';
            $this->db->prepare("UPDATE ssw_users SET email=?,status='deleted',revoked_at=CURRENT_TIMESTAMP WHERE id=?")->execute([$anonymousEmail, $userId]);
            $this->event($userId, null, 'portal.user_deleted', [
                'original_user_id' => $userId,
                'email' => $user['email'],
                'company_name' => $user['company_name'],
                'first_name' => $user['first_name'],
                'last_name' => $user['last_name'],
                'device_count' => $deviceCount,
                'deleted_by' => $admin,
            ]);
            $this->db->commit();
        } catch (Throwable $exception) {
            if ($this->db->inTransaction()) $this->db->rollBack();
            throw $exception;
        }
    }

    public function setDeviceStatus(int $installationId, string $status, string $admin): void
    {
        if (!in_array($status, ['active', 'revoked'], true)) throw new RuntimeException('Stato dispositivo non valido.');
        $device = $this->db->prepare('SELECT user_id,status,device_number FROM ssw_installations WHERE id=? AND user_id IS NOT NULL');
        $device->execute([$installationId]);
        $deviceRow = $device->fetch();
        if (!is_array($deviceRow)) throw new RuntimeException('Dispositivo non trovato.');
        $userId = (int) $deviceRow['user_id'];
        if ($deviceRow['status'] === $status) return;
        if ($status === 'active') {
            $user = $this->user($userId);
            if ($user['status'] === 'revoked') throw new RuntimeException('Riattivare prima l’utente.');
            if ((int) $user['active_devices'] >= 2) throw new RuntimeException('L’utente ha già due dispositivi attivi.');
            $deviceNumber = $this->availableDeviceNumber($userId, $installationId);
        } else {
            $deviceNumber = (int) $deviceRow['device_number'];
        }
        $sql = $status === 'revoked'
            ? "UPDATE ssw_installations SET status='revoked',revoked_at=CURRENT_TIMESTAMP WHERE id=?"
            : "UPDATE ssw_installations SET status='active',device_number=?,revoked_at=NULL WHERE id=?";
        $this->db->prepare($sql)->execute($status === 'revoked' ? [$installationId] : [$deviceNumber, $installationId]);
        $this->event($userId, $installationId, 'portal.device_' . $status, ['admin_user' => $admin, 'device_number' => $deviceNumber]);
    }

    public function archiveDevice(int $installationId, string $admin): void
    {
        $statement = $this->db->prepare('SELECT user_id,status,device_number,archived_at FROM ssw_installations WHERE id=? AND user_id IS NOT NULL');
        $statement->execute([$installationId]);
        $device = $statement->fetch();
        if (!is_array($device)) throw new RuntimeException('Dispositivo non trovato.');
        if ($device['status'] !== 'revoked') throw new RuntimeException('Revocare il dispositivo prima di archiviarlo.');
        if ($device['archived_at'] !== null) return;
        $this->db->prepare('UPDATE ssw_installations SET archived_at=CURRENT_TIMESTAMP WHERE id=?')->execute([$installationId]);
        $this->event((int) $device['user_id'], $installationId, 'portal.device_archived', [
            'admin_user' => $admin,
            'device_number' => (int) $device['device_number'],
        ]);
    }

    private function user(int $userId): array
    {
        $statement = $this->db->prepare('SELECT u.*, (SELECT COUNT(*) FROM ssw_installations i WHERE i.user_id=u.id AND i.status=\'active\') AS active_devices FROM ssw_users u WHERE u.id=?');
        $statement->execute([$userId]);
        $row = $statement->fetch();
        if (!is_array($row)) throw new RuntimeException('Utente non trovato.');
        return $row;
    }

    private function availableDeviceNumber(int $userId, int $exceptInstallationId): int
    {
        $statement = $this->db->prepare("SELECT device_number FROM ssw_installations WHERE user_id=? AND status='active' AND id<>?");
        $statement->execute([$userId, $exceptInstallationId]);
        $used = array_map('intval', $statement->fetchAll(PDO::FETCH_COLUMN));
        foreach ([1, 2] as $candidate) if (!in_array($candidate, $used, true)) return $candidate;
        throw new RuntimeException('L’utente ha già due dispositivi attivi.');
    }

    private function updateWorkflowEvents(): array
    {
        $rows = $this->db->query("SELECT id,user_id,event_type,event_data,created_at FROM ssw_license_events WHERE event_type IN ('portal.update_uninstall_requested','portal.update_uninstall_confirmed','portal.update_reinstall_prepared') ORDER BY id ASC")->fetchAll();
        $workflows = [];
        foreach ($rows as $row) {
            $userId = (int) ($row['user_id'] ?? 0);
            if ($userId < 1) continue;
            $type = (string) $row['event_type'];
            $data = json_decode((string) $row['event_data'], true);
            $data = is_array($data) ? $data : [];
            if ($type === 'portal.update_uninstall_requested') {
                $workflows[$userId] = ['request' => $row];
                continue;
            }
            $requestId = (int) ($data['request_event_id'] ?? 0);
            if ($requestId < 1 || $requestId !== (int) ($workflows[$userId]['request']['id'] ?? 0)) continue;
            if ($type === 'portal.update_uninstall_confirmed') $workflows[$userId]['confirmed'] = $row;
            elseif ($type === 'portal.update_reinstall_prepared') $workflows[$userId]['reinstall'] = $row;
        }
        return $workflows;
    }

    private function updateUninstallRequestResult(int $eventId, int $expires): array
    {
        $statement = $this->db->prepare('SELECT e.user_id,u.email,u.company_name,u.first_name,u.last_name FROM ssw_license_events e JOIN ssw_users u ON u.id=e.user_id WHERE e.id=?');
        $statement->execute([$eventId]);
        $request = $statement->fetch();
        if (!is_array($request)) throw new RuntimeException('Richiesta di disinstallazione non trovata.');
        $request['event_id'] = $eventId;
        $request['activity_code'] = $this->activityCode('DIS', (string) $eventId);
        $request['expires_at'] = gmdate('Y-m-d H:i:s', $expires);
        $request['token'] = $this->updateUninstallToken($eventId, $expires);
        return $request;
    }

    private function updateUninstallToken(int $eventId, int $expires): string
    {
        $payload = $eventId . '.' . $expires;
        return $payload . '.' . hash_hmac('sha256', 'ssw-uninstall|' . $payload, $this->invitationSecret());
    }

    private function event(int $userId, ?int $installationId, string $type, array $data): void
    {
        $this->db->prepare('INSERT INTO ssw_license_events (user_id,installation_id,event_type,event_data) VALUES (?,?,?,?)')
            ->execute([$userId > 0 ? $userId : null, $installationId, $type, json_encode($data, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE)]);
    }

    private function normalizeEmail(string $email): string
    {
        $email = strtolower(trim($email));
        if (filter_var($email, FILTER_VALIDATE_EMAIL) === false || strlen($email) > 254) throw new RuntimeException('Email non valida.');
        return $email;
    }

    private function normalizeCompanyName(string $companyName): string
    {
        $companyName = trim(preg_replace('/\s+/', ' ', $companyName) ?? '');
        $length = function_exists('mb_strlen') ? mb_strlen($companyName) : strlen($companyName);
        if ($length < 2 || $length > 160) throw new RuntimeException('La società deve contenere da 2 a 160 caratteri.');
        return $companyName;
    }

    private function registrationInvitationResult(int $invitationId): array
    {
        $statement = $this->db->prepare('SELECT * FROM ssw_registration_invitations WHERE id=?');
        $statement->execute([$invitationId]);
        $invitation = $statement->fetch();
        if (!is_array($invitation)) throw new RuntimeException('Invito non trovato.');
        $invitation['token'] = $this->invitationToken($invitationId);
        $invitation['activity_code'] = $this->activityCode('REG', $invitationId . '|' . $invitation['created_at']);
        return $invitation;
    }

    private function registrationInvitationExpiry(string $createdAt): DateTimeImmutable
    {
        return (new DateTimeImmutable($createdAt, new DateTimeZone('UTC')))->modify('+7 days');
    }

    private function formatRemainingTime(DateTimeImmutable $expiry): string
    {
        $remaining = $expiry->getTimestamp() - time();
        $expired = $remaining < 0;
        $remaining = abs($remaining);
        $days = intdiv($remaining, 86400);
        $hours = intdiv($remaining % 86400, 3600);
        $minutes = intdiv($remaining % 3600, 60);
        if ($days > 0) $duration = $days . ($days === 1 ? ' giorno' : ' giorni') . ($hours > 0 ? ' e ' . $hours . ($hours === 1 ? ' ora' : ' ore') : '');
        elseif ($hours > 0) $duration = $hours . ($hours === 1 ? ' ora' : ' ore') . ($minutes > 0 ? ' e ' . $minutes . ' min' : '');
        else $duration = max(1, $minutes) . ' min';
        return ($expired ? 'Scaduto da ' : 'Scade tra ') . $duration;
    }

    private function invitationIdFromToken(string $token): int
    {
        if (!preg_match('/^([1-9][0-9]*)\.([a-f0-9]{64})$/D', trim($token), $matches)) throw new RuntimeException('Questo invito non è valido.');
        $id = (int) $matches[1];
        if (!hash_equals($this->invitationToken($id), trim($token))) throw new RuntimeException('Questo invito non è valido.');
        return $id;
    }

    private function invitationSecret(): string
    {
        $secret = function_exists('selection_env') ? (selection_env('SSW_SELECTION_RATE_SECRET', false) ?? '') : '';
        if (strlen($secret) < 16) throw new RuntimeException('La firma degli inviti non è configurata sul server.');
        return $secret;
    }

    private function activityCode(string $prefix, string $identity): string
    {
        $digest = strtoupper(substr(hash_hmac('sha256', 'ssw-mail-activity|' . $prefix . '|' . $identity, $this->invitationSecret()), 0, 6));
        return $prefix . '-' . $digest;
    }

    private function normalizePersonName(string $name, string $label): string
    {
        $name = trim(preg_replace('/\s+/', ' ', $name) ?? '');
        $length = function_exists('mb_strlen') ? mb_strlen($name) : strlen($name);
        if ($length < 1 || $length > 100) throw new RuntimeException($label . ' non è valido.');
        return $name;
    }

    private function personComparisonKey(string $name): string
    {
        $normalized = trim(preg_replace('/\s+/', ' ', $name) ?? '');
        return function_exists('mb_strtolower') ? mb_strtolower($normalized, 'UTF-8') : strtolower($normalized);
    }

    private function companyComparisonKey(string $companyName): string
    {
        $normalized = trim(preg_replace('/\s+/', ' ', $companyName) ?? '');
        return function_exists('mb_strtolower') ? mb_strtolower($normalized, 'UTF-8') : strtolower($normalized);
    }
}
