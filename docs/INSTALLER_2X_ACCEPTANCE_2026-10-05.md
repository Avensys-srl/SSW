# Installer 2.x acceptance - 2026-10-05

User approved deferring the unavailable actual 1.x upgrade test and preparing
2.0.0.24. This checkpoint tests local packaging; it does not publish a release.

## Results

| Check | Result | Evidence |
| --- | --- | --- |
| AV/x86 candidate build | Passed | SSW.exe and SSWLib.dll 2.0.0.24 |
| Next-only assembly boundary | Passed | No CLMainForm; shared runtime retained |
| Fresh 2.0.0.24 installation | Passed | Isolated Inno installer, installed service smoke |
| Upgrade 2.0.0.23 to 2.0.0.24 | Passed | Same isolated directory; installed version checked |
| Existing selection preservation | Passed | SHA256 0AC7144C3BA4AF66FF8C5E53723A0CCC67AD2068FDA02A3091C779DD1BC00662 unchanged |
| Installed Next startup | Passed | Captured installed-startup.png, version 2.0.0.24 |
| Installed service and report smoke | Passed | Both fresh and upgraded installations |
| PDF format and sample visual review | Passed | Ten generated PDFs, A4; representative 2-page and 4-page report inspected |
| Frontend boundary and normalization regression | Passed | Existing Node tests |
| Actual 1.x upgrade | Deferred by user | No 1.x installation available |

Reproducible runner: tests/Test-Installer2x.ps1. It expects test-only packages
compiled from installer/SSW.iss with /DIntegrationTest. That switch uses a
separate AppId and creates no shortcuts. Never publish these unsigned packages.
The source binaries for 23 were the previous local compiled candidate, not a
claim of testing every historical public 2.x installer.

Final evidence directory:
C:\Users\PC\Documents\SSW_develop\tmp\installer-2x-run-20261005c
contains setup logs, result.json, screenshots, PDFs and rendered review pages.
Package directory: tmp\installer-2x-test under the same workspace.

## Problems found and corrected

- The service smoke incorrectly dereferenced winter calculations for CFI/CDR/CFD.
  These deliberately require Avensys selection. The test now verifies absence of
  calculations instead; production formulas and availability policy unchanged.
- Installer recursion included diagnostic SDF copies. Added data\*.sdf.* to
  packaging exclusions. Old copies already installed by an older package are not
  deleted by an upgrade; user data is not removed.
- Smoke PDF retention is optional through SSW_NEXT_SMOKE_REPORT_DIRECTORY and
  only used by the CLI smoke command; normal report handling is unchanged.

## Limits and next gate

Follow-up correction: the duplicated regulation percent suffix was fixed in
CLNextUiReportService; RDLC supplies the suffix and the data now contains only
the formatted number. AV/x86 build and report smoke passed. Five fresh PDFs
were checked for duplicate percent signs, and the Italian winter/summer page
visually reviewed. Evidence: workspace tmp/regulation-percent-fix. The duplicate
percent observation below describes the earlier test, not the corrected build.

This is a same-user, isolated-AppId install/upgrade on the current Windows host,
not a clean VM, actual customer's production registration, new license activation,
or a server-side offer/reminder test. Those online flows were not repeated here.
The main production AppId remains unchanged. The 1.x test is deferred, not passed.
Poppler warned about its local Symbol display-font substitution; the reviewed
pages rendered. Existing sample formatting includes duplicate percent signs and
English accessory descriptions in synthetic Italian report inputs; those are not
installer regressions and are not claimed as localization acceptance here.

Before publication: complete the production signed-package/manifest readiness
checks and explicit release authorization. No installer publication, commit or
push performed in this checkpoint.
