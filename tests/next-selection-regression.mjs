import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import ts from '../frontend/ssw-next/node_modules/typescript/lib/typescript.js';

const compile = (source) => ts.transpileModule(source, {
  compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
}).outputText;
const moduleContext = vm.createContext({ exports: {}, structuredClone });
vm.runInContext(compile(fs.readFileSync(new URL('../frontend/ssw-next/src/bridge/normalizeSelection.ts', import.meta.url), 'utf8')), moduleContext);
const { normalizeSelection } = moduleContext.exports;
const baseline = () => ({ selectedUnitId: 'old', installationMode: 'ceiling', layoutCode: 'B6',
  regulationPercent: 85, accessoryCodes: [], sound: { iso16032Enabled: false },
  waterCoilId: 0, waterCoilEnabled: false, waterCoilCustomized: false,
  electricPreheaterId: 0, electricPreheaterEnabled: false,
  electricPostheaterId: 0, electricPostheaterEnabled: false });
const resultFor = () => ({ layoutConfigurations: [{ code: 'B6', installationMode: 'ceiling', isDefault: true }],
  accessories: [], waterCoils: [], electricHeaters: [] });
const main = fs.readFileSync(new URL('../frontend/ssw-next/src/main.ts', import.meta.url), 'utf8');
const functions = main.slice(main.indexOf('const refreshPreselection = async'), main.indexOf('const saveDraft ='));
function harness(bridge, input = baseline()) {
  const context = vm.createContext({ structuredClone, normalizeSelection, bridge, input,
    renderShell() {}, logClientError() {}, lockWorkflowAtPreselection() {}, confirmInstallationReview() {} });
  vm.runInContext(compile(`let draft=input; let result={}; let data={units:[]};
    let preselectionRequestVersion=0, calculationRequestVersion=0;
    let calculating=false, calculationFailed=false, toastMessage='';
    let confirmedUnitId=null, installationReviewRequired=false;
    ${functions}
    globalThis.run=recalculate; globalThis.search=refreshPreselection;
    globalThis.change=(id)=>{draft.selectedUnitId=id};
    globalThis.state=()=>({draft,result,data,calculating,calculationFailed});`), context);
  return context;
}
const copy = (value) => JSON.parse(JSON.stringify(value));
const input = { ...baseline(), waterCoilEnabled: true, waterCoilId: 11,
  electricPreheaterEnabled: true, electricPreheaterId: 12,
  electricPostheaterEnabled: true, electricPostheaterId: 13 };
const reply = { ...resultFor(), waterCoils: [{ id: 22, mode: 'CWD', lengthMm: 500, heightMm: 300,
  rows: 2, circuits: 4, finSpacingMm: 2 }], electricHeaters: [{ id: 23, mode: 'PEHD', isDefault: true }] };
const normalized = normalizeSelection(input, reply);
assert.equal(normalized.waterCoilEnabled, false);
assert.equal(normalized.electricPreheaterEnabled, false);
assert.equal(normalized.electricPostheaterEnabled, false);
assert.equal(normalized.waterCoilId, 22);
assert.equal(input.waterCoilId, 11, 'normalization must not mutate caller');
assert.deepEqual(copy(normalizeSelection(normalized, reply)), copy(normalized), 'normalization must converge');
assert.equal(normalizeSelection(baseline(), { ...resultFor(), layoutConfigurations: [] }).layoutCode, '', 'no fabricated fallback');
const newLayout = normalizeSelection(baseline(), { ...resultFor(), layoutConfigurations: [
  { code: 'E2', installationMode: 'wall', isDefault: true, referenceView: 'OSC_EAST_WEST' }] });
assert.equal(newLayout.layoutCode, 'E2');
assert.equal(newLayout.installationMode, 'wall');

let calls = 0;
const coherent = harness({ calculate: async (draft) => {
  calls++; return { ...reply, calculatedWithWaterCoil: draft.waterCoilEnabled };
}}, input);
assert.equal(await coherent.run(), true);
assert.equal(calls, 2, 'must recalculate after resetting an incompatible treatment');
assert.equal(coherent.state().result.calculatedWithWaterCoil, false);
assert.equal(coherent.state().calculating, false);

const failure = harness({ calculate: async () => { throw Error('test failure'); } });
assert.equal(await failure.run(), false);
assert.equal(failure.state().calculating, false);
assert.equal(failure.state().calculationFailed, true);
const searchFailure = harness({ preselect: async () => { throw Error('search failure'); } });
await searchFailure.search();
assert.equal(searchFailure.state().calculating, false);
assert.equal(searchFailure.state().calculationFailed, true);

let release;
const race = harness({ calculate: (draft) => draft.selectedUnitId === 'old'
  ? new Promise((resolve) => { release = () => resolve({ ...resultFor(), model: 'old' }); })
  : Promise.resolve({ ...resultFor(), model: 'new' }) });
const pending = race.run();
race.change('new');
assert.equal(await race.run(), true);
release();
assert.equal(await pending, false);
assert.equal(race.state().result.model, 'new', 'outdated result must not overwrite the new model');
assert.equal(race.state().draft.selectedUnitId, 'new');

let firstSearch;
let searches = 0;
const searchRace = harness({
  preselect: () => ++searches === 1 ? new Promise((resolve) => { firstSearch = resolve; })
    : Promise.resolve([{ id: 'new', requiredRegulation: 85 }]),
  calculate: async () => resultFor(),
});
const oldSearch = searchRace.search();
await searchRace.search();
firstSearch([{ id: 'old', requiredRegulation: 85 }]);
await oldSearch;
assert.equal(searchRace.state().data.units[0].id, 'new');
assert.equal(searchRace.state().calculating, false);
console.log('Next selection regression: normalization, treatment consistency, failures and stale responses passed.');
