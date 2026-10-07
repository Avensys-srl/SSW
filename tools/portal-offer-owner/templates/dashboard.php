<div class="page-heading"><div><h1>Selezioni tecniche</h1><p>Archivio centralizzato delle configurazioni generate da SSW.</p></div></div>
<section class="stats" aria-label="Riepilogo">
    <div><strong><?= (int) $stats['selections'] ?></strong><span>Selezioni</span></div>
    <div><strong><?= (int) $stats['revisions'] ?></strong><span>Revisioni</span></div>
    <div><strong><?= (int) $stats['recent'] ?></strong><span>Ultimi 30 giorni</span></div>
    <div><strong><?= (int) $stats['customers'] ?></strong><span>Clienti attivi</span></div>
    <div><strong><?= (int) $stats['installations'] ?></strong><span>Installazioni attive</span></div>
    <div><strong><?= (int) $stats['projects'] ?></strong><span>Progetti</span></div>
    <div><strong><?= (int) $stats['downloads'] ?></strong><span>Download SSW</span></div>
</section>
<section class="filter-band">
    <form method="get" class="filter-grid">
        <label class="search-field">Ricerca<input name="q" value="<?= portal_e($filters['q']) ?>" placeholder="Riferimento, cliente, unità o progetto"></label>
        <label>Cliente<select name="customer"><option value="">Tutti</option><?php foreach ($availableFilters['customers'] as $item): ?><option value="<?= portal_e($item['customer_code']) ?>"<?= $filters['customer'] === $item['customer_code'] ? ' selected' : '' ?>><?= portal_e($item['display_name']) ?></option><?php endforeach; ?></select></label>
        <label>Versione<select name="version"><option value="">Tutte</option><?php foreach ($availableFilters['versions'] as $item): ?><option<?= $filters['version'] === $item ? ' selected' : '' ?>><?= portal_e($item) ?></option><?php endforeach; ?></select></label>
        <label>Nazione<select name="country"><option value="">Tutte</option><?php foreach ($availableFilters['countries'] as $item): ?><option<?= $filters['country'] === $item ? ' selected' : '' ?>><?= portal_e($item) ?></option><?php endforeach; ?></select></label>
        <label>Dal<input type="date" name="from" value="<?= portal_e($filters['from']) ?>"></label>
        <label>Al<input type="date" name="to" value="<?= portal_e($filters['to']) ?>"></label>
        <label class="filter-checkbox"><input type="checkbox" name="include_unlocated" value="1"<?= !empty($filters['include_unlocated']) ? ' checked' : '' ?>>Mostra località non disponibile</label>
        <div class="filter-actions"><button class="button button-primary" type="submit">Applica</button><a class="button" href="<?= portal_e(portal_url()) ?>">Azzera</a></div>
    </form>
</section>
<div class="table-heading"><strong><?= (int) $results['total'] ?> risultati</strong><span>Ordine: aggiornamento più recente</span></div>
<div class="table-scroll">
<table class="data-table">
    <thead><tr><th>Riferimento</th><th>Cliente</th><th>Rif. cliente</th><th>Unità</th><th>Rev.</th><th>Versione</th><th>Località</th><th>Aggiornata</th><th></th></tr></thead>
    <tbody>
    <?php foreach ($results['items'] as $item): $payload = json_decode((string) $item['selection_payload'], true); $payload = is_array($payload) ? $payload : []; ?>
    <tr>
        <td><a class="reference" href="<?= portal_e(portal_url(['action' => 'detail', 'reference' => $item['public_reference']])) ?>"><?= portal_e(SelectionPresenter::reference($item['public_reference'], (int) $item['latest_revision'])) ?></a></td>
        <td><strong><?= portal_e(SelectionPresenter::offerOwner($item)) ?></strong><small><?= portal_e($item['display_name']) ?> · <?= portal_e($item['customer_code']) ?></small></td>
        <td><?= portal_e(SelectionPresenter::customerReference($payload) ?: '-') ?></td>
        <td><?= portal_e(SelectionPresenter::unit($payload)) ?></td>
        <td><?= (int) $item['latest_revision'] ?></td><td><?= portal_e($item['software_version']) ?></td>
        <td><?= portal_e(SelectionPresenter::location($item)) ?></td><td><?= portal_e($item['revision_created_at']) ?></td>
        <td><a class="button button-small" href="<?= portal_e(portal_url(['action' => 'detail', 'reference' => $item['public_reference']])) ?>">Apri</a></td>
    </tr>
    <?php endforeach; ?>
    <?php if (!$results['items']): ?><tr><td colspan="9" class="empty-state">Nessuna selezione corrisponde ai filtri.</td></tr><?php endif; ?>
    </tbody>
</table>
</div>
<?php if ($results['pages'] > 1): ?><nav class="pagination" aria-label="Pagine"><?php for ($pageNumber = 1; $pageNumber <= $results['pages']; $pageNumber++): $params = array_merge($filters, ['page' => $pageNumber]); ?><a class="<?= $pageNumber === $results['page'] ? 'current' : '' ?>" href="<?= portal_e(portal_url($params)) ?>"><?= $pageNumber ?></a><?php endfor; ?></nav><?php endif; ?>
