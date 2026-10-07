# License notification preview - 2026-10-06

Deployed overlay for A:\webavensys\ssw-portal. Hovering Licenze or its red dot
now displays name/email and reason for open actions and unread user changes.
Up to8 distinct entries are shown, with an additional-entry count. No PIN or
activation secret is included. Existing count/read semantics remain unchanged.
The authenticated polling endpoint adds preview, keeping its count contract.
Template and JavaScript use escaped attributes / DOM title, not injected HTML.

Validation: staged102 tests, PHP lint and JavaScript syntax checks passed.
Existing production98 tests passed after deployment. No database writes or
migrations were performed; tests use fixtures. Authenticated browser hover was
not inspected in the current session. Refresh the portal to load new assets.

Rollback: restore the five files from
C:\Users\PC\Documents\SSW_develop\tmp\portal-license-tooltip-20261006\backup-live.
Backup and deployed file hashes verified. No Git commit/push performed.
The test runner here must be overlaid onto a full portal checkout before running.
