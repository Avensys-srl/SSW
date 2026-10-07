# Offer lifecycle acceptance - 2026-10-05

## UI visibility update - 2026-10-05

At user request, Save as (selection and multi-project) and Generate provisional
offer controls are hidden. Their handlers, translations and backend workflows
are retained. To reactivate them, change optionalWorkflowControls in main.ts.
Generate definitive offer, regular Save, Open and reminders remain available.
Frontend build passed; browser checks confirmed all three controls hidden and
the definitive button visible. Standard desktop rebuild initially blocked by
the running user instance; close SSW before rebuilding or publishing an installer.

## Scope

Central per-revision Provisional/Definitive state, compiled desktop reminder
verification, offline persistence and offer-dialog localization. Numerical
selection rules and SDF data were not changed by this work.

| Gate | Result | Evidence |
| --- | --- | --- |
| Standard AV/x86 build | Passed | Exact bin/x86/AV/SSW.exe exercised |
| Central provisional -> definitive | Passed | Reference 4974-8419-1569-8345, revision 1 |
| Reprint | Passed | Same reference, revision and single reminder |
| Desktop restart and reopen | Passed | Saved selection reopened from reminder |
| Test reminder cancellation | Passed | API synchronization confirmed, zero pending mutations |
| Offline provisional PDF/save/reopen | Passed | No central registration and no reminder |
| Offline definitive | Correctly blocked | Screenshot scenario exit 3 on unavailable loopback API |
| Offline queue/retry | Passed | SelectionIdentitySmoke persistence and ordered retry tests |
| Languages | Passed for offer workflow | 15 dialogs captured; no dialog overflow |
| Frontend regression | Passed | Normalization, failures and stale responses |
| Backend contract | Passed | PHP service and offer status tests, syntax checks |

## Contract

POST selections/{reference}/offer requires authentication, resume token,
current revision and snapshot hash. Definitive state cannot be downgraded by
a provisional reprint. Historical revisions and old clients remain compatible:
their offer status is null, not retrospectively inferred.

PDF preparation precedes central status recording. Definitive generation
requires online technical registration; offline provisional documents remain
local drafts. The reminder interval starts at the first definitive timestamp.
Reprinting the same revision does not create another reminder or restart time.
Months remain fixed 30-day periods, with the existing 90-day API limit.

Admin selection details and export include offer state and timestamps.
No commercial prices or customer contact payload was added to this endpoint.

## Deployment and recovery

Migration 010 adds three nullable columns to ssw_selection_revisions and one
migration-history row. A complete pre-migration dump is stored outside Git in
the development workspace tmp/ssw_selections-before-offer-status-20261005.sql.
Original server source files are in api/tools/backups/offer-status-20261005,
protected by a deny-all .htaccess. Restore source files first for rollback;
leave additive columns intact to preserve the new audit information. Do not
restore the full database over subsequent production writes.

The reproducible source overlay is tools/offer-status-api/overlay. It contains
no runtime configuration, credentials or database dump. Apply it only to the
matching API baseline after backup; run migration 010 once, not on every build.
The overlay tests run in the target API checkout with its existing bootstrap.

## Evidence and residual scope

Workspace tmp contains offer-lifecycle-create.png.result.json,
offer-lifecycle-create.png.provisional.json, offer-lifecycle-reprint.png.result.json,
offer-reminder-reopen-final.png.result.json,
offer-reminder-cleanup-final.png.result.json, offer-offline.png.result.json,
offer-language-checks.json and offer-dialog-{language}.png.

Direct read-only SQL verification confirmed revision 1 as Definitive and the
test reminder as Cancelled. Screenshot: tmp/offer-central-verification.png.

The disposable lifecycle reminder a82d235b-6bbf-4436-b458-61426809dc8f was
cancelled through the official API, not deleted. Earlier isolated diagnostic
queues were not replayed. Their reconciliation is separate from this acceptance.
Localization review covers the offer/save/reminder workflow, not a linguistic
certification of every legacy message. No public installer release or Git push
was performed.
