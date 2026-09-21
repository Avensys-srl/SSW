import assert from "node:assert/strict";
import fs from "node:fs";

const main = fs.readFileSync(new URL("../frontend/ssw-next/src/main.ts", import.meta.url), "utf8");
const report = fs.readFileSync(new URL("../SSWLib/CLInstallationLayoutReportRenderer.vb", import.meta.url), "utf8");

assert.ok(main.includes("const uprightSscFloor = isSameSideUprightFloor();"));
assert.ok(!main.includes('isSameSideUprightFloor() && ["A1", "B1"].includes'));
assert.ok(report.includes('c.ReferenceView = "SSC_UPRIGHT"'));
assert.ok(!report.includes('c.ReferenceView = "SSC_UPRIGHT" AndAlso {"A1", "B1"}.Contains(code)'));

console.log("SSC upright geometry is driven by catalog metadata in UI and report.");
