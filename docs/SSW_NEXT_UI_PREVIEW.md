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
- HEDes water-coil catalog and calculation, including the second calculation
  pass at the airflow actually available after the additional pressure loss;
- PEHD/EHD catalog, temperature calculation, compatibility rules and
  additional quadratic air-pressure loss;
- legacy layout variants, dimensions and four flow-port records through a
  UI-neutral repository;
- localized accessory catalog and model relations;
- canonical `.sswsel` document creation and serializer round-trip;
- production save/project workflow reached with the complete selection
  document, with a persistent project workspace in the right sidebar;
- direct report preview from SSW Next, using the existing authoritative RDLC
  templates and datasets without displaying or navigating to the legacy UI;
- dedicated CO2 step with the three established calculation methods,
  300-minute concentration curve and RDLC output;
- dedicated sound step with octave-band calculation, LwA, two sound-pressure
  distances and optional EN ISO 16032 row, with RDLC output;
- contextual help, optional tooltips and the 15 SSW language choices;
- local bundled frontend assets.

Intentionally delegated to the production workflow:

- the final Windows file dialogs;
- technical-selection registration and follow-up scheduling;
- reopening historical projects and creating alternatives.

Project documents remain mono-language. The document language selector in the
right sidebar is deliberately independent from the interface language: changing it
regenerates every embedded project PDF transactionally, while opening a project
selection leaves the current UI language unchanged. Project email composition uses
the selected document language.

The new host creates a canonical `CLSelectionProjectDocument`; it does not copy
formulas, serializers or report logic into TypeScript. UI-neutral services
calculate the selection, CO2 and sound results, then prepare the authoritative
RDLC datasets before SSW Next opens `CLReportViewerForm` directly. The legacy
selection form is never instantiated.

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
layout repository, water-coil calculation and canonical project serializer
round-trip. It also starts the WinForms WebView2 host, waits for the local SDF
bridge and frontend rendering, opens the Layout step and captures a full-page
PNG through the Chromium DevTools protocol. The smoke is part of
`Invoke-TechnicalSelectionReleaseTests.ps1`.

The layout smoke also verifies that selecting two compatible configuration
codes changes the flow-port assignment. The frontend always renders Fresh and
Return as incoming flows and Supply and Exhaust as outgoing flows, regardless
of the side occupied by each port.

The same rendering check can be run directly without desktop capture:

```powershell
D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe `
  --next-ui-screenshot C:\Temp\ssw-next.png layout
```

The optional final argument selects the step prepared before capture. `layout`
waits until all four performance charts are present. Omitting it captures the
initial SSW Next screen. The command returns `0` on success, writes diagnostic
details beside the requested image as `<output>.error.log` on failure, and
never depends on window visibility or Windows Graphics Capture permissions.

The browser prototype has been checked at `1440x900` and `1024x768` across all
ten steps with no horizontal overflow.
