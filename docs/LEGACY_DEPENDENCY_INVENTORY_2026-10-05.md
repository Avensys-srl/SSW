# Legacy dependency inventory - 2026-10-05

| Component | Incoming dependency | Decision |
| --- | --- | --- |
| CLMainForm partials, designer and resources | Legacy UI, diagnostic baseline capture | Excluded from customer build; sources retained |
| CLTechnicalBaselineCommand | Explicit CLI diagnostic capture | Available only with SSWIncludeLegacyReferenceUi=true |
| CLAboutBoxForm, CLPleaseWaitForm, CLHelpForm | Legacy main form only | Excluded from customer build |
| CLFollowUpNotificationCenterForm and private helpers | Legacy main form only | Excluded; Next reminder service retained |
| CLMultiSelectionProjectForm | Historical project dialog smoke test | Retained for now; separate cleanup requires replacing dialog acceptance |
| Calculation engines, SDF, RDLC, CLReportViewerForm | Next runtime and technical reports | Retained |
| Selection serializers and migration backups | Historical supported selection imports | Retained and tested |
| ClaimLegacyDeviceLicenseAsync | Next startup license migration | Retained; not an old UI dependency |

Default build property: SSWIncludeLegacyReferenceUi=false. Diagnostic builds
must use an isolated output directory and explicit true. The installer rejects
an assembly without the false packaging marker. No source history is deleted.

## Evidence

- Full AV/x86 customer and isolated diagnostic builds passed.
- Assembly boundary tests passed for both modes; shared runtime types retained.
- Next product boundary and frontend selection regression passed.
- All four numeric technical baselines passed against the diagnostic build and
  a copied local catalog; fixtures were not updated by this work.
- SelectionIdentitySmoke passed against the isolated Next-only assembly,
  including historical file migration, backup, identity and reminder contracts.
  Two tests specific to retired main-form controls are explicitly skipped;
  Next normalization remains covered separately.
- Standard AV/x86 executable rebuilt; customer build contains no CLMainForm.

## Remaining release gates

Actual 1.x installer upgrade requires an isolated Windows profile or VM with
an existing 1.x installation, representative saved files and licensing state.
Verify preservation, Next startup, import and rollback there before publication.
No installer was published or existing installer overwritten.
Fresh visual PDF acceptance is still required before release.
Server rejection of old clients remains deferred pending installation inventory
and separately authorized staged deployment; no API rejection was introduced.
