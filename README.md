# SSW Next - New UI

Use the new guided WebView2 UI for local compilation and testing.
The active development branch is
[`codex/ssw-next-ui-backend`](https://github.com/Avensys-srl/SSW/tree/codex/ssw-next-ui-backend).
This `master` checkout contains the older source line; switch branches before building.

## Start From Zero

Follow the complete [new-PC setup and test guide](https://github.com/Avensys-srl/SSW/blob/codex/ssw-next-ui-backend/README.md#new-pc-setup).
It includes software download links, installation commands, the four private
NuGet packages, runtime database preparation, compilation and smoke testing.

Required tools: Git for Windows, Visual Studio .NET desktop Build Tools,
.NET Framework 4.8 Developer Pack, Node.js LTS/npm, and WebView2 Evergreen Runtime.
Private package archives and an AV product database must be obtained through
Avensys access; they are not included in this public repository.

Clone the new-UI branch into an empty folder:

```powershell
git clone --branch codex/ssw-next-ui-backend https://github.com/Avensys-srl/SSW.git D:\SSW
Set-Location D:\SSW
```

For an existing clean checkout:

```powershell
git fetch origin
git switch codex/ssw-next-ui-backend
git pull --ff-only
```

After preparing the dependencies described in the guide:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-local.ps1
```

The script builds only the AV new-UI host into `SSW\bin\x86\NewUI`.
Place a compatible AV `DataCentral.sdf` in `SSW\bin\x86\NewUI\data`, then launch:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\start-new-ui.ps1
```

The executable is `SSW\bin\x86\NewUI\SSW.exe`. Normal startup retains the
application's license and update checks. The calculation library is still used
by the new UI; no separate old-edition executable is built by this script.

## Verification

The local AV x86 build, new-UI smoke command and embedded WebView2 screenshot
check passed on 7 October 2026 using a schema-6 AV catalog. See the
[verification checkpoint](https://github.com/Avensys-srl/SSW/blob/codex/ssw-next-ui-backend/docs/LOCAL_NEW_UI_BUILD_2026-10-07.md)
for evidence and remaining test scope.
