# SSW agent instructions

## Mission

Evolve SSW without rewriting or invalidating the verified calculation
algorithms. The current WinForms application remains the behavioral reference
and release fallback while the application layer is separated from the UI and
a new guided desktop UI is introduced.

The target architecture is:

- existing calculation libraries, HEDes integration, SDF data, RDLC reports,
  APIs and update system remain authoritative;
- calculation orchestration and selection state become independent from
  WinForms controls;
- the new desktop presentation is a local HTML/CSS/TypeScript application
  hosted by WebView2 in the Windows x86 application;
- all technical functions continue to work offline;
- commercial pricing, quotations, CRM and customer authentication are out of
  scope until explicitly scheduled.

## Repository strategy

- Continue in the existing `Avensys-srl/SSW` repository.
- Do not create a copied repository or restart history.
- Keep `master` releasable.
- Perform the modernization on a dedicated branch until the new UI reaches
  agreed parity.
- Add new projects beside the legacy projects; do not replace the legacy
  executable at the beginning of the migration.
- Commit by testable architectural increment. Avoid one final large migration
  commit.

## Major release objective

- The current WinForms product line remains `1.3.0.xx`.
- The first public release in which the guided WebView2 UI is the default
  technical-selection experience is `2.0.0.0`.
- Do not publish modernization previews through the normal production update
  manifest. Use a separate preview channel and informational preview version.
- Do not change `SSWVersion.props` to `2.0.0.0` merely because the new shell
  starts or renders.
- `2.0.0.0` is a release gate, not a development branch label.
- Software major version, selection file schema, SDF schema, calculation
  engine version, report template version and API version remain independent.

## Authoritative documents

Start every session by reading `docs/README.md` and the history for the area
being changed. Keep proposals, implemented solutions and verified outcomes
distinct. Record significant problems, decisions and fixes in `docs/`, with
dated checkpoints, reproducible evidence and remaining work; add new documents
to the index and include them in the corresponding Git change. Do not rely on
conversation history alone, and revalidate older findings against current code.

Read these documents before changing the corresponding area:

- `docs/UI_BACKEND_SEPARATION_ROADMAP.md`
- `docs/UNBALANCED_AIRFLOW_CONTRACT.md`
- `docs/INSTALLATION_LAYOUT_ROADMAP.md`
- `docs/TECHNICAL_SELECTION_ROADMAP.md`
- `docs/TECHNICAL_SELECTION_VERSIONING.md`
- `docs/TECHNICAL_SELECTION_TEST_MATRIX.md`
- `docs/ACCESSORIES_CONTROL_FUNCTIONS_ROADMAP.md`
- `docs/FOLLOW_UP_REMINDERS_ROADMAP.md`

Update the relevant roadmap after every completed checkpoint.

## Non-negotiable architecture rules

1. Domain and application code must not accept, return or mutate
   `TextBox`, `ComboBox`, `DataGridView`, `Chart`, `Form` or other UI types.
2. Existing formulas must not be changed while extracting them from
   `CLMainForm`.
3. Use typed input/output DTOs. UI formatting and localization belong to the
   presentation layer.
4. Chart-producing services return numeric series, axes and working points,
   not rendered controls or bitmaps.
5. New contracts must support separate supply and extract airflow from the
   beginning. Balanced mode sets the two values equal.
6. Preserve `.sswsel` backward compatibility. Missing branch-specific values
   in old files are initialized from the legacy single airflow.
7. Preserve SDF offline operation and avoid hardcoded commercial data that
   belongs in Explorer or the central database.
8. Do not migrate runtime, SQL Server Compact, RDLC or native x86 libraries in
   the same change that separates the UI.
9. The legacy UI must continue to use the extracted application services
   through an adapter. Do not maintain two calculation implementations.
10. Commercial prices, currencies, quotation totals, CRM states and customer
    logins are not part of the technical UI modernization.

## Required application boundaries

The extraction should converge on boundaries equivalent to:

- `SelectionInput`: all technical inputs and selected product data;
- `AirflowPair`: supply and extract airflow;
- `SelectionApplicationService`: selection orchestration and validation;
- `SelectionResult`: seasonal, aeraulic, thermal and electrical results;
- `BranchOperatingPoint`: airflow, pressure, power and specific performance
  for one branch;
- `ChartData`: UI-neutral chart series and working points;
- infrastructure repositories for SDF, images, APIs and persistence;
- legacy WinForms adapter;
- WebView2 host adapter.

Names may follow existing conventions, but responsibilities must remain
separate.

## Unbalanced airflow

Before changing airflow calculations, read
`docs/UNBALANCED_AIRFLOW_CONTRACT.md`.

Important rules:

- input and output are branch-specific;
- the heat recovery scenario is coupled and must not be approximated as two
  unrelated balanced calculations;
- supply and extract pressure curves and working points are distinct;
- supply and extract fan power are distinct;
- total fan power is their sum;
- total SFP is the sum of the two branch SFP values, according to the approved
  SSW convention;
- winter and summer efficiency views show both branch working points;
- branch and total values must be available to UI, report, project file,
  snapshot, API and portal;
- balanced-mode regression must reproduce current outputs within the agreed
  tolerance.

## New UI

- The new UI is a guided technical workflow, not a visual copy of the current
  tabs.
- Keep the stable technical steps: project, guided preselection, detailed unit
  selection, installation/layout, water coils, electric heaters, accessories,
  technical documents and summary/email/follow-up.
- Optional steps are skipped or clearly marked when unavailable.
- Keep persistent context visible: project, selected unit, operating point,
  selected accessories and validation state.
- Bundle all production frontend assets locally. No network connection is
  required to calculate, reopen a project or generate a report.
- In Debug, the WebView2 host may load a local frontend development server for
  hot reload. In Release, it must load the packaged assets.
- The current WinForms application remains available until numerical,
  persistence, reporting and release parity is demonstrated.

## Visual development workflow

WebView2 does not provide a WinForms-style drag-and-drop designer for the HTML
content. Agents must therefore provide:

- a local development command with hot reload;
- a component/demo route containing the reusable controls and states;
- responsive preview sizes matching supported desktop layouts;
- browser developer-tools support in Debug;
- deterministic screenshots for review;
- CSS design tokens for spacing, colors, typography and control dimensions.

The Visual Studio designer is expected to show only the WebView2 host control.
The accurate UI preview is the running frontend in a browser or in the Debug
desktop host.

## Validation

For every extraction step:

1. capture representative current results before editing;
2. run the same inputs through the extracted service;
3. compare numeric and chart-series outputs;
4. test balanced airflow explicitly;
5. open legacy `.sswsel` fixtures;
6. compile `SSW.sln` with configuration `AV`, platform `x86`;
7. for desktop validation launch exactly:
   `D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe`;
8. verify the tested process path before interacting with the UI.

The minimum regression set must cover:

- winter only and winter/summer;
- balanced and unbalanced airflow when available;
- no coil, water coil, PEHD and EHD;
- enthalpic and non-enthalpic heat exchangers;
- accessories with pressure losses;
- save/reopen/report;
- current and historical project schema versions;
- offline operation.

## Agent coordination

- Assign one owner to application contracts and calculation extraction.
- Agents working on Explorer/SDF, frontend components, translations or
  read-only audits may work in parallel only when their files and contracts do
  not overlap.
- Do not let multiple agents modify `CLMainForm`, project schema or calculation
  contracts concurrently.
- Every delegated task must state inputs, owned files, expected outputs,
  validation and handoff notes.
- Record unresolved technical decisions in the relevant roadmap instead of
  silently choosing a formula or fallback.
- Before every release, run the existing Help/UX audit and update help,
  tooltips and all supported localizations for user-visible changes.

## Release gate

A modernization increment is publishable only when:

- legacy and new paths use the same calculation implementation;
- balanced-mode reference results match;
- project migrations are tested;
- SDF schema and required images are available offline;
- all relevant RDLC files and API payloads are aligned;
- Help and localization parity checks pass;
- installer, manifest, changelog and release documentation are updated;
- all involved repositories are committed and pushed intentionally.

The first `2.0.0.0` production release additionally requires:

- the new guided UI is the default entry point;
- the agreed technical scope has parity with the legacy UI;
- historical project files open and migrate correctly;
- balanced calculation reference results match;
- installation layout, guided preselection and unbalanced airflow are either
  complete or explicitly removed from the approved `2.0.0.0` scope before the
  release candidate;
- reports, email, follow-up, offline mode and automatic update are validated;
- rollback to the last `1.3.0.xx` production release is documented and tested.
