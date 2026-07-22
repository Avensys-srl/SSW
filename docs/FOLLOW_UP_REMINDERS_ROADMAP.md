# SSW follow-up reminders roadmap

## Objective

Add a small, non-invasive notification centre to SSW for commercial follow-up
after a technical selection or a multi-selection project has been prepared for
email. A reminder belongs exclusively to the Windows installation that created
it. Other SSW installations, including installations of the same customer,
must never receive or display it.

This roadmap is the implementation contract for SSW, the technical-selection
API and the related database migration.

## Approved product decisions

- The feature represents a **follow-up reminder**, not the technical expiry of
  a selection or project.
- Reminder scheduling is offered when SSW successfully prepares an Outlook
  email with the relevant PDF attachment or project PDF attachments.
- SSW cannot prove that Outlook actually sent the message. The recorded event
  therefore means `EmailPrepared`, never `EmailSent`.
- Scheduling is enabled by default with a default delay of 7 days.
- The user can choose any whole number from 1 through 90 days.
- The due instant is calculated from the local calendar date and stored in UTC.
  The UI displays dates and times in the local time zone.
- Due reminders do not open modal dialogs at application startup. A coloured
  bell indicator and unread counter provide the notification.
- The notification centre contains upcoming, due/overdue and closed reminders.
- Available actions are:
  - close successfully;
  - close unsuccessfully;
  - reschedule for another 1-90 day interval, proposing 7 days again;
  - cancel a reminder created by mistake.
- Double-clicking a notification opens its referenced `.sswsel` or `.sswproj`
  file when the local path still exists.
- When the file no longer exists, the reminder remains readable and SSW shows
  a localized, non-destructive explanation instead of removing the record.
- Reminders are private to the installation/PC. API queries and mutations must
  derive ownership from the authenticated installation token and must not
  accept a caller-supplied installation identifier.
- All UI text is localized in every language supported by SSW. Language names
  in the language menu remain untranslated autonyms.

## Scope boundaries

### Included

- Single technical selections and `.sswproj` multi-selection projects.
- Central persistence through the existing authenticated SSW API.
- A local cache and offline mutation queue for continuity without a network.
- Notification centre, bell state, filters, actions and local-file opening.
- Idempotent creation and mutation APIs.
- Automated API, persistence and SSW tests.

### Excluded from the first release

- Reading Outlook delivery or read receipts.
- Sending email directly through SMTP, Microsoft Graph or an Outlook add-in.
- Sharing reminders between PCs or operators.
- Customer contact data, recipient email addresses, commercial values or free
  text notes on the server.
- Windows toast notifications and background services while SSW is closed.
- Portal management of reminders. The schema must permit a future internal
  read-only view, but the first release is operated from SSW only.

## Domain model

### Server reminder

Create `ssw_follow_up_reminders` with at least:

| Field | Purpose |
| --- | --- |
| `id` | Internal primary key. |
| `reminder_uuid` | Random UUID used by API routes. |
| `customer_id` | Tenant boundary inherited from the token. |
| `installation_id` | Private owner inherited from the token. |
| `target_type` | `Selection` or `Project`. |
| `selection_id` | Optional link to the registered technical selection. |
| `multi_project_id` | Optional link to the synchronized project. |
| `target_uuid` | Stable `.sswsel` project ID or `.sswproj` project UUID. |
| `display_reference` | Snapshot used in the notification text. |
| `email_prepared_at_utc` | Time Outlook draft preparation succeeded. |
| `due_at_utc` | Current reminder deadline. |
| `status` | `Pending`, `Succeeded`, `Unsuccessful`, `Cancelled`. |
| `reschedule_count` | Number of follow-up postponements. |
| `closed_at_utc` | Completion/cancellation time. |
| `created_at`, `updated_at` | Audit timestamps. |

Constraints:

- unique `(installation_id, reminder_uuid)`;
- indexed `(installation_id, status, due_at_utc)` for the notification poll;
- exactly one valid target type and corresponding optional foreign key;
- no local path stored centrally;
- no recipient, customer name, email body or PDF uploaded.

Create `ssw_follow_up_reminder_events` as an append-only history containing the
old/new due date and status, event type, event timestamp and owning
installation. This keeps rescheduling and closure auditable without exposing
the history to other installations.

### Local record

The local cache adds data that must not be sent to the server:

- absolute `.sswsel` or `.sswproj` path;
- last known display name;
- last successful synchronization time;
- unread/read state for the bell;
- queued mutation and idempotency key;
- local retry count and last diagnostic.

Store it below `%LocalAppData%\Avensys\SSW` using atomic replacement. Protect
concurrent SSW instances with a named mutex. The cache is operational state,
not part of either portable file format.

## API contract

All routes require the existing bearer installation token. Every read and
write includes `WHERE installation_id = :authenticated_installation_id`.

Proposed endpoints:

- `POST /api/v1/follow-ups`
  Creates a reminder after an email draft was prepared. Requires an
  `Idempotency-Key` and accepts target type/UUID, optional registered target
  reference, display reference, prepared timestamp and due timestamp.
- `GET /api/v1/follow-ups?scope=active&updated_since=<utc>`
  Returns only reminders owned by the authenticated installation. Supports a
  bounded incremental refresh and a full active refresh.
- `POST /api/v1/follow-ups/{uuid}/reschedule`
  Replaces the due timestamp, increments the counter and records an event.
- `POST /api/v1/follow-ups/{uuid}/close`
  Accepts only `Succeeded`, `Unsuccessful` or `Cancelled` and records an event.

Rules:

- validate delay-derived dates to the product range of 1-90 days;
- reject target/customer mismatches without revealing target existence;
- use the existing idempotency service for create, reschedule and close;
- cap list sizes and reject malformed UUIDs and timestamps;
- do not add a public read route;
- audit only technical action metadata, never local paths.

## SSW integration points

### Single-selection email

Integrate below `CLReportViewerForm.EmailButton_Click` only after
`CLOutlookEmailService.DisplayMessage` has successfully created and displayed
the Outlook draft. Before draft creation, show a compact scheduling control or
dialog with:

- checked `Schedule follow-up` option;
- numeric day value, default 7, range 1-90;
- localized explanation that the reminder starts when the email draft is
  prepared.

The context passed to the report viewer must include the stable selection
project UUID, current registered reference and current `.sswsel` local path.

### Project email

Integrate below `CLMultiSelectionProjectForm.EmailClick` after
`CLOutlookEmailService.DisplayHtmlMessage` succeeds. Use the `.sswproj` project
UUID/reference and current local package path. One project email creates one
project reminder, not one reminder per attached PDF.

### Notification centre

Add a bell command to the main form using the existing menu/toolbar visual
language. It has stable dimensions and three states:

- neutral: no due unread reminder;
- coloured: at least one due unread reminder;
- coloured with numeric badge: bounded display such as `9+`.

The centre is a dedicated WinForms dialog with:

- `Due/overdue`, `Upcoming`, `Closed` filters;
- reference, type, due date, age/status and reschedule count;
- success, unsuccessful, reschedule and cancel commands;
- refresh command and a quiet offline/synchronization status;
- double-click and `Open` command for the local source file.

Opening uses the existing selection/project loading entry points. It must first
run the normal unsaved-changes guard. Missing or moved files produce a localized
message and leave the reminder intact.

### Refresh policy

- Load the local cache immediately at startup so the bell never waits for the
  network.
- Start one asynchronous API refresh after the main form is usable.
- Refresh periodically while SSW is open, with a conservative interval such as
  15 minutes, and when the notification centre is opened manually.
- Network failures remain silent outside the centre and never block selection,
  calculation, reporting or email preparation.
- A successful sync merges by reminder UUID and server `updated_at` timestamp.

## Offline and idempotency strategy

1. After Outlook draft creation succeeds, create the local reminder
   immediately with a random reminder UUID and idempotency key.
2. Attempt the server create asynchronously.
3. If unavailable, retain the queued mutation and keep the reminder active
   locally.
4. Retry at startup, periodic refresh and manual refresh.
5. Apply the same local-first pattern to reschedule and close operations.
6. Preserve mutation ordering per reminder. A close must never be uploaded
   before an earlier create/reschedule.
7. Server idempotency makes process crashes and repeated retries harmless.

## Localization inventory

Provide localized resources for:

- notification centre title, bell tooltip and unread counter;
- selection/project target labels;
- schedule checkbox, day field and 1-90 validation;
- due, upcoming, succeeded, unsuccessful and cancelled states;
- reminder message with target reference;
- success, unsuccessful, reschedule, cancel, open and refresh commands;
- missing local file and offline synchronization messages;
- email-prepared semantic explanation.

The canonical source is English. Italian must be reviewed manually. Remaining
languages follow the existing 14-language resource policy and require a final
technical language review.

## Delivery waves

### Wave A - Contract, migration and tests

- [x] Assign the next numbered API database migration.
- [x] Add reminder and event tables, indexes and constraints.
- [x] Extend PHP test fixtures with the new tables.
- [x] Implement service methods and authenticated/idempotent routes.
- [x] Test installation isolation explicitly with two active installations.
- [x] Test create replay, reschedule, all closure outcomes and invalid access.
- [x] Update API/database documentation and deployment migration checklist.

Completion evidence: PHP test suite passes on SQLite fixtures and migration is
reviewed for production MariaDB 10.1 compatibility.

### Wave B - Local repository and API client

- [x] Add reminder DTOs, statuses and versioned local envelope.
- [x] Implement atomic local persistence, named mutex and corruption fallback.
- [x] Add ordered offline mutation queue and idempotency keys.
- [x] Extend the existing authenticated technical-selection API client.
- [x] Implement merge and incremental refresh behaviour.
- [x] Add deterministic tests for UTC/local conversion and 1-90 day limits.

Completion evidence: tests cover restart, concurrent instances, offline create,
retry, duplicate retry and close-after-reschedule.

### Wave C - Email scheduling

- [x] Add the localized scheduling UI shared by single and project email.
- [x] Pass stable target UUID/reference and current local path to both flows.
- [x] Create reminders only after Outlook draft creation succeeds.
- [x] Ensure Outlook/PDF failures do not create reminders.
- [x] Keep email preparation usable when API synchronization is offline.

Completion evidence: single and project smoke tests prove one successful draft
creates exactly one local reminder and failed drafts create none.

### Wave D - Notification centre

- [x] Add bell indicator, bounded badge and localized tooltip.
- [x] Implement notification list, filters, actions and refresh state.
- [x] Implement non-modal startup/periodic refresh.
- [x] Open `.sswsel`/`.sswproj` by double-click through existing lifecycle
  guards.
- [x] Handle missing files and moved paths without deleting server history.
- [x] Persist local read/unread state.

Completion evidence: graphical checks on large and reduced form sizes, all
actions verified, no modal startup warning and no UI-thread network blocking.

### Wave E - Localization and regression

- [x] Complete and validate all supported language resources.
- [x] Run resource-key completeness and no-empty-value checks.
- [x] Verify reminder privacy with two installations belonging to AV.
- [x] Verify existing selection registration, report viewer, project email and
  offline report generation remain unchanged.
- [x] Build `AV|x86` and test the exact executable under
  `D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe`.

Completion evidence: API tests, SSW automated tests, AV/x86 build and manual
workflow matrix pass.

### Wave F - Deployment and operational close-out

- [x] Back up the production selection database.
- [x] Apply and verify the reminder migration.
- [x] Deploy API files and run health/smoke checks.
- [x] Update changelog, versions, installer and update manifest.
- [x] Commit and push each affected repository with narrow scope.
- [x] Mark this roadmap complete with migration, build and release evidence.

Completion evidence (2026-07-22): API schema 6 is deployed and healthy; the
API service tests and the complete SSW release matrix pass; the exact AV/x86
runtime is responsive; the 14-language Help/UX and SDF catalog gates pass; and
the signed `SSW_Setup_1_3_0_55.exe` is published with verified manifest and
SHA-256 `CE6AE088CB9D48274404A085DD40FD18431878D662D6CF66557C8038F69BC575`.
The packaged SDF is schema 3/exporter 1.2.0 and its SHA-256 is
`044D444C8C75082618E356CC2BE71B4D21FA735CF4F6E316886C2E935766688F`.

## Acceptance matrix

At minimum validate:

1. Single selection, online, default 7-day reminder.
2. Single selection with 1-day and 90-day limits.
3. Project email creates one project reminder.
4. Outlook unavailable or attachment failure creates no reminder.
5. Offline creation appears locally and synchronizes once only later.
6. Due reminder colours the bell without opening a modal dialog.
7. Double-click opens the correct `.sswsel` and `.sswproj`.
8. Missing local file is reported without losing the reminder.
9. Success and unsuccessful closure remove the item from active results.
10. Reschedule updates the due date and increments the counter.
11. A second installation cannot list, mutate or infer the reminder.
12. Existing report and project workflows remain operational with reminders
    disabled by the user.

## Deferred extension

The internal portal may later expose aggregate or support views. This must be a
separate, explicitly authorized feature. The SSW API contract remains private
per installation and does not gain a cross-installation endpoint as a side
effect.
