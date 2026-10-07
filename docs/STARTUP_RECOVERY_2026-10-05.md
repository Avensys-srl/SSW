# SSW startup verification - 2026-10-05

Status: AV/x86 build and normal desktop startup verified locally.

The reported Windows exception code 0xe0434352 did not include a managed
exception stack trace. The original cause is therefore not established.

## Database isolation

The previous catalog was restored as `SSW/bin/x86/AV/data/DataCentral.sdf`.
The newer catalog remains preserved as
`DataCentral.sdf.diagnostic-20261005-091144` in the same directory.
The restored catalog passed the document coverage smoke check (111 models).

## Verification and corrected diagnostic interpretation

Earlier restricted-process tests stopped after WebView2 initialization and
navigation to the packaged frontend. Repeating the final executable test
outside the restricted execution environment completed successfully. Those
restricted tests do not establish a defect in the packaged UI or database.

Temporary startup tracing was removed from the source. The complete solution
was built with Visual Studio 2019 MSBuild, configuration AV and platform x86;
the frontend TypeScript and production Vite build also completed successfully.
No calculation or WebView2 navigation workaround was required.

The exact final executable `SSW/bin/x86/AV/SSW.exe` was then launched normally,
without screenshot arguments. Its window remained responsive and titled
`Avensys Selection Software - UI Preview (SDF connected)`.
The application log recorded successful `app.initialize`, navigation,
`selection.preselect`, `selection.calculate`, `project.workspace` and
`notifications.peek` completion at approximately 09:28 local time.
The process was left open for user verification.

The original reported crash has not been reproduced in this successful test.
The preserved newer catalog has not been reactivated or validated by this
checkpoint. No public release or central API changes were performed.

## Newer catalog restored, 09:29 local time

At the user's request the preserved newer catalog was copied back to
`SSW/bin/x86/AV/data/DataCentral.sdf`, with its SHA256 verified before startup.
The previous active catalog was preserved as
`DataCentral.sdf.previous-restored-20261005-092937`; the diagnostic newer
catalog copy also remains available.

Normal startup of the exact final executable succeeded with the newer catalog.
The window remained responsive, connected to SDF, and the log recorded
successful navigation, initialization, preselection, calculation and workspace
loading at 09:29. The newer SDF therefore does not reproduce the reported
startup failure in this test. The application was left open.
