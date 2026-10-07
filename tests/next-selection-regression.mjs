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
  minimumRegulationPercent: 70, regulationPercent: 85, accessoryCodes: [], sound: { iso16032Enabled: false },
  waterCoilId: 0, waterCoilEnabled: false, waterCoilCustomized: false,
  electricPreheaterId: 0, electricPreheaterEnabled: false,
  electricPostheaterId: 0, electricPostheaterEnabled: false });
const resultFor = () => ({ layoutConfigurations: [{ code: 'B6', installationMode: 'ceiling', isDefault: true }],
  accessories: [], waterCoils: [], electricHeaters: [] });
const main = fs.readFileSync(new URL('../frontend/ssw-next/src/main.ts', import.meta.url), 'utf8');
assert.match(main, /data-action="offer-provisional"/);
assert.match(main, /data-action="offer-definitive"/);
assert.match(main, /data-offer-reminder/);
const offerRules = main.slice(main.indexOf('const offerReminderToDays'), main.indexOf('const offerText ='));
const offerRuleContext = vm.createContext({});
vm.runInContext(compile(`${offerRules}\nglobalThis.toDays = offerReminderToDays;`), offerRuleContext);
assert.equal(offerRuleContext.toDays(7, 'days'), 7);
assert.equal(offerRuleContext.toDays(3, 'months'), 90, 'months use the API-supported 30-day interval');
const offerLocaleStart = main.indexOf('const offerTexts:');
const offerLocaleEnd = main.indexOf('const offerReminderToDays');
const offerLocaleContext = vm.createContext({});
vm.runInContext(compile(`${main.slice(offerLocaleStart, offerLocaleEnd)}\nglobalThis.copy = offerTexts;`), offerLocaleContext);
for (const language of ['bg', 'cs', 'da', 'de', 'en', 'fr', 'hu', 'is', 'it', 'nl', 'no', 'pl', 'ro', 'sl', 'sv']) {
  assert.ok(offerLocaleContext.copy[language]?.provisional, `provisional offer label is localized for ${language}`);
  assert.ok(offerLocaleContext.copy[language]?.definitive, `definitive offer label is localized for ${language}`);
}
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
    globalThis.state=()=>({draft,result,data,calculating,calculationFailed,installationReviewRequired});`), context);
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

const changedLayout = harness({ calculate: async () => ({ ...resultFor(), layoutConfigurations: [
  { code: 'E2', installationMode: 'wall', isDefault: true }] }) });
assert.equal(await changedLayout.run(), true);
assert.equal(changedLayout.state().installationReviewRequired, true, 'automatic layout changes require explicit review');

const navigation = vm.createContext({});
const navigationRules = main.slice(main.indexOf('const stepIndex ='), main.indexOf('const resetOptionalStepVisits ='));
vm.runInContext(compile(`const steps=['project','preselection','installation','accessories','summary'].map(id=>({id}));
  let maximumReachableStepIndex=4, installationReviewRequired=true, calculationFailed=false;
  let result={status:'invalid'}, currentStep='accessories';
  ${navigationRules}
  globalThis.allowed=canNavigateToStep;
  globalThis.confirm=()=>{installationReviewRequired=false};
  globalThis.validation=(status)=>{result={status}};
  globalThis.special=()=>{result={status:'warning',requiresAvensysSelection:true}};`), navigation);
navigation.confirm();
assert.equal(navigation.allowed('installation'), true, 'blocking issue must still allow returning to earlier steps');
assert.equal(navigation.allowed('accessories'), true, 'current step remains accessible while correcting a blocking issue');
assert.equal(navigation.allowed('summary'), false, 'blocking validation must prevent advancing');
navigation.validation('warning');
assert.equal(navigation.allowed('summary'), true, 'non-blocking warnings must not stop workflow');
navigation.validation('valid');
assert.equal(navigation.allowed('summary'), true);
navigation.special();
navigation.currentStep = 'installation';
assert.equal(navigation.allowed('preselection'), true, 'special catalog units remain accessible for changing the selection');
assert.equal(navigation.allowed('installation'), true, 'special units can be inspected without a technical layout');
assert.equal(navigation.allowed('accessories'), false, 'special units must stop before technical calculations');

let calls = 0;
const coherent = harness({ calculate: async (draft) => {
  calls++; return { ...reply, calculatedWithWaterCoil: draft.waterCoilEnabled };
}}, input);
assert.equal(await coherent.run(), true);
assert.equal(calls, 2, 'must recalculate after resetting an incompatible treatment');
assert.equal(coherent.state().result.calculatedWithWaterCoil, false);
assert.equal(coherent.state().calculating, false);

let coilCalls = 0;
const coilInput = { ...baseline(), regulationPercent: 85.8, waterCoilEnabled: true, waterCoilId: 22 };
const coilLoop = harness({ calculate: async (draft) => {
  coilCalls++;
  return { ...reply, effectiveRegulationPercent: Math.max(90.1, draft.regulationPercent),
    calculatedRegulation: draft.regulationPercent, waterCoilResults: [{ mode: 'CWD' }] };
}}, coilInput);
assert.equal(await coilLoop.run(), true, 'coil pressure compensation must converge');
assert.equal(coilCalls, 2, 'calculate once more using the effective fractional regulation');
assert.equal(coilLoop.state().draft.regulationPercent, 90.1);
assert.equal(coilLoop.state().draft.minimumRegulationPercent, 70, 'coil must not change the search minimum');
assert.equal(coilLoop.state().draft.waterCoilEnabled, true);
assert.equal(coilLoop.state().result.calculatedRegulation, 90.1);
assert.equal(await coilLoop.run(), true);
assert.equal(coilCalls, 3, 'unchanged coil selection must stay stable on recalculation');

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
assert.equal(searchRace.state().draft.minimumRegulationPercent, 70, 'search must preserve the user threshold');
assert.equal(searchRace.state().draft.regulationPercent, 85, 'calculation must use the selected unit regulation');
assert.equal(searchRace.state().calculating, false);
console.log('Next selection regression: normalization, treatment consistency, failures and stale responses passed.');
