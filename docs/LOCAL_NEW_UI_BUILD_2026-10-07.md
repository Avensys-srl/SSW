# New-UI-only local build

The new UI was built from `codex/ssw-next-ui-backend`, version 2.0.0.19.
The user subsequently requested promotion of this source to `master`.
`master` is now the default new-UI source and local working branch.
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

## Development Workstation Follow-Up

The master build was rebuilt and the workstation dependencies rechecked.
`start-frontend.ps1` starts Vite with a fixed localhost port for hot reload.
`start-new-ui.ps1 -DevUrl <url>` launches the real desktop backend against that
server; without the option it explicitly uses bundled assets. The launcher
restores the caller's environment after spawning the host.
Browser previews use mock data; desktop validation uses the SQL CE catalog.

Follow-up verification: full master rebuild, new-UI smoke and screenshot command
passed; the captured guided project screen was inspected. The frontend development
server returned HTTP 200 at localhost:5173. All three PowerShell entry scripts
passed syntax parsing. Existing dependency/obsolete-API warnings remain; this is
development readiness, not a signed production installer release.
