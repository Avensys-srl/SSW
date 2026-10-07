# SSW - Selection Software Workbench

SSW is a Windows desktop selection tool (WinForms) built for multiple HVAC/ventilation manufacturers. The same codebase is compiled into different editions (profiles) that customize product data, branding, and customer information. The solution contains a C# WinForms application and a VB.NET class library that holds most UI and domain logic.

This repository targets the .NET Framework and uses SQL Server Compact for the local data store, Entity Framework for data access, and ReportViewer/iTextSharp for report generation.

## Key Capabilities

- Multiple OEM builds via compile-time profiles.
- Unit selection and performance calculations for heat recovery and related components.
- Local product data storage in SQL Server Compact (`.sdf`) files.
- Multi-language UI resources.
- Report generation using Microsoft ReportViewer and PDF export (iTextSharp).

## Solution Structure

- `SSW.sln`: Visual Studio solution.
- `SSW/`: C# WinForms executable (entry point, profile selection, build configurations).
- `SSWLib/`: VB.NET class library with UI forms and domain logic.
- `3rd/`: HEDes local water-coil calculation runtime (`COILcalc.dll`) and its dependencies.
- `packages/`: NuGet packages (legacy `packages.config` restore).

## Profiles

Profiles are controlled by conditional compilation symbols (`_PROFILE_*`) defined per solution configuration. Choose the configuration that matches the target customer.

Profiles defined in `SSW/CLProgram.cs`:

- `AC`
- `AL`
- `AV`
- `CL`
- `CV`
- `DAN`
- `FA`
- `FAI`
- `FS`
- `FT`
- `IN`
- `NL`
- `SIG`
- `SKL`
- `SU`
- `WE`

Each profile maps to an `SSWInfo` class (`SSW/CLSSWInfo_*.cs`) that provides customer data, branding, and default language.

## Tech Stack

- .NET Framework 4.8
- C# (WinForms) + VB.NET class library
- Entity Framework 6 (SQL Server Compact provider)
- Microsoft SQL Server Compact 4.0
- Microsoft ReportViewer (WinForms)
- iTextSharp (PDF output)
- alglib.net (math)
- BouncyCastle (crypto)

## Current Version

- Application version: `1.3.0.55` (single source: `SSWVersion.props`)

## Prerequisites

- Windows with PowerShell and access to Avensys private dependencies.
- [Git for Windows](https://git-scm.com/download/win).
- [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/) with
  the .NET desktop build tools workload. Visual Studio 2022 Build Tools was used
  to verify the commands below; the full Visual Studio IDE is optional.
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48),
  including its targeting pack and SDK. A modern `dotnet` SDK alone is insufficient.

## New PC Setup

The steps below prepare a fresh checkout of `master`, build the Avensys profile,
and launch it with a compatible product database. Private package archives and
product databases are not distributed by this public repository.

### 1. Install the build tools

Open PowerShell as Administrator. If `winget` is unavailable, use the download
links above and select the same workload/components in Visual Studio Installer.

```powershell
winget install --id Git.Git -e --source winget --accept-package-agreements --accept-source-agreements
winget install --id Microsoft.VisualStudio.2022.BuildTools -e --source winget --accept-package-agreements --accept-source-agreements --silent --override "--wait --quiet --norestart --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.Net.Component.4.8.TargetingPack --add Microsoft.Net.Component.4.8.SDK --includeRecommended"
```

Complete any requested restart, then open a new PowerShell window. Verify Git:

```powershell
git --version
```

### 2. Create the local repository

Choose an empty folder. This example uses `D:\SSW`; use another writable location
if the PC has no `D:` drive.

```powershell
git clone --branch master https://github.com/Avensys-srl/SSW.git D:\SSW
Set-Location D:\SSW
git remote -v
git status
```

`origin` must point to `https://github.com/Avensys-srl/SSW.git`.
Read `SSWVersion.props` for the version of the selected checkout. `master` can
be older than the versions listed under [release tags](https://github.com/Avensys-srl/SSW/tags).
Use a database compatible with the checked-out source, not simply the newest SDF.

### 3. Obtain the four private packages

These exact archives are required:

| Archive | Source repository (requires Avensys access) |
| --- | --- |
| `CLCommonLib.2018.1.24.16190.nupkg` | [CLCommonLib](https://github.com/Avensys-srl/CLCommonLib) |
| `CLDataCentralCommonLib.2018.1.24.16193.nupkg` | [CLDataCentralCommonLib](https://github.com/Avensys-srl/CLDataCentralCommonLib) |
| `CLDataCentralLTModel.2018.1.24.16201.nupkg` | [CLDataCentralLTModel](https://github.com/Avensys-srl/CLDataCentralLTModel) |
| `CLEFCommonLib.2017.12.4.11112.nupkg` | [CLEFCommonLib](https://github.com/Avensys-srl/CLEFCommonLib) |

The linked repositories contain library source, not the original package archives.
Their READMEs identify the internal package feed. On the verified workstation,
the archives were available at `T:\TECHNO_SOFT\nuget\repository`.
Connect to the Avensys share, or obtain these archives from the package owner.
They cannot be restored from NuGet.org.

From the SSW repository root, copy the archives to the local feed:

```powershell
$packageSource = 'T:\TECHNO_SOFT\nuget\repository'
New-Item -ItemType Directory -Path .\packages\local-feed -Force | Out-Null
$packages = @(
    'CLCommonLib.2018.1.24.16190',
    'CLDataCentralCommonLib.2018.1.24.16193',
    'CLDataCentralLTModel.2018.1.24.16201',
    'CLEFCommonLib.2017.12.4.11112'
)
foreach ($package in $packages) {
    Copy-Item -LiteralPath (Join-Path $packageSource ($package + '.nupkg')) -Destination .\packages\local-feed -ErrorAction Stop
}
```

Set `$packageSource` to your actual archive directory if the share differs.
[NuGet.Config](NuGet.Config) uses `packages\local-feed` and NuGet.org. Public
dependencies, including `System.Text.Json`, restore automatically during the build.
The `packages` directory is ignored by Git; keep private archives out of commits.
Cloning the four library repositories is optional for inspecting their source.

### 4. Compile

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-local.ps1 -Configuration AV
```

[build-local.ps1](build-local.ps1) finds MSBuild through `vswhere`, restores NuGet
packages, and performs a full `AV|x86` rebuild. It stops on restore/build failure.
The resulting executable is `SSW\bin\x86\AV\SSW.exe`. Without `-Configuration`,
the script builds `Debug|x86` and writes `SSW\bin\Debug\SSW.exe`.

Assembly and ClickOnce manifest signing are enabled when their certificate files
are present. An unsigned development build does not require the private PFX files.
Signed installer publication has separate requirements described below.

For profiles that embed commercial PDF sheets, the build uses the mapped `M:`
marketing share when available, with the legacy share as a fallback. To build
without these external PDF sheets:

```powershell
$env:IncludeCSS = 'false'
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-local.ps1 -Configuration AV
Remove-Item Env:\IncludeCSS
```

Alternatively, set `$env:SSWMarketingRoot` to the share root containing
`MKTG_PRODOTTO\MKTG_PR_CLRC\SORGENTE_SSW`, ending the root path with `\`.
SQL Server Compact native DLLs are copied from restored packages into the output;
a separate SQL Server installation is not required for the local product database.

### 5. Prepare the runtime database

Compilation does not create a product database. Obtain a profile-compatible
`DataCentral.sdf` from an approved Avensys export or a matching installation.
Copy it into `data` beside the executable. Work on a copy of the database.

For the `master` version `1.3.0.55`, the compatibility reader supports legacy
databases and managed schemas 1 through 3. A schema-6 database from a newer
installation is rejected. Do not modify its schema metadata to bypass that check.
The database must also match the customer code and minimum SSW version.

The following legacy AV database was successfully used for startup on the verified
workstation. Availability of this internal archive depends on your share access:

```powershell
$databaseSource = 'T:\TECHNO_SOFT\mercurial\SSW\SSW_1305\SSW\SSW\bin\x86\AV\data\DataCentral.sdf'
$dataDirectory = '.\SSW\bin\x86\AV\data'
New-Item -ItemType Directory -Path $dataDirectory -Force | Out-Null
Copy-Item -LiteralPath $databaseSource -Destination (Join-Path $dataDirectory 'DataCentral.sdf') -ErrorAction Stop
```

This older database is suitable for legacy startup/performance checks. Features
requiring a newer managed catalog need a compatible export with those features.
The SDF files under `tests\fixtures` are test fixtures, not production catalogs.

### 6. Launch and test

```powershell
$exe = (Resolve-Path .\SSW\bin\x86\AV\SSW.exe).Path
Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe)
```

Verify that the performance screen opens, a product is selected, and results and
charts are populated. Change airflow within the product's range and confirm that
the results recalculate. Check project save/reopen and report preview separately.
Decline software-update installation prompts when testing the local executable;
installing a published update does not test the locally compiled build.

Run the lightweight alternative-reference smoke test with 32-bit PowerShell:

```powershell
& "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File .\tests\Invoke-AlternativeReferenceSmoke.ps1
```

Expected output: `Alternative reference progression passed.`
This test and the `Debug|x86` / `AV|x86` builds were verified during local setup.
The AV performance screen opened with calculated values using the legacy database.
This does not establish that every workflow or API integration passes.

[tests/Invoke-TechnicalSelectionReleaseTests.ps1](tests/Invoke-TechnicalSelectionReleaseTests.ps1)
is the broader release matrix. It currently assumes a Visual Studio 2019 Enterprise
MSBuild path, a feature-complete AV catalog, and a separate PHP/API environment.
It needs environment-specific setup before use on a fresh PC; it is not the basic
desktop startup test. API-connected registration and synchronization additionally
need the appropriate service access/enrollment configuration.

Optional for runtime distribution:

- Microsoft SQL Server Compact 4.0 runtime (native binaries are copied post-build)

Required for installer builds:

- Inno Setup 6
- Windows SDK SignTool, available from the Windows SDK
- A valid code-signing certificate installed in the Windows certificate store. The installer build script reads the signing certificate thumbprint from `SSW/SSW.csproj`.

## Build

For a local command-line build, run `powershell -ExecutionPolicy Bypass -File .\build-local.ps1`.
The script restores packages and rebuilds `Debug|x86`; use `-Configuration AV` for the AV profile.
The Debug executable is written to `SSW\bin\Debug\SSW.exe`.

Private package archives on this PC are stored in `packages\local-feed`, configured by `NuGet.Config`.
Assembly and manifest signing are enabled when their certificate files are present.
Datasheets use the mapped `M:` marketing share when available, with the legacy share as a fallback.
Override the share root with the MSBuild property `SSWMarketingRoot`, or use `/p:IncludeCSS=false`
to build without the external datasheets.

1. Open `SSW.sln` in Visual Studio.
2. Restore NuGet packages (solution uses `packages.config`).
3. Select the desired configuration (e.g., `CL|x86`, `AC|x86`, etc.).
4. Build the solution.

The `SSW` project includes a post-build step that copies SQL Server Compact native binaries into `x86` and `amd64` folders in the output directory.

## Installer Build

The installer is generated with Inno Setup from `installer/SSW.iss`.

Use the helper script from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

The script:

- builds `SSW.sln` in `AV|x86`
- reads the application version from `SSWVersion.props`
- generates matching assembly versions for `SSW.exe` and `SSWLib.dll`
- stops the release if either binary does not match the declared version
- signs `SSW.exe`, `SSWLib.dll`, and the final installer with SignTool
- reads the AV enrollment key from `SSW_SELECTION_BOOTSTRAP_KEY_AV` without
  storing it in the repository and provisions it for the current Windows user
- creates `installer/output/SSW_Setup_<version>.exe`
- atomically publishes the installer and manifest to
  `F:\DOCUMENTS\tools\Selection Software`, verifies their integrity, and
  removes older `SSW_Setup_*` public artifacts only after verification

Historical installers and manifests remain available in `installer/output`;
the public distribution directory intentionally contains only the latest pair.

The enrollment key is consumed after the first successful API registration;
the resulting installation token is stored with Windows DPAPI. Use
`-SkipBootstrap` only for a non-distributable diagnostic installer.

To build without recompiling the solution, pass `-SkipBuild`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File installer\build-installer.ps1 -SkipBuild
```

The generated installer is what the update API exposes for automatic updates.

## Run

Run from Visual Studio (Start) or execute the built binary directly:

- `SSW\bin\x86\<PROFILE>\SSW.exe`

Example:

- `SSW\bin\x86\CL\SSW.exe`

## Data Files

At runtime the application expects a SQL Server Compact data file:

- `data\DataCentral.sdf` located next to the executable.

For normal runs, ensure the `data` folder is present in the output directory with the correct `.sdf` file for the chosen profile.

## Localization

Localized resources live in `SSWLib/Resources.*.resx`. Available resource languages in this repo include:

- `bg`, `da`, `de`, `en`, `fr`, `hu`, `it`, `nl`, `pl`, `ro`, `sl`, `sv`

## Reporting

Report generation uses Microsoft ReportViewer. Output templates are deployed alongside the binaries (e.g., `CLMainReport.rdlc` in `bin` folders).

## Commercial Sheet Lookup

Commercial Sheet lookup is implemented in `SSWLib/CLMainForm.vb` and is based on folder structure + model naming.

### Local Folder Structure

- Base folder: `css` next to executable (`SSW\bin\x86\<PROFILE>\css`)
- Serie folder: `S<SerieCode>` (example: `S0`)
- Language folder: `<LANG>` (example: `EN`, `IT`)
- File name pattern:
  - Standard models (3 or more tokens): `token[2]_token[1]_<LANG>_<SHORTNAME>.pdf`
  - Models with 2 tokens: `token[1]_<LANG>_<SHORTNAME>.pdf`

Example:

- Model name: `PRIME 030BD OSC`
- Profile shortname: `AV`
- Language: `EN`
- Expected file: `OSC_030BD_EN_AV.pdf`
- Expected path: `...\css\S0\EN\OSC_030BD_EN_AV.pdf`

### Exceptions

- Serie code `32` maps to folder `SA` (instead of `S32`).
- If selected language is not available, fallback language is `EN`.

### Online Fallback (AV Only)

For profile shortname `AV`, lookup tries online first, then local fallback:

- Base URL:
  - `https://www.avensys-srl.com/ftproot/DOCUMENTS/Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units/LEAFLETS`
- Same structure from serie folder onward:
  - `/<Sxx or SA>/<LANG>/<FILENAME>.pdf`
- Downloaded files are stored/updated directly in local `css`:
  - `SSW\bin\x86\<PROFILE>\css\<Sxx or SA>\<LANG>\<FILENAME>.pdf`

For non-AV profiles, lookup is local only.

### Diagnostic Log

Commercial Sheet lookup writes diagnostics to:

- `SSW\bin\x86\<PROFILE>\commercialsheet_lookup.log`

Log includes:

- full path being tried
- fallback to `EN`
- found/not found status
- online URL attempts and errors

## Signing and Publish

The `SSW` project includes multiple `.pfx` key files and ClickOnce settings in the project file. If you publish or sign builds, verify the correct key and publishing settings for the target customer.

## Dependencies

See:

- `SSW/packages.config`
- `SSWLib/packages.config`

These define all NuGet dependencies and versions used by each project.

## License

No license file is present in this repository. Treat the code and assets as proprietary unless a license is added.

## Troubleshooting

- Missing packages: run NuGet restore; the project will fail with a clear error if EF or SQL Server Types packages are missing.
- Missing data: ensure `data\DataCentral.sdf` exists in the output folder for the profile.
- Wrong branding: verify the selected solution configuration matches the intended profile (`AC`, `CL`, `SIG`, etc.).
- Commercial Sheet disabled:
  - check `commercialsheet_lookup.log`
  - verify serie folder (`S<code>` or `SA` for serie `32`)
  - verify language folder and final file name pattern
  - for `AV`, verify internet access to the configured online base URL
