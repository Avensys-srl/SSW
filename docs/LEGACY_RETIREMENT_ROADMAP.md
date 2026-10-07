# Gradual retirement of SSW 1.x

Approved 2026-10-05. This decision supersedes earlier requirements to maintain
an active 1.3.x product line beside Next. Only SSW 2.0.0.0 and later receive new
development and release acceptance. Historical releases remain in Git; no
commit, tag, installer, user file or database is deleted by this decision.

## Acceptance boundaries

- Next is the only supported user interface. Runtime legacy fallback was already
  removed before this decision; the new boundary test prevents its reintroduction.
- Historical supported selection formats remain importable, with pre-migration
  backup. Do not confuse file schema version 1 with software major version 1.
- New saves use the current format. There is no obligation to export files that
  old executables can read. Native historical formats without an existing importer
  are not newly claimed as supported by this policy.
- Verified calculation engines, SDF, report templates, model mappings and device
  license migration remain shared production components, not removal candidates
  merely because they originated in 1.x.
- Existing server clients and stored records remain compatible for now. Retiring
  old API clients requires installation inventory and a separately tested rollout;
  no server access or old-client rejection is introduced in phase 1.

## Roadmap

| Phase | Status | Completion criterion |
| --- | --- | --- |
| 1. Support boundary | Implemented | Next-only source test, build refuses software major below 2, historical import smoke passes |
| 2. Dependency inventory | Completed | See LEGACY_DEPENDENCY_INVENTORY_2026-10-05.md |
| 3. Remove UI-only code | Build exclusion implemented; final report acceptance pending | Customer assembly excludes legacy main form; diagnostic opt-in retained; project dialog cleanup deferred |
| 4. Installer migration | Fresh install and 2.x upgrade passed; 1.x deferred by user | See INSTALLER_2X_ACCEPTANCE_2026-10-05.md; actual 1.x installation unavailable, test not claimed as passed |
| 5. Old-client API retirement | Deferred | Inventory proves transition feasible; staged server contract and offline/licensing effects verified |

Phase 1 avoids maintaining a parallel legacy user-interface acceptance matrix
for every new feature. Numerical baselines and historical import tests remain
mandatory. Time savings are not yet quantified; removal of shared code cannot
be justified as a saving before phase 2.

## Verification

User-approved release scope: 2.0.0.24; unavailable 1.x upgrade test deferred.
Fresh installation and upgrade from the previous local 2.0.0.23 binary passed
using an isolated installer AppId. Selection preserved unchanged, installed
services/report smoke passed, installed UI and representative PDFs inspected.
This does not claim production registration or license activation acceptance.

Continuation: customer and diagnostic assembly boundary checks passed, four
numeric baseline fixtures passed, frontend regression and Next-only identity
smoke passed. Standard AV/x86 rebuilt. Sources remain recoverable; customer
build excludes legacy UI by default. Diagnostic build opt-in is
`SSWIncludeLegacyReferenceUi=true`; installer packaging rejects that mode.
See the dated inventory for retained dependencies and outstanding release gates.

Executed 2026-10-05: Next boundary test passed; the intentional 1.3.0.56
target failed with the expected retirement message; full AV/x86 build passed
in the separate next-only-build directory; SelectionIdentitySmoke passed,
including historical v1 fixture migration, backup and reload. This is phase 1
acceptance, not a claim that physical legacy-code removal is complete.

Run `node tests/next-product-boundary.mjs`, the existing SelectionIdentitySmoke
(including v1 fixture migration and backup), and the AV/x86 solution build.
An intentional `SSWVersion=1.3.0.56` build must fail at
EnforceSupportedSSWProductLine before compilation. Do not override this guard
to produce an unsupported installer; use an archived checkout for diagnosis.
