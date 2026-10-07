# Commercial brochures in Next SSW - 2026-10-07

Status: implemented and tested; development version2.0.0.26. Public installer25
remains unchanged. No installer publication, server migration or Git push in
this task. WebSite source/catalog/database were read only.

## Shared contract

Authoritative WebSite documentation: A:\webavensys\docs\COMMERCIAL_BROCHURES.md.
Catalog URL:
https://www.avensys-srl.com/ftproot/DOCUMENTS/Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units/brochures.json

Bundled offline seed: SSW/Resources/brochures.json, copied byte-identically from
the WebSite catalog. SHA256:
99E50534F9258138EF9601681000B49D8F99D4C047847E404BCA4AB9E251E0E2.
Update this copy when the shared catalog changes; runtime refreshes the public
catalog once per application session and falls back to cached/bundled copies.
No brochure rules are inferred from translated/display model names.

- GRAND_TERTIAIRE: S3, S6, SA (SDF series32); S8 size89 and larger.
- PETIT_TERTIAIRE: S8 OSC/SSC, size88 and smaller.
- SCHOOL_HCI: S7 HCI only.
- Requested language first, then EN only. Current FR-only PDFs must not appear
  for IT or any other language without an appropriate English PDF.
- Multiple matching documents are supported as distinct cards with stable IDs.
- Catalog schema1 and PDF paths under COMMERCIAL are validated. Downloads are
  bounded (JSON1MiB, PDF25MiB), PDFs require their signature, cache writes use
  replacement files. No arbitrary path is accepted from the bridge/frontend.

## Runtime and UI

CLCommercialBrochureService reads structured series/configuration/size,
downloads matching PDFs and retains them for offline opening. Cache:
%LOCALAPPDATA%\Avensys\SSW\Brochures. Downloads are triggered by the Documents
page or internal audit, not calculations or normal startup. PDF checks are
memoized for5 minutes per session; catalog for the session. On an unavailable
network, existing PDF copies remain usable. First access can incur bounded
network waits; warm document views also use the existing host model/language cache.

The original commercial-sheet action, paths, filenames and fallback resolver
are unchanged. Its visible label is now Technical-commercial data sheet in all
15 UI languages. Commercial brochure labels also cover all15 languages.
An unavailable brochure has a visible disabled placeholder, not a false link.
Available cards include the actual PDF language. Benchmark remains a separate
placeholder immediately after the existing technical-commercial category.

Internal document audit includes a separate brochure column with online/offline
badges, EN fallback, confirmed-missing filtering and printable HTML/PDF output.
Network probes for shared brochure URLs are deduplicated within each audit.
Unknown responses remain unverified, not confirmed missing. The existing
internal audit remains predominantly Italian; its printable headings are IT/EN.

## Validation

- Full AV/x86 and frontend/TypeScript build passed in isolated output.
- Test-CommercialBrochures.ps1: series/rules,88/89 boundaries, HCI versus FS,
  missing sizes, regional languages,15-language policy, traversal and schema
  rejection; online FR retrieval of all3 PDFs, offline reuse, no FR fallback
  into IT, and a fixture-only future English offline fallback passed.
- commercial-brochures-regression.mjs:45 localized UI cases passed (empty,
  one and multiple documents across15 languages).
- Existing application document renderer8 cases and resolver25 cases passed;
  packaged ST application PDF hash unchanged.
- Document dashboard badges/filter regressions passed, including brochures.
- Next selection and product-boundary regressions passed.
- Standard AV and NewUI build outputs now report2.0.0.26; service/report smoke,
  offer archive regression and Next-only assembly checks passed.
- Native CLRC023OSC French and Italian screenshots reviewed: FR brochure
  enabled; IT brochure disabled with the same cached French PDF present.

Evidence: C:\Users\PC\Documents\SSW_develop\tmp\brochure-fr.png and
brochure-it.png. Tests retain their isolated temporary cache directories.
No real offers/reminders were created. Normal external PDF-reader launch and
full live audit UI were not exercised; resolver paths, HTML badges and native
Documents UI were verified. No complete numerical-baseline rerun was required
for this document-only change; calculations were not edited.

Development executables:
D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe and
D:\mdev\SSW\SSW\bin\x86\NewUI\SSW.exe (start-new-ui.ps1).
The new NewUI output received a copy of the existing AV SDF because it had no
database; existing AV data and user selections were not replaced.

## Remaining content

Request TODO-002 is recorded as DONE-002 for category creation and
technical-commercial renaming. Confidential benchmark documents and additional
brochure languages still require actual content. DONE-001 records fan consumption
verification confirmed by the project owner; DONE-004 records the existing
offline SG 127 ST application document. TODO-003 remains open for explicit
recovery-type classification in DataCentral. There is no FR-to-other-language
fallback.
