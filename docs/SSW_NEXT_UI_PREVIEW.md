# SSW Next UI preview

![Guided selection preview](ssw-next-selection-preview.png)

## Scope

The preview is an offline WebView2 shell beside the production WinForms
interface. It demonstrates the guided workflow without changing the current
default startup path or the production version.

Implemented with real local data:

- model and series catalog from `DataCentral.sdf`;
- current balanced winter and summer calculation through
  `CLSelectionApplicationService`;
- pressure, power, efficiency, SFP and thermodynamic results;
- legacy layout variants, dimensions and four flow-port records through a
  UI-neutral repository;
- localized accessory catalog and model relations;
- local bundled frontend assets.

Still delegated to the production interface:

- `.sswsel` save, reopen, alternatives and project orchestration;
- HEDes water-coil selection and calculation;
- complete PEHD/EHD orchestration;
- RDLC reports, email and follow-up.

The relevant buttons open `CLMainForm`; no calculation or persistence formula
is copied into TypeScript.

## Build and run

```powershell
cd D:\mdev\SSW
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe' `
  .\SSW.sln /p:Configuration=AV /p:Platform=x86 /m
.\SSW\bin\x86\AV\SSW.exe --next-ui
```

The build runs the TypeScript/Vite build and copies `dist` to
`SSW\bin\x86\AV\frontend\ssw-next`.

For hot reload:

```powershell
cd D:\mdev\SSW\frontend\ssw-next
npm run dev
$env:SSW_NEXT_UI_DEV_URL = 'http://127.0.0.1:5173'
D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe --next-ui
```

## Verification

`tests\Invoke-NextUiSmoke.ps1` verifies the exact AV executable, packaged
frontend, x86 WebView2 loader, model catalog, real winter/summer calculation,
layout repository and JSON serialization. The smoke is part of
`Invoke-TechnicalSelectionReleaseTests.ps1`.

The browser prototype has been checked at `1440x900` and `1024x768` across all
nine steps with no horizontal overflow.
