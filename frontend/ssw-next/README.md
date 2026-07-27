# SSW Next frontend

Offline-first guided UI for the WebView2 preview host. This folder is
independent from the legacy WinForms controls and does not contain technical
calculation formulas.

## Commands

```powershell
npm install
npm run dev
npm run build
npm run preview
```

- Main workflow: `http://127.0.0.1:5173/#/selection`
- Component showcase: `http://127.0.0.1:5173/#/showcase`

## Bridge boundary

The presentation depends only on `SelectionBridge`.

- In a normal browser, `createBridge()` returns `MockSelectionBridge`, backed
  by deterministic fixtures under `src/mock`.
- In the desktop host, `NativeSelectionBridge` exchanges typed messages with
  `CLNextHostForm`. Model catalog, layout fallback, localized accessories and
  winter/summer balanced calculations come from `SSWLib` and the local SDF.
- Save and report commands currently open the production legacy interface.
  They are not duplicated in TypeScript.

No remote assets, fonts, APIs or CDN resources are required at runtime.

## Desktop preview

Build `SSW.sln` as `AV|x86`, then launch:

```powershell
D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe --next-ui
```

Set `SSW_NEXT_UI_DEV_URL=http://127.0.0.1:5173` before launching to use Vite
hot reload. Without that variable the host loads the assets packaged beside
the executable.
