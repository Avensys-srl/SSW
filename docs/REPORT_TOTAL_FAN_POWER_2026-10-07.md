# Report total fan power

The Next SSW winter and summer working-point tables display only total unit
power in W, replacing the single-branch value. The existing RDLC power row is
reused in all four report templates without changing legacy datasets or report
definitions. No fan-specific wording appears in the caption.

The total sums SupplyBranch.AbsorbedPowerW and ExtractBranch.AbsorbedPowerW,
the branches underlying combined SFP. The current balanced calculation supplies
the same power for both branches; the report sums rather than assumes doubling.
Electric heaters and coils are excluded; no extra electronics consumption is
estimated or added to the existing SFP calculation. Missing branch power leaves the total
blank, rather than reporting an incomplete total. No calculation is changed.

PDF_TotalUnitPower is localized in all 15 SSW language resources. The report
uses its existing document-language localization mechanism.

Verification:
- AV/x86 standard build passed.
- Test-ReportTotalFanPower.ps1 passed: unequal branch powers, zero/missing power,
  winter/summer and all 15 resource captions.
- Native --next-ui-smoke passed, including report preparation/rendering checks
  for total-only power and PDF generation in Italian, Bulgarian and Norwegian.
- Italian first-page PNG visually checked after the total-only change: legible power row,
  no overlap with neighboring tables or subsequent sections.
- Evidence PDFs: C:/Users/PC/Documents/SSW_develop/tmp/total-power-reports.
- Installer and public release unchanged.
