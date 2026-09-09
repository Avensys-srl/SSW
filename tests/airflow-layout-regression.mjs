import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import { createRequire } from 'node:module';
const require = createRequire(new URL('../frontend/ssw-next/package.json', import.meta.url));
const ts = require('typescript');
const source = fs.readFileSync(new URL('../frontend/ssw-next/src/airflow-layout.ts', import.meta.url), 'utf8');
const context = { exports: {} };
vm.runInNewContext(ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS } }).outputText, context);
const { airflowSlot, airflowSide } = context.exports;
for (const [sameSide, flatFloor, eastWest, slots, sides] of [
  [false, false, false, [3,4,1,2], ['south','south','north','north']],
  [false, false, true, [1,2,3,4], ['west','west','east','east']],
  [true, false, false, [1,2,3,4], ['north','north','north','north']],
  [true, true, false, [1,2,3,4], ['south','south','south','south']],
]) {
  for (let position = 1; position <= 4; position++) {
    assert.equal(airflowSlot(position, sameSide, eastWest), slots[position-1]);
    assert.equal(airflowSide(position, sameSide, flatFloor, eastWest), sides[position-1]);
  }
}
const main = fs.readFileSync(new URL('../frontend/ssw-next/src/main.ts', import.meta.url), 'utf8');
assert.ok(main.includes('compatibleLayouts.length > 0 && !calculating && !calculationFailed'));
assert.ok(!main.includes('class="core"'));
console.log('Airflow view position and stale-result guards passed.');
