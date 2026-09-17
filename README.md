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

- Application version: `2.0.0.5` (single source: `SSWVersion.props`)

## Prerequisites

- Windows
- Visual Studio 2019 or newer with ".NET desktop development" workload
- .NET Framework 4.8 Targeting Pack

Optional for runtime distribution:

- Microsoft SQL Server Compact 4.0 runtime (native binaries are copied post-build)

Required for installer builds:

- Inno Setup 6
- Windows SDK SignTool, available from the Windows SDK
- A valid code-signing certificate installed in the Windows certificate store. The installer build script reads the signing certificate thumbprint from `SSW/SSW.csproj`.

## Build

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
- uses the email and six-digit installation PIN flow for new device activation,
  without embedding enrollment secrets in the installer
- creates `installer/output/SSW_Setup_<version>.exe`
- leaves the installer and manifest in `installer\output` for pilot testing
- publishes only when `-PublishCopyDir "F:\DOCUMENTS\tools\Selection Software"`
  is passed explicitly; publication verifies integrity and removes older
  `SSW_Setup_*` public artifacts only after verification

Historical installers and manifests remain available in `installer/output`;
the public distribution directory intentionally contains only the latest pair.

The optional legacy bootstrap parameters remain available only for controlled
backward-compatibility builds. Normal public installers omit the bootstrap key;
the resulting device token is stored with Windows DPAPI after activation.

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
