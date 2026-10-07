# Unit classification - TODO-003

## Approved contract

DataCentral dbo.CLHeatRecoveryModels now has these additive fields:

| Field | Values / unit |
| --- | --- |
| RecoveryOperation | Plate, Rotary |
| ExchangerType | EN, LT, AL |
| UnitApplication | Centralized, Decentralized |
| InstallationEnvironment | Indoor, Outdoor, Both |
| OutdoorIncreaseA/B/C | Whole nonnegative millimeters, default 0; stored as decimal(10,2) for compatibility |

Customer labels must not contain "Only" or "Solo". Both means an indoor unit
convertible to outdoor, not an unchanged dual-use installation.

## Initial assignment, approved 2026-10-07

- Model Name contains EN: EN; otherwise contains LT: LT; otherwise AL.
- ECO-R (database series name ECOR) and ROOF-R: Rotary; all other units: Plate.
- SG (series 7): Decentralized; all other unit families: Centralized.
  SGD (series 1) is a distinct family and remains Centralized.
- ROOF-P / ROOF-R: Outdoor.
- HAKUNA / COCO / PRIME / MICRO: Indoor.
- Remaining units: Both.
- All three outdoor increases start at 0. Do not derive them from panel thickness.
- Exclude manual, service and accessory records from unit classification.

These are initial database assignments, not runtime name/family heuristics.
The application must subsequently consume explicit catalog values.

## Completed and verified

- [x] Read live CLDataCentral2 schema and inventory before editing.
- [x] Preserve all model rows in schema-bearing XML backups before migration.
- [x] Add constrained fields and seed 154 unit models; exclude 10 non-unit rows.
- [x] Verify every seed rule against live data with Test-UnitClassification.ps1.
- [x] Preserve manual corrections on migration reruns (seed only NULL values).

Verified totals: Plate 129 / Rotary 25; EN 9 / LT 8 / AL 137;
Centralized 142 / Decentralized 12; Indoor 20 / Outdoor 10 / Both 124.

Migration and verification sources:
T:/TECHNO_SOFT/mercurial/CLDCExplorer/CLDCExplorer/documents/sql/
add_unit_classification_20261007.sql,
Invoke-UnitClassificationMigration.ps1, Test-UnitClassification.ps1.

First pre-schema backup:
unit-classification-before-20261007-143146.xml in that directory's backups folder.
Second backup (before LT/AL seed): unit-classification-before-20261007-143219.xml.
Migration is transactional; existing dimension/performance values were not changed.
Rollback must preserve subsequent manual work: inspect backup and dependencies
before removing these additive columns; do not restore the entire live database.

## Remaining implementation gates

- [x] Explorer: added a dedicated tab with four selectors, three increments,
  explicit parameterized save and optimistic concurrency checks. Release/x86
  build and compiled-controls smoke passed; interactive save/reopen still needs
  operator acceptance. New persisted models must be classified before export.
- [x] CSV: seven invariant columns added; PHP lint, round-trip/invalid-value tests
  and all 154 live source rows passed. Full batch and InDesign acceptance not run.
- [x] SDF export: additive classification table and feature implemented in
  exporter 1.7.0, installed in Explorer and private server runtime. Real AV export
  passed and all 111 profiled model rows match all eight SQL Server fields.
- [x] SDF consumer: optional classification table read independently of the legacy
  EF model; fresh private AV export matches all 111 source models and loads in SSW.
- [x] SSW: four independent filters, AND across categories, all values initially
  permitted; Indoor matches Indoor/Both, Outdoor matches Outdoor/Both.
- [x] SSW: Both initially uses indoor dimensions. Choosing Outdoor automatically
  selects OKI and applies A+A_incr, B+B_incr, C+C_incr to the correct model-specific
  dimension axes. Outdoor-only units already have final outdoor base dimensions.
- [x] Returning to Indoor removes OKI, preserving other accessories and existing
  controller upgrade rules. Outdoor-required OKI is locked; missing/incompatible
  catalog dependencies block the configuration rather than creating an incomplete offer.
- [x] User confirmed A -> W, B -> L, C -> H on 2026-10-07.
- [x] Verify selection persistence/reopening, summary, generated drawing and PDF
  report generation. Outdoor reports identify Outdoor and include final W/L/H
  dimensions beneath the installation diagram. Existing static source PDFs are
  not automatically rewritten.
- [x] Build and test the development AV/x86 binary in the standard directory.
- [x] Package/publish installer and catalog; 2.0.0.26 update API and downloaded
  catalog hashes verified. See RELEASE_2_0_0_26_2026-10-07.md.

## Development verification 2026-10-07

- AV/x86 build and frontend typecheck passed.
- Added the localized clear-filters icon (15 languages). All seven criteria and
  legacy filter flags are cleared without changing the working point; numerical
  thresholds are preserved for reuse. Regression: tests/reset-preselection-filters-regression.mjs.
- The compiled preselection screenshot scenario selects AL and presses Clear,
  checking 0/7, unchanged working-point inputs and the still-open criteria panel.
- Existing Next UI application-service smoke and eight desktop screenshot checks
  passed. Expanded classification filters and Outdoor selection were additionally
  captured and visually checked in the compiled desktop; OKI and KTS EXTRA appear.
- tests/Invoke-UnitClassificationSmoke.ps1: 111 model classifications, AND and
  environment filters; required/locked OKI; selection serialization and reopening.
- Nonzero W/L/H arithmetic tested with 13/17/19 mm without changing database data.
  The integration test uses actual exported increments, currently zero by default.
- Outdoor PDF rendered successfully (273 KB); generated installation image checked
  visually, including the final W/L/H footer. Development executable version 2.0.0.26.
- Fresh isolated export: C:/Users/PC/Documents/SSW_develop/tmp/ssw-classification-20261007-162559.
- Previous AV/NewUI databases backed up under
  C:/Users/PC/Documents/SSW_develop/tmp/before-ssw-classification-20261007-162853.
- Static source PDF documents retain their original printed base dimensions;
  calculated dimensions and generated selection report use the environment choice.
- Public SDF/catalog and installer published with 2.0.0.26 on 2026-10-07.
  Live update API and catalog download integrity passed. Interactive installed
  upgrade acceptance remains available for the operator.

TODO-003 stays open for the remaining acceptance/publication gates, not for the
implemented four-category filtering or basic Outdoor/OKI functionality.
