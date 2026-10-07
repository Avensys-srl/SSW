# Application document catalog

Implemented in development 2.0.0.26; public installer unchanged.

Shared contract and maintenance: A:/webavensys/docs/APPLICATION_DOCUMENTS.md.
Public catalog: application-documents.json alongside brochures.json, under
Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units.
Seed: SSW/Resources/application-documents.json, bundled as
css/ApplicationDocuments/application-documents.json.

SG_127_ST_APPLICATION applies only to S7/ST/127 and resolves
ApplicationDocuments/S7/{lang}/ST_127_{lang}_AV.pdf. Current EN PDF revision 07
is byte-identical to the original F: asset and the previously bundled SSW PDF.
Requested language -> EN fallback; no other language substitution.

CLApplicationDocumentService shares rule matching, bounded download and atomic
cache helpers with brochures, but keeps a separate catalog and PDF-path whitelist.
Cache: %LOCALAPPDATA%/Avensys/SSW/ApplicationDocuments. Test override:
SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY. Catalog cached per online session;
PDF probes deduplicated for five minutes. Valid cached PDFs or the bundled EN
PDF are usable offline. Document lookup does not alter calculations or the SDF.

AV application documents now use explicit structured series/configuration/size
rules rather than display-name inference. Other customer profiles retain their
existing local lookup. The existing application card/action and localized labels
are retained; internal offline coverage also checks the catalog/cache.
The current desktop contract opens one application document (first matching id
by order); web supports multiple links. Extending the desktop to multiple cards
will be necessary if several distinct application documents apply to one model.

Tests:
- tests/Test-ApplicationDocumentCatalog.ps1 -Online: rules, sizes, 15 languages,
  traversal rejection, source PDF hash, online -> offline cache, strict lookup.
- tests/Invoke-ApplicationDocumentSmoke.ps1: packaged asset hash and 25 model,
  brand, series and language cases with no downloads.
- Existing brochure catalog and application-card regressions passed.
- Public catalog and PDF HTTP 200. No installer publication or Git push.
- Native Documents screenshot for SG 127 ST did not complete: the configured
  workflow timed out waiting for the Layout view, before document rendering.
  Desktop visual confirmation remains pending; compiled resolver tests passed.
