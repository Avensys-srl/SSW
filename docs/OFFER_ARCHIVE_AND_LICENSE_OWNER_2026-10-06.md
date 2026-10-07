# Offer archive and licensed owner - 2026-10-06

## Requested behavior

Definitive offers remain visible in SSW even when no reminder is requested.
Reminders are optional scheduled follow-ups, not the offer register. Portal
customer display should identify the user associated with the licensed device.

## Implemented

- SSW notification center now separates Definitive offers and Reminders.
- Local archive reads previously saved definitive selections, including those
  without reminders; provisional or mismatched-revision records are excluded.
- New saves include the revision number so R02 does not overwrite R01.
- Existing old paths remain readable. Duplicate saved copies of the same public
  reference/revision are grouped, including aliases for reminder matching.
- Offer rows show public reference, revision, model, date and reminder state.
  The folder button reopens the archived selection using the existing bridge.
- New UI labels cover the 15 supported languages.
- Portal dashboard resolves latest revision installation -> ssw_installations.user_id
  -> ssw_users. Name/surname takes precedence, then email, then historical company.
  Company/code remain visible below the user name. Search includes name and email;
  count and row queries use identical joins. Missing license links do not remove rows.

## Acceptance

- AV/x86 build and TypeScript/frontend build passed.
- Test-OfferArchive.ps1 passed: definitive without reminder, revision retention,
  reprint deduplication and provisional exclusion; only temporary fixture files used.
- Native WebView screenshot reviewed with two definitive offers and zero reminders.
- Native folder-button reopening passed, preserving definitive identity.
- Next boundary and selection normalization regressions passed.
- Portal's 98 existing tests passed locally and after source deployment.
- New SQLite fixture tests passed for latest-revision owner, search/count parity,
  historical fallback and location-filter behavior. No live database writes.

## Deployment and boundaries

Portal source deployed to SERVER01 ssw-portal: src/SelectionRepository.php,
src/SelectionPresenter.php and templates/dashboard.php. Original source backup:
C:\Users\PC\Documents\SSW_develop\tmp\portal-offer-owner-20261006\backup-20261006-154426.
Validated source overlay and fixture test are preserved in tools/portal-offer-owner.
No SQL migration required; no user/device ownership records modified.

SSW standard development binary is now 2.0.0.25. Public installer2.0.0.24 and
its manifest remain unchanged. No new installer publication or Git push performed.

The archive is local to the Windows profile. It does not fetch offers generated
on another device or recover deleted selection files. Licensed owner means the
user currently linked to the installation of that revision, not a separately
stored immutable historical author snapshot. Missing links are never inferred
from company code or email similarity.

The existing portal option Mostra localita non disponibile can hide otherwise
registered selections when unchecked. Its policy was not changed here; enable it
when checking recent offers without location. Attribution tests are fixture-based,
not an authenticated visual inspection of production customer data.
