<!doctype html>
<html lang="<?= portal_e($pageLanguage ?? 'it') ?>">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?= portal_e($pageTitle ?? 'SSW Portal') ?> · Avensys</title>
    <link rel="stylesheet" href="<?= portal_e(portal_asset('assets/app.css')) ?>">
</head>
<body>
<?php if (!empty($authenticated)): ?>
<header class="app-header">
    <a class="brand" href="<?= portal_e(portal_url()) ?>"><span class="brand-mark" aria-hidden="true"></span><span>SSW Portal</span></a>
    <nav class="main-nav"><a href="<?= portal_e(portal_url()) ?>">Selezioni</a><a href="<?= portal_e(portal_url(['action' => 'projects'])) ?>">Progetti</a><a href="<?= portal_e(portal_url(['action' => 'downloads'])) ?>">Download</a><?php if (!empty($licenseManagementAvailable)): ?><a class="license-nav-link" title="<?= portal_e($licenseNotificationPreview ?? '') ?>" href="<?= portal_e(portal_url(['action' => 'licenses'])) ?>">Licenze<?php if (!empty($licenseNotificationCount)): ?><span class="license-notification" title="<?= portal_e($licenseNotificationPreview ?? 'Nuove richieste o variazioni di licenza') ?>" aria-label="<?= (int) $licenseNotificationCount ?> notifiche. <?= portal_e($licenseNotificationPreview ?? '') ?>"><span aria-hidden="true"></span></span><?php endif; ?></a><?php endif; ?></nav>
    <?php if (is_array($latestRelease ?? null)): ?><a class="latest-release" href="<?= portal_e(portal_url(['action' => 'downloads'])) ?>" title="Pubblicata il <?= portal_e($latestRelease['published_at_utc'] ?: 'dato non disponibile') ?> UTC"><span>Ultima release disponibile</span><strong><?= portal_e($latestRelease['version']) ?></strong></a><?php endif; ?>
    <div class="header-actions"><button class="button button-small push-toggle" type="button" data-push-toggle data-csrf="<?= portal_e(portal_csrf()) ?>" hidden>Attiva notifiche</button><span><?= portal_e($user ?? '') ?></span><form method="post" action="<?= portal_e(portal_url(['action' => 'logout'])) ?>"><input type="hidden" name="csrf" value="<?= portal_e(portal_csrf()) ?>"><button class="button button-quiet" type="submit">Esci</button></form></div>
</header>
<?php endif; ?>
<main class="page-shell<?= empty($authenticated) ? ' login-shell' : '' ?><?= !empty($publicPage) ? ' public-registration-shell' : '' ?>">
