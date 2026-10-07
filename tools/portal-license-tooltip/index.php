<?php
declare(strict_types=1);

require_once __DIR__ . '/src/bootstrap.php';
portal_start();
$auth = new PortalAuth();
$action = isset($_GET['action']) ? (string) $_GET['action'] : 'dashboard';
$error = null;
$loginUsername = '';

function portal_registration_url(string $token): string
{
    return 'https://www.avensys-srl.com/ssw-portal/index.php?' . http_build_query(['action' => 'registration', 'token' => $token]);
}

function portal_registration_locale(): string
{
    $accepted = strtolower((string) ($_SERVER['HTTP_ACCEPT_LANGUAGE'] ?? 'en'));
    foreach (explode(',', $accepted) as $candidate) {
        $language = substr(trim(explode(';', $candidate)[0]), 0, 2);
        if (in_array($language, ['it', 'en', 'fr', 'de', 'es', 'da'], true)) return $language;
    }
    return 'en';
}

function portal_download_url(int $userId): string
{
    $secret = function_exists('selection_env') ? (selection_env('SSW_SELECTION_RATE_SECRET', false) ?? '') : '';
    if ($userId < 1 || strlen($secret) < 16) throw new RuntimeException('Il link di download non è configurato sul server.');
    $expires = time() + (7 * 24 * 60 * 60);
    $payload = $userId . '.' . $expires;
    $token = $payload . '.' . hash_hmac('sha256', 'ssw-download|' . $payload, $secret);
    return 'https://www.avensys-srl.com/api/ssw_download.php?' . http_build_query(['source' => 'first_download', 'download_token' => $token]);
}

function portal_update_confirmation_url(string $token): string
{
    return 'https://www.avensys-srl.com/ssw-portal/index.php?' . http_build_query(['action' => 'confirm_uninstall', 'token' => $token]);
}

function portal_add_email_activity_reference(string $subject, string $text, string $html, array $activity): array
{
    $code = preg_replace('/[^A-Z0-9-]/', '', strtoupper((string) ($activity['activity_code'] ?? '')));
    if ($code === '') return [$subject, $text, $html];
    $subject .= ' [' . $code . ']';
    $text .= "\r\nAvensys reference / Référence / Referenz: $code\r\n";
    $reference = '<div style="max-width:620px;margin:0 auto;padding:8px 12px;text-align:center;font-size:10px;color:#7b858a">Avensys reference / Référence / Referenz: <strong>' . portal_e($code) . '</strong></div>';
    $html = str_replace('</body>', $reference . '</body>', $html);
    return [$subject, $text, $html];
}

function portal_emit_uninstall_email(array $request): void
{
    $email = str_replace(["\r", "\n"], '', (string) ($request['email'] ?? ''));
    $company = (string) ($request['company_name'] ?? '');
    $expires = (string) ($request['expires_at'] ?? '');
    $url = portal_update_confirmation_url((string) ($request['token'] ?? ''));
    $subject = 'SSW reinstall preparation / Préparation de la réinstallation SSW / Vorbereitung der SSW-Neuinstallation';
    $text = "SSW REINSTALLATION\r\n\r\n🇬🇧 Please uninstall every existing SSW version from Windows and remove any old SSW shortcuts. When this is complete, open the confirmation link below. Avensys will then prepare a new installer and a safety activation code.\r\n\r\n🇫🇷 Veuillez désinstaller toutes les versions SSW présentes dans Windows et supprimer les anciens raccourcis SSW. Une fois cette opération terminée, ouvrez le lien de confirmation ci-dessous. Avensys préparera ensuite un nouvel installateur et un code d’activation de sécurité.\r\n\r\n🇩🇪 Bitte deinstallieren Sie alle vorhandenen SSW-Versionen in Windows und entfernen Sie alte SSW-Verknüpfungen. Öffnen Sie anschließend den folgenden Bestätigungslink. Avensys stellt danach ein neues Installationsprogramm und einen Sicherheits-Aktivierungscode bereit.\r\n\r\n$url\r\n\r\nLink valid until $expires UTC.\r\n";
    $html = '<!doctype html><html><body style="margin:0;padding:30px;background:#eef2f3;font-family:Segoe UI,Arial,sans-serif;color:#17242b"><table role="presentation" width="100%"><tr><td align="center"><table role="presentation" width="620" style="width:100%;max-width:620px;background:#fff;border:1px solid #d4dadd"><tr><td style="padding:24px 32px 18px;border-bottom:4px solid #25815d"><img src="cid:avensys-logo" width="190" alt="Avensys" style="display:block;border:0"><div style="margin-top:18px;font-size:12px;font-weight:700;color:#25815d">SSW LICENSE</div><h1 style="margin:5px 0 0;font-size:24px;line-height:1.4">&#127468;&#127463; Prepare SSW reinstallation<br><span style="font-size:18px;color:#5d696f">&#127467;&#127479; Préparer la réinstallation SSW</span><br><span style="font-size:18px;color:#5d696f">&#127465;&#127466; SSW-Neuinstallation vorbereiten</span></h1></td></tr><tr><td style="padding:26px 32px"><p><strong>' . portal_e($company) . '</strong></p><p style="line-height:1.55;color:#4f5d64">&#127468;&#127463; Uninstall every SSW version from Windows and remove old SSW shortcuts. Then confirm the operation using the button below.</p><p style="line-height:1.55;color:#4f5d64">&#127467;&#127479; Désinstallez toutes les versions SSW de Windows et supprimez les anciens raccourcis SSW. Confirmez ensuite l’opération avec le bouton ci-dessous.</p><p style="line-height:1.55;color:#4f5d64">&#127465;&#127466; Deinstallieren Sie alle SSW-Versionen in Windows und entfernen Sie alte SSW-Verknüpfungen. Bestätigen Sie den Vorgang anschließend über die folgende Schaltfläche.</p><p style="margin:26px 0;text-align:center"><a href="' . portal_e($url) . '" style="display:inline-block;background:#254a92;color:#fff;text-decoration:none;padding:13px 22px;font-weight:700;line-height:1.55">&#127468;&#127463; I completed the uninstall<br>&#127467;&#127479; J’ai terminé la désinstallation<br>&#127465;&#127466; Deinstallation abgeschlossen</a></p><p style="font-size:12px;color:#68767c">Link valid until ' . portal_e($expires) . ' UTC.</p><p style="font-size:12px;color:#68767c;word-break:break-all">' . portal_e($url) . '</p></td></tr></table></td></tr></table></body></html>';
    [$subject, $text, $html] = portal_add_email_activity_reference($subject, $text, $html, $request);
    $logo = file_get_contents(__DIR__ . '/assets/avensys-email-logo.jpg');
    if ($logo === false) throw new RuntimeException('Logo email non disponibile.');
    $related = 'ssw-uninstall-' . bin2hex(random_bytes(12));
    $alternative = 'ssw-uninstall-alt-' . bin2hex(random_bytes(12));
    $eml = "X-Unsent: 1\r\nTo: $email\r\nSubject: $subject\r\nMIME-Version: 1.0\r\nContent-Type: multipart/related; boundary=\"$related\"\r\n\r\n"
        . "--$related\r\nContent-Type: multipart/alternative; boundary=\"$alternative\"\r\n\r\n"
        . "--$alternative\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$text\r\n"
        . "--$alternative\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$html\r\n--$alternative--\r\n"
        . "--$related\r\nContent-Type: image/jpeg; name=\"avensys-logo.jpg\"\r\nContent-Transfer-Encoding: base64\r\nContent-ID: <avensys-logo>\r\nContent-Disposition: inline; filename=\"avensys-logo.jpg\"\r\n\r\n"
        . chunk_split(base64_encode($logo), 76, "\r\n") . "--$related--\r\n";
    header('Content-Type: message/rfc822');
    header('Content-Disposition: attachment; filename="SSW-uninstall-request-' . preg_replace('/[^a-z0-9]+/i', '-', $email) . '.eml"');
    echo $eml;
    exit;
}

function portal_emit_download_email(array $user): void
{
    $email = str_replace(["\r", "\n"], '', (string) ($user['email'] ?? ''));
    $company = (string) ($user['company_name'] ?? '');
    $url = portal_download_url((int) ($user['user_id'] ?? 0));
    $subject = 'SSW download link / Lien de téléchargement SSW / SSW-Download-Link';
    $text = "SSW DOWNLOAD\r\n\r\n🇬🇧 Hello $company, use the link below to download SSW. It is valid for 7 days and can be reused for multiple installations. The installer below can also be used to refresh an existing installation with the latest software improvements.\r\n🇫🇷 Bonjour $company, utilisez le lien ci-dessous pour télécharger SSW. Il est valable 7 jours et peut être réutilisé pour plusieurs installations. Le programme d’installation ci-dessous peut également être utilisé pour actualiser une installation existante avec les dernières améliorations.\r\n🇩🇪 Hallo $company, verwenden Sie den folgenden Link zum Download von SSW. Er ist 7 Tage gültig und kann für mehrere Installationen wiederverwendet werden. Das folgende Installationsprogramm kann auch verwendet werden, um eine bestehende Installation mit den neuesten Verbesserungen zu aktualisieren.\r\n\r\n$url\r\n\r\n🇬🇧 Support / 🇫🇷 Assistance / 🇩🇪 Support: info@avensys-srl.com\r\n";
    $html = '<!doctype html><html><body style="margin:0;padding:30px;background:#eef2f3;font-family:Segoe UI,Arial,sans-serif;color:#17242b"><table role="presentation" width="100%" cellspacing="0" cellpadding="0"><tr><td align="center"><table role="presentation" width="620" cellspacing="0" cellpadding="0" style="width:100%;max-width:620px;background:#fff;border:1px solid #d4dadd"><tr><td style="padding:24px 32px 18px;border-bottom:4px solid #25815d"><img src="cid:avensys-logo" width="190" alt="Avensys" style="display:block;width:190px;max-width:100%;height:auto;border:0"><div style="margin-top:18px;font-size:12px;font-weight:700;color:#25815d">SSW LICENSE</div><h1 style="margin:5px 0 0;font-size:25px;line-height:1.35">&#127468;&#127463; Download SSW<br><span style="color:#5d696f;font-size:19px">&#127467;&#127479; Télécharger SSW</span><br><span style="color:#5d696f;font-size:19px">&#127465;&#127466; SSW herunterladen</span></h1></td></tr><tr><td style="padding:26px 32px 28px"><p style="margin:0 0 8px"><strong>' . portal_e($company) . '</strong></p><p style="line-height:1.55;color:#4f5d64">&#127468;&#127463; The link is valid for 7 days and can be reused for multiple installations. The installer below can also be used to refresh an existing installation with the latest software improvements.</p><p style="line-height:1.55;color:#4f5d64">&#127467;&#127479; Le lien est valable 7 jours et peut être réutilisé pour plusieurs installations. Le programme d’installation ci-dessous peut également être utilisé pour actualiser une installation existante avec les dernières améliorations.</p><p style="line-height:1.55;color:#4f5d64">&#127465;&#127466; Der Link ist 7 Tage gültig und kann für mehrere Installationen wiederverwendet werden. Das folgende Installationsprogramm kann auch verwendet werden, um eine bestehende Installation mit den neuesten Verbesserungen zu aktualisieren.</p><p style="margin:26px 0 10px;text-align:center"><a href="' . portal_e($url) . '" style="display:inline-block;background:#254a92;color:#fff;text-decoration:none;padding:13px 22px;font-weight:700;line-height:1.55">&#127468;&#127463; Download SSW<br>&#127467;&#127479; Télécharger SSW<br>&#127465;&#127466; SSW herunterladen</a></p><p style="font-size:12px;color:#68767c;word-break:break-all">' . portal_e($url) . '</p></td></tr><tr><td style="padding:17px 32px;background:#f6f8f8;border-top:1px solid #e1e5e7;font-size:12px;line-height:1.5;color:#68767c"><strong style="color:#17242b">Avensys S.r.l.</strong><br>Advanced ventilation systems<br><a href="mailto:info@avensys-srl.com" style="color:#254a92">info@avensys-srl.com</a> &nbsp;|&nbsp; <a href="https://www.avensys-srl.com" style="color:#254a92">www.avensys-srl.com</a></td></tr></table></td></tr></table></body></html>';
    if (isset($_GET['reinstall']) && (string) $_GET['reinstall'] === '1') {
        $subject = 'SSW reinstall download / Téléchargement de réinstallation SSW / SSW-Neuinstallation herunterladen';
        $text = "SSW REINSTALLATION\r\n\r\n🇬🇧 Your uninstall confirmation has been received. Install SSW using the link below. No additional PIN was generated because both device slots are already assigned; contact Avensys only if SSW requests a new activation.\r\n\r\n🇫🇷 Votre confirmation de désinstallation a été reçue. Installez SSW avec le lien ci-dessous. Aucun PIN supplémentaire n’a été généré car les deux emplacements sont déjà attribués ; contactez Avensys uniquement si SSW demande une nouvelle activation.\r\n\r\n🇩🇪 Ihre Deinstallationsbestätigung ist eingegangen. Installieren Sie SSW über den folgenden Link. Es wurde keine zusätzliche PIN erstellt, da beide Geräteplätze bereits belegt sind; kontaktieren Sie Avensys nur, wenn SSW eine neue Aktivierung verlangt.\r\n\r\n$url\r\n";
        $notice = '<div style="margin:0 0 20px;padding:14px;background:#edf7f2;border-left:4px solid #25815d;line-height:1.55;color:#35444b">&#127468;&#127463; Uninstall confirmed. No additional PIN was generated because both device slots are already assigned.<br><br>&#127467;&#127479; Désinstallation confirmée. Aucun PIN supplémentaire n’a été généré car les deux emplacements sont déjà attribués.<br><br>&#127465;&#127466; Deinstallation bestätigt. Es wurde keine zusätzliche PIN erstellt, da beide Geräteplätze bereits belegt sind.</div>';
        $html = str_replace('<p style="margin:0 0 8px"><strong>', $notice . '<p style="margin:0 0 8px"><strong>', $html);
    }
    [$subject, $text, $html] = portal_add_email_activity_reference($subject, $text, $html, $user);
    $logo = file_get_contents(__DIR__ . '/assets/avensys-email-logo.jpg');
    if ($logo === false) throw new RuntimeException('Logo email non disponibile.');
    $boundary = 'ssw-download-' . bin2hex(random_bytes(12));
    $alternativeBoundary = 'ssw-download-alt-' . bin2hex(random_bytes(12));
    $eml = "X-Unsent: 1\r\nTo: $email\r\nSubject: $subject\r\nMIME-Version: 1.0\r\nContent-Type: multipart/related; boundary=\"$boundary\"\r\n\r\n"
        . "--$boundary\r\nContent-Type: multipart/alternative; boundary=\"$alternativeBoundary\"\r\n\r\n"
        . "--$alternativeBoundary\r\nContent-Type: text/plain; charset=UTF-8\r\n\r\n$text\r\n"
        . "--$alternativeBoundary\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n$html\r\n"
        . "--$alternativeBoundary--\r\n"
        . "--$boundary\r\nContent-Type: image/jpeg; name=\"avensys-logo.jpg\"\r\nContent-Transfer-Encoding: base64\r\nContent-ID: <avensys-logo>\r\nContent-Disposition: inline; filename=\"avensys-logo.jpg\"\r\n\r\n"
        . chunk_split(base64_encode($logo), 76, "\r\n") . "--$boundary--\r\n";
    header('Content-Type: message/rfc822');
    header('Content-Disposition: attachment; filename="SSW-download-' . preg_replace('/[^a-z0-9]+/i', '-', $email) . '.eml"');
    echo $eml;
    exit;
}

function portal_emit_registration_email(array $invitation): void
{
    $email = str_replace(["\r", "\n"], '', (string) ($invitation['email'] ?? ''));
    $company = (string) ($invitation['company_name'] ?? '');
    $url = portal_registration_url((string) ($invitation['token'] ?? ''));
    $subject = 'Complete your SSW registration / Finalisez votre inscription SSW / SSW-Registrierung abschliessen';
    $text = "SSW REGISTRATION\r\n\r\n🇬🇧 Avensys has prepared an SSW registration invitation for $company.\r\n🇬🇧 Complete the form using the email address $email.\r\n🇬🇧 After submission, wait for Avensys approval. The same link will then show the authorized SSW download button.\r\n\r\n🇫🇷 Avensys a préparé une invitation d’inscription SSW pour $company.\r\n🇫🇷 Complétez le formulaire avec l’adresse e-mail $email.\r\n🇫🇷 Après l’envoi, attendez la validation d’Avensys. Le même lien affichera ensuite le bouton autorisé de téléchargement SSW.\r\n\r\n🇩🇪 Avensys hat eine Einladung zur SSW-Registrierung für $company vorbereitet.\r\n🇩🇪 Füllen Sie das Formular mit der E-Mail-Adresse $email aus.\r\n🇩🇪 Warten Sie nach dem Absenden auf die Freigabe durch Avensys. Derselbe Link zeigt danach die autorisierte SSW-Download-Schaltfläche.\r\n\r\n🇬🇧 Registration and download link / 🇫🇷 Lien d’inscription et de téléchargement / 🇩🇪 Registrierungs- und Download-Link:\r\n$url\r\n\r\n🇬🇧 Assistance: info@avensys-srl.com\r\n🇫🇷 Assistance : info@avensys-srl.com\r\n🇩🇪 Support: info@avensys-srl.com\r\n";
    $html = '<!doctype html><html><body style="margin:0;padding:0;background:#eef2f3;font-family:Segoe UI,Arial,sans-serif;color:#17242b"><table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#eef2f3;padding:30px 12px"><tr><td align="center"><table role="presentation" width="620" cellspacing="0" cellpadding="0" style="width:100%;max-width:620px;background:#fff;border:1px solid #d4dadd"><tr><td style="padding:24px 32px 18px;border-bottom:4px solid #25815d"><img src="cid:avensys-logo" width="190" alt="Avensys" style="display:block;width:190px;max-width:100%;height:auto;border:0"><div style="margin-top:18px;font-size:12px;font-weight:700;color:#25815d">SSW LICENSE</div><h1 style="margin:5px 0 0;font-size:25px;line-height:1.35">&#127468;&#127463; Complete your registration<br><span style="color:#5d696f;font-size:19px">&#127467;&#127479; Finalisez votre inscription</span><br><span style="color:#5d696f;font-size:19px">&#127465;&#127466; Schließen Sie Ihre Registrierung ab</span></h1></td></tr><tr><td style="padding:26px 32px 10px"><p style="margin:0 0 8px"><strong>' . portal_e($company) . '</strong></p><p style="margin:0;color:#5d696f">' . portal_e($email) . '</p></td></tr><tr><td style="padding:12px 32px 28px"><p style="line-height:1.55">&#127468;&#127463; Avensys has prepared your SSW registration. Complete the form and wait for approval. After approval, this same link also provides the authorized download for reinstalling SSW.</p><p style="line-height:1.55">&#127467;&#127479; Avensys a préparé votre inscription SSW. Complétez le formulaire et attendez sa validation. Après validation, ce même lien permet aussi de télécharger SSW pour le réinstaller.</p><p style="line-height:1.55">&#127465;&#127466; Avensys hat Ihre SSW-Registrierung vorbereitet. Füllen Sie das Formular aus und warten Sie auf die Freigabe. Danach ermöglicht derselbe Link auch den Download zur Neuinstallation von SSW.</p><p style="margin:24px 0 10px;text-align:center"><a href="' . portal_e($url) . '" style="display:inline-block;background:#254a92;color:#fff;text-decoration:none;padding:13px 22px;font-weight:700;line-height:1.55">&#127468;&#127463; Registration and download<br>&#127467;&#127479; Inscription et téléchargement<br>&#127465;&#127466; Registrierung und Download</a></p><p style="font-size:12px;color:#68767c;word-break:break-all">' . portal_e($url) . '</p></td></tr><tr><td style="padding:17px 32px;background:#f6f8f8;border-top:1px solid #e1e5e7;font-size:12px;line-height:1.5;color:#68767c"><strong style="color:#17242b">Avensys S.r.l.</strong><br>Advanced ventilation systems<br><a href="mailto:info@avensys-srl.com" style="color:#254a92">info@avensys-srl.com</a></td></tr></table></td></tr></table></body></html>';
    [$subject, $text, $html] = portal_add_email_activity_reference($subject, $text, $html, $invitation);
    $logo = file_get_contents(__DIR__ . '/assets/avensys-email-logo.jpg');
    if ($logo === false) throw new RuntimeException('Logo email non disponibile.');
    $relatedBoundary = 'ssw-related-' . bin2hex(random_bytes(12));
    $alternativeBoundary = 'ssw-alternative-' . bin2hex(random_bytes(12));
    $eml = "X-Unsent: 1\r\nTo: $email\r\nSubject: $subject\r\nMIME-Version: 1.0\r\nContent-Type: multipart/related; boundary=\"$relatedBoundary\"\r\n\r\n"
        . "--$relatedBoundary\r\nContent-Type: multipart/alternative; boundary=\"$alternativeBoundary\"\r\n\r\n"
        . "--$alternativeBoundary\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$text\r\n"
        . "--$alternativeBoundary\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$html\r\n"
        . "--$alternativeBoundary--\r\n--$relatedBoundary\r\nContent-Type: image/jpeg; name=\"avensys-logo.jpg\"\r\nContent-Transfer-Encoding: base64\r\nContent-ID: <avensys-logo>\r\nContent-Disposition: inline; filename=\"avensys-logo.jpg\"\r\n\r\n"
        . chunk_split(base64_encode($logo), 76, "\r\n") . "--$relatedBoundary--\r\n";
    header('Content-Type: message/rfc822');
    header('Content-Disposition: attachment; filename="SSW-registration-' . preg_replace('/[^a-z0-9]+/i', '-', $email) . '.eml"');
    echo $eml;
    exit;
}

try {
    if ($action === 'logout') {
        if ($_SERVER['REQUEST_METHOD'] === 'POST') portal_verify_csrf();
        $auth->logout();
        header('Location: ' . portal_url());
        exit;
    }
    if ($action === 'registration') {
        $licenses = new LicenseRepository(selection_database());
        $token = trim((string) ($_GET['token'] ?? $_POST['token'] ?? ''));
        $registration = null;
        $registrationError = null;
        try {
            if ($token === '') throw new RuntimeException('A personal invitation link is required. Please contact Avensys.');
            if ($_SERVER['REQUEST_METHOD'] === 'POST') {
                portal_verify_csrf();
                $registration = $licenses->submitRegistrationInvitation($token,
                    (string) ($_POST['first_name'] ?? ''), (string) ($_POST['last_name'] ?? ''),
                    (string) ($_POST['email'] ?? ''), (string) ($_POST['company_name'] ?? ''),
                    isset($_POST['privacy_accepted']) && (string) $_POST['privacy_accepted'] === '1');
            } else {
                $registration = $licenses->registrationInvitation($token);
            }
        } catch (Throwable $exception) {
            $registrationError = $exception->getMessage();
        }
        portal_render('registration', ['pageTitle' => 'SSW registration', 'pageLanguage' => portal_registration_locale(),
            'authenticated' => false, 'registration' => $registration, 'registrationToken' => $token,
            'registrationError' => $registrationError, 'publicPage' => true]);
        exit;
    }
    if ($action === 'confirm_uninstall') {
        $licenses = new LicenseRepository(selection_database());
        $token = trim((string) ($_GET['token'] ?? $_POST['token'] ?? ''));
        $confirmation = null;
        $confirmationError = null;
        try {
            if ($token === '') throw new RuntimeException('A personal confirmation link is required.');
            $confirm = $_SERVER['REQUEST_METHOD'] === 'POST';
            if ($confirm) portal_verify_csrf();
            $confirmation = $licenses->updateUninstallConfirmation($token, $confirm);
        } catch (Throwable $exception) {
            $confirmationError = $exception->getMessage();
        }
        portal_render('uninstall-confirmation', ['pageTitle' => 'SSW uninstall confirmation', 'authenticated' => false,
            'confirmation' => $confirmation, 'confirmationToken' => $token,
            'confirmationError' => $confirmationError, 'publicPage' => true]);
        exit;
    }
    if (!$auth->isAuthenticated()) {
        if ($_SERVER['REQUEST_METHOD'] === 'POST') {
            $loginUsername = trim((string) ($_POST['username'] ?? ''));
            if ($auth->login($loginUsername, (string) ($_POST['password'] ?? ''))) {
                header('Location: ' . portal_url());
                exit;
            } else {
                $error = 'Credenziali non valide.';
            }
        }
        portal_render('login', ['pageTitle' => 'Accesso', 'error' => $error, 'authenticated' => false, 'loginUsername' => $loginUsername]);
        exit;
    }

    $repository = new SelectionRepository(selection_database());
    if (in_array($action, ['push_config', 'push_subscribe', 'push_unsubscribe', 'license_notification_count'], true)) {
        $licenses = new LicenseRepository(selection_database());
        header('Content-Type: application/json; charset=utf-8');
        if ($action === 'push_config') {
            $key = $licenses->pushPublicKey();
            echo json_encode(['enabled' => $key !== '', 'publicKey' => $key]);
            exit;
        }
        if ($action === 'license_notification_count') {
            $count = $licenses->unreadNotificationCount();
            echo json_encode(['count' => $count, 'preview' => $count > 0 ? $licenses->notificationPreview() : '']);
            exit;
        }
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') throw new RuntimeException('Metodo non consentito.');
        portal_verify_csrf();
        $subscription = json_decode((string) ($_POST['subscription'] ?? ''), true);
        if (!is_array($subscription)) throw new RuntimeException('Sottoscrizione notifiche non valida.');
        if ($action === 'push_subscribe') $licenses->savePushSubscription($subscription);
        else $licenses->removePushSubscription((string) ($subscription['endpoint'] ?? ''));
        echo json_encode(['ok' => true]);
        exit;
    }
    if ($action === 'registration_invitation_email') {
        $licenses = new LicenseRepository(selection_database());
        $invitation = isset($_GET['invitation_id'])
            ? $licenses->registrationInvitationForAdmin((int) $_GET['invitation_id'])
            : ($_SESSION['registration_invitation_email'] ?? null);
        if (!is_array($invitation)) throw new RuntimeException('Nessun invito di registrazione disponibile.');
        portal_emit_registration_email($invitation);
    }
    if ($action === 'license_email') {
        $mail = isset($_GET['user_id'])
            ? (new LicenseRepository(selection_database()))->pendingActivation((int) $_GET['user_id'])
            : ($_SESSION['license_activation_email'] ?? null);
        if (!is_array($mail)) throw new RuntimeException('Nessun codice di attivazione disponibile per il download.');
        $email = str_replace(["\r", "\n"], '', (string) ($mail['email'] ?? ''));
        $pin = preg_replace('/[^0-9]/', '', (string) ($mail['pin'] ?? ''));
        $expires = (string) ($mail['expires_at'] ?? '');
        $downloadUrl = portal_download_url((int) ($mail['user_id'] ?? 0));
        $relatedBoundary = 'ssw-related-' . bin2hex(random_bytes(12));
        $alternativeBoundary = 'ssw-alternative-' . bin2hex(random_bytes(12));
        $subject = 'Your SSW activation code / Votre code d activation SSW / Ihr SSW-Aktivierungscode';
        $text = "SSW ACTIVATION\r\n\r\n🇬🇧 Your personal activation code is: $pin\r\n🇬🇧 Enter it in SSW to complete the activation. The code is valid until $expires UTC and can be used only once.\r\n\r\n🇫🇷 Votre code d’activation personnel est : $pin\r\n🇫🇷 Saisissez-le dans SSW pour terminer l’activation. Le code est valable jusqu’au $expires UTC et ne peut être utilisé qu’une seule fois.\r\n\r\n🇩🇪 Ihr persönlicher Aktivierungscode lautet: $pin\r\n🇩🇪 Geben Sie ihn in SSW ein, um die Aktivierung abzuschließen. Der Code ist bis $expires UTC gültig und kann nur einmal verwendet werden.\r\n\r\n🇬🇧 Assistance: info@avensys-srl.com\r\n🇫🇷 Assistance : info@avensys-srl.com\r\n🇩🇪 Support: info@avensys-srl.com\r\n";
        $html = '<!doctype html><html><body style="margin:0;padding:0;background:#eef2f3;font-family:Segoe UI,Arial,sans-serif;color:#17242b"><table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#eef2f3;padding:30px 12px"><tr><td align="center"><table role="presentation" width="620" cellspacing="0" cellpadding="0" style="width:100%;max-width:620px;background:#ffffff;border:1px solid #d4dadd"><tr><td style="padding:24px 32px 18px;border-bottom:4px solid #25815d"><img src="cid:avensys-logo" width="190" alt="Avensys" style="display:block;width:190px;max-width:100%;height:auto;border:0"><div style="margin-top:18px;font-size:12px;font-weight:700;color:#25815d">SSW LICENSE</div><h1 style="margin:5px 0 0;font-size:25px;line-height:1.35;font-weight:650">&#127468;&#127463; Activation code<br><span style="color:#5d696f;font-size:19px">&#127467;&#127479; Code d’activation</span><br><span style="color:#5d696f;font-size:19px">&#127465;&#127466; Aktivierungscode</span></h1></td></tr><tr><td style="padding:28px 32px 12px"><div style="padding:20px 12px;text-align:center;background:#f1f7f4;border:2px solid #25815d"><div style="margin-bottom:8px;font-size:12px;line-height:1.55;color:#5d696f">&#127468;&#127463; YOUR PERSONAL CODE<br>&#127467;&#127479; VOTRE CODE PERSONNEL<br>&#127465;&#127466; IHR PERSÖNLICHER CODE</div><strong style="font-size:34px;letter-spacing:8px;color:#17242b">' . portal_e($pin) . '</strong></div></td></tr><tr><td style="padding:12px 32px 26px"><p style="margin:7px 0 14px;line-height:1.55;color:#4f5d64">&#127468;&#127463; Enter the code above in SSW to complete the activation. The code is valid until <strong>' . portal_e($expires) . ' UTC</strong> and can be used only once.</p><p style="margin:7px 0 14px;line-height:1.55;color:#4f5d64">&#127467;&#127479; Saisissez le code ci-dessus dans SSW pour terminer l’activation. Le code est valable jusqu’au <strong>' . portal_e($expires) . ' UTC</strong> et ne peut être utilisé qu’une seule fois.</p><p style="margin:7px 0 0;line-height:1.55;color:#4f5d64">&#127465;&#127466; Geben Sie den obigen Code in SSW ein, um die Aktivierung abzuschließen. Der Code ist bis <strong>' . portal_e($expires) . ' UTC</strong> gültig und kann nur einmal verwendet werden.</p></td></tr><tr><td style="padding:17px 32px;background:#f6f8f8;border-top:1px solid #e1e5e7;font-size:12px;line-height:1.5;color:#68767c"><strong style="color:#17242b">Avensys S.r.l.</strong><br>Advanced ventilation systems<br><a href="mailto:info@avensys-srl.com" style="color:#254a92">info@avensys-srl.com</a> &nbsp;|&nbsp; <a href="https://www.avensys-srl.com" style="color:#254a92">www.avensys-srl.com</a></td></tr></table></td></tr></table></body></html>';
        $html = str_replace('</td></tr><tr><td style="padding:17px 32px;background:#f6f8f8', '<p style="margin:22px 0 0;text-align:center"><a href="' . portal_e($downloadUrl) . '" style="display:inline-block;background:#254a92;color:#fff;text-decoration:none;padding:12px 20px;font-weight:700">Download SSW / Télécharger SSW / SSW herunterladen</a></p></td></tr><tr><td style="padding:17px 32px;background:#f6f8f8', $html);
        $text .= "\r\nDownload SSW (valid for 7 days) / Télécharger SSW (valable 7 jours) / SSW herunterladen (7 Tage gültig): $downloadUrl\r\n";
        if (isset($_GET['reinstall']) && (string) $_GET['reinstall'] === '1') {
            $subject = 'SSW reinstall package and safety PIN / Réinstallation SSW et PIN / SSW-Neuinstallation und PIN';
            $guidanceText = "🇬🇧 Your uninstall confirmation has been received. Download and install SSW from the link below. Use the PIN only if SSW requests activation on first launch.\r\n\r\n🇫🇷 Votre confirmation de désinstallation a été reçue. Téléchargez et installez SSW avec le lien ci-dessous. Utilisez le PIN uniquement si SSW demande l’activation au premier démarrage.\r\n\r\n🇩🇪 Ihre Deinstallationsbestätigung ist eingegangen. Laden Sie SSW über den folgenden Link herunter und installieren Sie es. Verwenden Sie die PIN nur, wenn SSW beim ersten Start eine Aktivierung verlangt.\r\n\r\n";
            $text = "SSW REINSTALLATION\r\n\r\n" . $guidanceText . $text;
            $guidanceHtml = '<div style="margin:0 32px 14px;padding:14px;background:#edf7f2;border-left:4px solid #25815d;color:#35444b;line-height:1.55"><p style="margin:0 0 9px">&#127468;&#127463; Your uninstall confirmation has been received. Use the PIN only if SSW requests activation on first launch.</p><p style="margin:0 0 9px">&#127467;&#127479; Votre confirmation de désinstallation a été reçue. Utilisez le PIN uniquement si SSW demande l’activation au premier démarrage.</p><p style="margin:0">&#127465;&#127466; Ihre Deinstallationsbestätigung ist eingegangen. Verwenden Sie die PIN nur, wenn SSW beim ersten Start eine Aktivierung verlangt.</p></div>';
            $html = str_replace('<tr><td style="padding:28px 32px 12px">', '<tr><td>' . $guidanceHtml . '</td></tr><tr><td style="padding:28px 32px 12px">', $html);
        }
        [$subject, $text, $html] = portal_add_email_activity_reference($subject, $text, $html, $mail);
        $logoPath = __DIR__ . '/assets/avensys-email-logo.jpg';
        $logo = is_file($logoPath) ? file_get_contents($logoPath) : false;
        if ($logo === false) throw new RuntimeException('Logo email non disponibile.');
        $eml = "X-Unsent: 1\r\nTo: $email\r\nSubject: $subject\r\nMIME-Version: 1.0\r\nContent-Type: multipart/related; boundary=\"$relatedBoundary\"\r\n\r\n"
            . "--$relatedBoundary\r\nContent-Type: multipart/alternative; boundary=\"$alternativeBoundary\"\r\n\r\n"
            . "--$alternativeBoundary\r\nContent-Type: text/plain; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$text\r\n"
            . "--$alternativeBoundary\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\n\r\n$html\r\n"
            . "--$alternativeBoundary--\r\n"
            . "--$relatedBoundary\r\nContent-Type: image/jpeg; name=\"avensys-logo.jpg\"\r\nContent-Transfer-Encoding: base64\r\nContent-ID: <avensys-logo>\r\nContent-Disposition: inline; filename=\"avensys-logo.jpg\"\r\n\r\n"
            . chunk_split(base64_encode($logo), 76, "\r\n")
            . "--$relatedBoundary--\r\n";
        header('Content-Type: message/rfc822');
        header('Content-Disposition: attachment; filename="SSW-activation-' . preg_replace('/[^a-z0-9]+/i', '-', $email) . '.eml"');
        echo $eml;
        exit;
    }
    if ($action === 'download_invitation_email') {
        $licenses = new LicenseRepository(selection_database());
        $reinstall = isset($_GET['reinstall']) && (string) $_GET['reinstall'] === '1';
        $user = isset($_GET['user_id'])
            ? $licenses->downloadInvitation((int) $_GET['user_id'], $reinstall)
            : ($_SESSION['reinstall_download_email'] ?? null);
        if (!is_array($user)) throw new RuntimeException('Utente attivo non trovato.');
        portal_emit_download_email($user);
    }
    if ($action === 'update_uninstall_email') {
        $licenses = new LicenseRepository(selection_database());
        $request = isset($_GET['user_id'])
            ? $licenses->updateUninstallRequestForAdmin((int) $_GET['user_id'])
            : ($_SESSION['update_uninstall_email'] ?? null);
        if (!is_array($request)) throw new RuntimeException('Nessuna richiesta di disinstallazione disponibile.');
        portal_emit_uninstall_email($request);
    }
    if ($action === 'licenses') {
        if (!LicenseRepository::isAvailable(selection_database())) throw new RuntimeException('La gestione licenze non è ancora stata attivata sul database.');
        $licenses = new LicenseRepository(selection_database());
        $activation = null;
        $registrationInvitation = null;
        if ($_SERVER['REQUEST_METHOD'] === 'POST') {
            portal_verify_csrf();
            $operation = (string) ($_POST['operation'] ?? '');
            if ($operation === 'create_user') $activation = $licenses->createUser((string) ($_POST['company_name'] ?? ''), (string) ($_POST['email'] ?? ''), $auth->user());
            elseif ($operation === 'create_registration_invitation') {
                $registrationInvitation = $licenses->createRegistrationInvitation((string) ($_POST['company_name'] ?? ''), (string) ($_POST['email'] ?? ''), $auth->user(), isset($_POST['allow_archived']));
                $_SESSION['registration_invitation_email'] = $registrationInvitation;
                header('Location: ' . portal_url(['action' => 'registration_invitation_email']));
                exit;
            }
            elseif ($operation === 'renew_registration_invitation') {
                $registrationInvitation = $licenses->renewRegistrationInvitation((int) ($_POST['invitation_id'] ?? 0), $auth->user());
                $_SESSION['registration_invitation_email'] = $registrationInvitation;
                header('Location: ' . portal_url(['action' => 'registration_invitation_email']));
                exit;
            }
            elseif ($operation === 'revoke_registration_invitation') {
                $licenses->revokeRegistrationInvitation((int) ($_POST['invitation_id'] ?? 0), $auth->user());
                header('Location: ' . portal_url(['action' => 'licenses']));
                exit;
            }
            elseif ($operation === 'approve_registration_invitation') {
                $approval = $licenses->approveRegistrationInvitation((int) ($_POST['invitation_id'] ?? 0), $auth->user());
                $activation = $approval['activation'];
                $_SESSION['license_activation_email'] = $activation;
                header('Location: ' . portal_url(['action' => 'license_email']));
                exit;
            }
            elseif ($operation === 'approve_request') {
                $activation = $licenses->approveActivationRequest((int) ($_POST['request_event_id'] ?? 0), $auth->user());
                $_SESSION['license_activation_email'] = $activation;
                header('Location: ' . portal_url(['action' => 'license_email']));
                exit;
            }
            elseif ($operation === 'update_company') $licenses->updateCompany((int) ($_POST['user_id'] ?? 0), (string) ($_POST['company_name'] ?? ''), $auth->user());
            elseif ($operation === 'new_code') $activation = $licenses->newCode((int) ($_POST['user_id'] ?? 0), $auth->user());
            elseif ($operation === 'new_code_email') {
                $activation = $licenses->newCode((int) ($_POST['user_id'] ?? 0), $auth->user());
                $_SESSION['license_activation_email'] = $activation;
                header('Location: ' . portal_url(['action' => 'license_email']));
                exit;
            }
            elseif ($operation === 'request_update_uninstall') {
                $request = $licenses->requestUpdateUninstall((int) ($_POST['user_id'] ?? 0), $auth->user());
                $_SESSION['update_uninstall_email'] = $request;
                header('Location: ' . portal_url(['action' => 'update_uninstall_email']));
                exit;
            }
            elseif ($operation === 'prepare_reinstall_email') {
                $userId = (int) ($_POST['user_id'] ?? 0);
                $requestEventId = (int) ($_POST['request_event_id'] ?? 0);
                $licenses->assertReinstallReady($userId, $requestEventId);
                $downloadUser = $licenses->downloadInvitation($userId);
                if (!is_array($downloadUser)) throw new RuntimeException('Utente attivo non trovato.');
                if ((int) ($downloadUser['active_devices'] ?? 0) >= 2) {
                    $downloadUser['activity_code'] = $licenses->markReinstallPrepared($userId, $requestEventId, $auth->user());
                    $_SESSION['reinstall_download_email'] = $downloadUser;
                    header('Location: ' . portal_url(['action' => 'download_invitation_email', 'reinstall' => 1]));
                    exit;
                }
                $activation = $licenses->newCode($userId, $auth->user());
                $licenses->markReinstallPrepared($userId, $requestEventId, $auth->user());
                $_SESSION['license_activation_email'] = $activation;
                header('Location: ' . portal_url(['action' => 'license_email', 'reinstall' => 1]));
                exit;
            }
            elseif ($operation === 'add_problem_log') $licenses->addProblemLog((int) ($_POST['user_id'] ?? 0), (string) ($_POST['problem'] ?? ''), (string) ($_POST['solution'] ?? ''), (string) ($_POST['problem_status'] ?? 'proposed'), $auth->user());
            elseif ($operation === 'user_status') $licenses->setUserStatus((int) ($_POST['user_id'] ?? 0), (string) ($_POST['status'] ?? ''), $auth->user());
            elseif ($operation === 'delete_user') {
                $licenses->deleteUser((int) ($_POST['user_id'] ?? 0), $auth->user());
                header('Location: ' . portal_url(['action' => 'licenses']));
                exit;
            }
            elseif ($operation === 'remove_duplicate_deleted_users') {
                $removed = $licenses->removeDuplicateDeletedUserEvents();
                $message = $removed === 1 ? 'Rimosso definitivamente 1 duplicato dall’archivio.' : "Rimossi definitivamente $removed duplicati dall’archivio.";
            }
            elseif ($operation === 'device_status') $licenses->setDeviceStatus((int) ($_POST['installation_id'] ?? 0), (string) ($_POST['status'] ?? ''), $auth->user());
            elseif ($operation === 'archive_device') $licenses->archiveDevice((int) ($_POST['installation_id'] ?? 0), $auth->user());
            else throw new RuntimeException('Operazione licenza non valida.');
            if (is_array($activation)) $_SESSION['license_activation_email'] = $activation;
            if (is_array($registrationInvitation)) $_SESSION['registration_invitation_email'] = $registrationInvitation;
        }
        $licenseUserChanges = $licenses->unreadUserChanges();
        $licenses->markNotificationsRead($auth->user());
        $licenseProblemLogs = [];
        foreach ($licenses->users() as $problemUser) $licenseProblemLogs[(int) $problemUser['id']] = $licenses->problemLog((int) $problemUser['id']);
        portal_render('licenses', ['pageTitle' => 'Utenti e licenze', 'authenticated' => true, 'user' => $auth->user(),
            'licenseUsers' => $licenses->users(), 'customers' => $licenses->customers(), 'activation' => $activation,
            'activationRequests' => $licenses->activationRequests(), 'registrationRequests' => $licenses->registrationRequests(),
            'registrationInvitations' => $licenses->registrationInvitations(), 'registrationInvitation' => $registrationInvitation,
            'pendingUsers' => $licenses->pendingUsers(), 'deletedUsers' => $licenses->deletedUsers(),
            'updateInvitationUsers' => $licenses->updateInvitationUsers(),
            'licenseProblemLogs' => $licenseProblemLogs,
            'licenseUserChanges' => $licenseUserChanges,
            'licenseNotificationCount' => 0]);
        exit;
    }
    $includeUnlocated = isset($_GET['include_unlocated']) && (string) $_GET['include_unlocated'] === '1';
    if ($action === 'projects') {
        portal_render('projects', ['pageTitle' => 'Progetti', 'authenticated' => true, 'user' => $auth->user(),
            'projects' => $repository->projects($includeUnlocated), 'includeUnlocated' => $includeUnlocated]);
        exit;
    }
    if ($action === 'project') {
        $project = $repository->projectDetail((string) ($_GET['id'] ?? ''));
        if ($project === null) { http_response_code(404); throw new RuntimeException('Progetto non trovato.'); }
        portal_render('project', ['pageTitle' => 'Dettaglio progetto', 'authenticated' => true,
            'user' => $auth->user(), 'project' => $project, 'includeUnlocated' => $includeUnlocated]);
        exit;
    }
    if ($action === 'downloads') {
        portal_render('downloads', ['pageTitle' => 'Download SSW', 'authenticated' => true,
            'user' => $auth->user(), 'summary' => $repository->downloadSummary($includeUnlocated),
            'downloads' => $repository->downloads(200, $includeUnlocated), 'includeUnlocated' => $includeUnlocated]);
        exit;
    }
    if ($action === 'detail') {
        $reference = (string) ($_GET['reference'] ?? '');
        $revision = isset($_GET['revision']) ? max(1, (int) $_GET['revision']) : null;
        $detail = $repository->detail($reference, $revision);
        if ($detail === null) { http_response_code(404); throw new RuntimeException('Selezione tecnica non trovata.'); }
        $repository->audit('portal.selection_viewed', $auth->user(), (int) $detail['id'], ['revision' => (int) $detail['selected']['revision_number']]);
        portal_render('detail', ['pageTitle' => 'Dettaglio selezione', 'authenticated' => true, 'user' => $auth->user(), 'detail' => $detail]);
        exit;
    }
    if ($action === 'download') {
        $reference = (string) ($_GET['reference'] ?? '');
        $revision = isset($_GET['revision']) ? max(1, (int) $_GET['revision']) : null;
        $detail = $repository->detail($reference, $revision);
        if ($detail === null) { http_response_code(404); throw new RuntimeException('Selezione tecnica non trovata.'); }
        $repository->audit('portal.selection_downloaded', $auth->user(), (int) $detail['id'], ['revision' => (int) $detail['selected']['revision_number']]);
        header('Content-Type: application/json; charset=utf-8');
        header('Content-Disposition: attachment; filename="' . SelectionPresenter::reference($detail['public_reference'], (int) $detail['selected']['revision_number']) . '.json"');
        echo json_encode(['selection' => $detail, 'exported_at_utc' => gmdate('c')], JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
        exit;
    }
    if ($action === 'compare') {
        $reference = (string) ($_GET['reference'] ?? '');
        $toRevision = max(2, (int) ($_GET['to'] ?? 2));
        $fromRevision = max(1, (int) ($_GET['from'] ?? ($toRevision - 1)));
        $before = $repository->detail($reference, $fromRevision);
        $after = $repository->detail($reference, $toRevision);
        if ($before === null || $after === null || (int) $before['id'] !== (int) $after['id']) {
            http_response_code(404);
            throw new RuntimeException('Revisioni da confrontare non disponibili.');
        }
        $repository->audit('portal.revisions_compared', $auth->user(), (int) $after['id'], ['from' => $fromRevision, 'to' => $toRevision]);
        portal_render('compare', ['pageTitle' => 'Confronto revisioni', 'authenticated' => true, 'user' => $auth->user(), 'before' => $before, 'after' => $after]);
        exit;
    }

    $page = max(1, (int) ($_GET['page'] ?? 1));
    $filters = [
        'q' => (string) ($_GET['q'] ?? ''), 'customer' => (string) ($_GET['customer'] ?? ''),
        'version' => (string) ($_GET['version'] ?? ''), 'country' => (string) ($_GET['country'] ?? ''),
        'from' => (string) ($_GET['from'] ?? ''), 'to' => (string) ($_GET['to'] ?? ''),
        'include_unlocated' => $includeUnlocated,
    ];
    portal_render('dashboard', [
        'pageTitle' => 'Selezioni tecniche', 'authenticated' => true, 'user' => $auth->user(),
        'stats' => $repository->stats($includeUnlocated), 'availableFilters' => $repository->filters(),
        'results' => $repository->search($filters, $page, 30), 'filters' => $filters,
    ]);
} catch (Throwable $exception) {
    error_log('ssw_portal: ' . get_class($exception) . ': ' . $exception->getMessage());
    portal_render('error', ['pageTitle' => 'Errore', 'authenticated' => $auth->isAuthenticated(), 'user' => $auth->user(), 'message' => $exception->getMessage()]);
}
