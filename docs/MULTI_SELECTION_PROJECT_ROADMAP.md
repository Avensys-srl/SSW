# SSW multi-selection project roadmap

## Scope

The `.sswproj` format groups multiple existing `.sswsel` selections and their
verified PDF reports. The existing single-selection format and registration
workflow remain unchanged. Project metadata is synchronized with the central
API when a connection is available; the portable project and its PDFs remain
local.

## Decisions

- The project reference is mandatory and defaults to the localized
  `Project 01` value.
- Every item in a project uses the same project language.
- Updating an existing selection replaces its row while preserving its order.
- Creating an alternative adds a new row.
- The project is a portable ZIP package containing `project.json`, one
  `.sswsel` entry per selection and one PDF entry per selection.

## Local implementation

- [x] Versioned project model and atomic ZIP persistence.
- [x] Project manager with create, open, save, add/update, remove and open.
- [x] PDF freshness and language checks.
- [x] Localized HTML email summary with all valid PDFs attached.
- [x] Automated persistence, compatibility and email-composition tests.
- [x] AV x86 build and graphical verification.

## Central synchronization

- [x] Authenticated, idempotent project synchronization endpoint.
- [x] Project and item persistence linked to registered selections when
  available.
- [x] Offline-safe best-effort synchronization from SSW.
- [x] Project list and detail pages in SSW Portal.
- [x] First-download and update statistics with approximate location and no
  persisted IP address.
- [x] Production schema migration 005 applied and verified.

## Remaining product work

- Unbalanced-airflow calculation and installation-layout selection remain
  separate future developments.
