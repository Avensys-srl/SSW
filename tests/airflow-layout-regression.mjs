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
const styles = fs.readFileSync(new URL('../frontend/ssw-next/src/styles.css', import.meta.url), 'utf8');
assert.ok(main.includes('compatibleLayouts.length > 0 && !calculating && !calculationFailed'));
assert.ok(!main.includes('class="core"'));
for (const role of ['fresh', 'supply', 'return', 'exhaust']) {
  assert.ok(fs.existsSync(new URL(`../frontend/ssw-next/public/airflow/${role}.png`, import.meta.url)));
}
assert.ok(main.includes('src="/airflow/${role}.png"'));
assert.ok(main.includes('class="airflow-legend"'));
assert.ok(main.includes('data-port="${position}" class="duct-marker'));
assert.ok(main.includes('class="airflow-legend-item access-direction${surface === "front" ? " observer-side" : ""}"'));
assert.ok(main.includes('accessDirectionArrow(surface)'));
assert.ok(main.includes('surface === "front" ? observerAccessLabel() : accessLabel()'));
assert.ok(main.includes('surface === "lower" ? "&uarr;" : ""'));
assert.ok(main.includes('" observer-cross"'));
assert.ok(main.includes('class="access-indicator"'));
assert.ok(main.includes('class="installation-view-caption${isSameSideConnection() ? " same-side-caption" : ""}"'));
assert.ok(main.includes('installationViewLabel(draft!.installationMode, isOppositeSideEastWestWall())'));
assert.ok(main.includes('" same-side-caption"'));
assert.ok(styles.includes('.ahu-plan small.access-indicator'));
assert.ok(styles.includes('font-size: 32px;'));
assert.ok(styles.includes('.ahu-plan.access-lower small.access-indicator { top: calc(100% + 8px); bottom: auto; }'));
assert.ok(styles.includes('.ahu-plan.access-upper small.access-indicator { top: auto; bottom: calc(100% + 8px); }'));
assert.ok(styles.includes('.ahu-plan.access-front small.access-indicator { top: calc(42% + 24px); }'));
assert.ok(styles.includes('gap: 1em; padding: 0;'));
assert.ok(styles.includes('.observer-cross::before { transform: rotate(45deg); }'));
assert.ok(styles.includes('.observer-cross::after { transform: rotate(-45deg); }'));
assert.ok(styles.includes('.airflow-legend-item.access-direction.observer-side b { width: 15px; height: 15px; }'));
assert.ok(styles.includes('.flow img { position: absolute; display: block; width: 30px;'));
assert.ok(styles.includes('.flow.fresh img, .flow.exhaust img { width: 39.75px; }'));
assert.ok(styles.includes('.airflow-legend-item img { justify-self: start; width: 27px;'));
assert.ok(styles.includes('.connection-ssc:not(.ssc-flat-floor):not(.ssc-upright-floor) .flow { top: calc(50% - 28px); bottom: auto; }'));
assert.ok(styles.includes('.airflow-diagram .ahu-plan strong { display: none; }'));
assert.ok(styles.includes('.duct-marker { display: none; position: absolute; z-index: 4; pointer-events: none; }'));
assert.ok(!main.includes('${icon("arrow-down")}<span>'));
assert.ok(styles.includes('.connection-ssc:not(.ssc-upright-floor) .ahu-plan.access-lower strong { top: auto; bottom: 32px; }'));
assert.ok(styles.includes('.connection-ssc:not(.ssc-upright-floor) .ahu-plan.access-upper strong { top: 32px; bottom: auto; }'));
for (const color of ['#168bd2', '#279b55', '#e2c600', '#a6533c']) assert.ok(styles.includes(color));
assert.ok(styles.includes('font-family: Arial, sans-serif; font-size: 13px; font-weight: 700;'));
assert.ok(styles.includes('background: #fff; border: 2px solid #111;'));
assert.ok(styles.includes('border-radius: 4px; box-shadow: none;'));
assert.ok(styles.includes('color: #000; background: #fff; border: 1px solid #000; font-family: Arial, sans-serif;'));
assert.ok(styles.includes('.connection-osc.wall-east-west .flow-position-1, .connection-osc.wall-east-west .flow-position-3 { top: calc(30% + 31px); }'));
assert.ok(styles.includes('.connection-osc.wall-east-west .flow-position-2, .connection-osc.wall-east-west .flow-position-4 { top: calc(70% - 31px); }'));
assert.ok(styles.includes('.connection-osc:not(.wall-east-west):not(.wall-north-south) .flow-south { bottom: calc(50% - 80px); }'));
assert.ok(!styles.includes('.flow-south.exhaust { bottom: calc(50% - 50px); }'));
console.log('Airflow view position and stale-result guards passed.');
