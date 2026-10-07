# Coil regulation regression audit - 2026-10-01

Build tested: local AV/x86 development build, version 2.0.0.21 with the coil regulation fix. Not a published release.

## Catalog-wide pressure feedback

All 111 catalog models were exercised using the production FindCompatibleFanOperatingPoint method from the compiled SSWLib.dll.

- Airflows: 60%, 80%, and 100% of nominal airflow.
- Requested static pressures: 100 and 200 Pa.
- Synthetic additional pressure losses: 10, 42, 55, and 100 Pa.
- Minimum search regulation: 70%.
- Feedback: calculated regulation becomes the next minimum, exactly as in the reported failure mechanism.
- 2,152 evaluated combinations across all 111 models; all stabilized within two passes.
- 1,804 stable cases within pressure capacity.
- 348 stable cases exceeding pressure capacity (correctly reported as unavailable).
- 128 base airflow/pressure combinations were unavailable before adding a pressure loss and were not counted as feedback tests. Every model still had at least one evaluated combination.
- Zero nonconvergence failures or exceptions.

These are solver regression tests, not 2,152 complete thermal or UI validations. Synthetic losses do not establish suitability of an actual coil.

## Full calculation samples

The earlier exact CLRC 223 SSC / CWD 163 reproduction at 2,000 m3/h and 100 Pa passed HCD, CWD, and HWD via the full Calculate service.

The additional full-service sweep produced 28 completed cases for PRIME 020BD EN/LT before it was deliberately stopped due to runtime: 22 stable and 6 stable capacity-limited cases. The CSV is a partial sample, not a completed catalog-wide full-service audit. An additional partial direct coil/thermal routine sweep completed eight cases with OK coil statuses.

## Reproduction

Run tests/Invoke-CoilCatalogAudit.ps1 against the desired AV/x86 BinaryDirectory and choose an OutputPath for CSV evidence. Default mode tests the pressure-feedback solver. Optional -FullCoilCalculation exercises the production seasonal and coil routines through reflection, without unrelated layout/document/UI work; this mode has not completed a full catalog sweep.

The existing tests/Invoke-CoilNormalizationSmoke.ps1 remains the full-service regression for the customer's original reproduction. No additional production code changes were required by this audit.
