# New-UI-only local build

The local checkout uses `codex/ssw-next-ui-backend`, version 2.0.0.19.
The user requested only the new UI, with one recognizable x86 output directory.

`build-local.ps1` defaults to AV and writes the host and library into
`SSW/bin/x86/NewUI`. It restores both legacy packages.config projects, discovers
MSBuild with vswhere, and uses installed Node.js/npm for the frontend targets.
External commercial PDF embedding is disabled in this local build.
The authoritative calculation library remains a required dependency; the script
does not produce a separate old-edition application.

The four original private NuGet archives remain in the ignored local feed.
Node.js LTS was installed and the existing WebView2 Evergreen Runtime verified.
A copy of the installed AV schema-6 database was placed beside the new build.
Older local output folders were moved outside x86 into an ignored archive.
Private package archives, databases and build products are not published to Git.

Validation:

- AV x86 full rebuild, including npm ci and Vite assets: passed.
- `SSW.exe --next-ui-smoke`: exit code 0 with the schema-6 catalog.
- `SSW.exe --next-ui-screenshot <output.png>`: exit code 0.
- Screenshot inspected: guided project screen rendered with version 2.0.0.19.

The screenshot command validates the embedded WebView2 view without enrollment
or software-update actions. Normal startup retains the existing license and
update checks. This checkpoint is not a full report/API/installer release audit.
