# Technical calculation baselines

The input files are synthetic `.sswsel` selections. They contain no customer
identity, public reference, registration token, or production selection data.

Coverage:

- `standard-accessories`: winter, summer, charts, report datasets, accessories;
- `water-coil-hcd`: CWD and HWD calculations using one HCD coil;
- `electric-heaters`: PEHD and EHD calculations;
- `enthalpic-enrfc27`: enthalpic temperature/humidity recovery and zero condensate.

Expected JSON files are produced only with the explicit approval command:

```powershell
tests\Invoke-TechnicalBaselines.ps1 -Update
```

Normal execution compares the current AV/x86 executable with the committed
expected output and applies the tolerances documented in
`technical-baseline-tolerances.json`.
