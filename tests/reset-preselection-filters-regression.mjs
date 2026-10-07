import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import ts from '../frontend/ssw-next/node_modules/typescript/lib/typescript.js';

const source = fs.readFileSync(new URL('../frontend/ssw-next/src/reset-preselection-filters.ts', import.meta.url), 'utf8');
const labels = fs.readFileSync(new URL('../frontend/ssw-next/src/unit-classification-labels.ts', import.meta.url), 'utf8');
const context = vm.createContext({});
const compiled = ts.transpileModule(source.replace(/^import type.*\n/u, '').replace('export const', 'const') + '\n' + labels.replace('export const', 'const') + '\nglobalThis.resetFilters=resetPreselectionFilters;globalThis.labels=classificationLabels;', {compilerOptions:{target:ts.ScriptTarget.ES2022}}).outputText;
vm.runInContext(compiled, context);
const original = {maximumSfpEnabled:true,maximumSfp:2,supplyNoiseEnabled:true,maximumSupplyNoiseDbA:50,breakoutNoiseEnabled:true,maximumBreakoutNoiseDbA:45,rotaryOnlyEnabled:true,recoveryCategory:'rotary',recoveryOperation:'Rotary',exchangerType:'EN',unitApplication:'Centralized',installationEnvironment:'Outdoor'};
const reset = context.resetFilters(original);
for (const key of ['maximumSfpEnabled','supplyNoiseEnabled','breakoutNoiseEnabled','rotaryOnlyEnabled']) assert.equal(reset[key],false);
for (const key of ['recoveryCategory','recoveryOperation','exchangerType','unitApplication','installationEnvironment']) assert.equal(reset[key],'any');
for (const key of ['maximumSfp','maximumSupplyNoiseDbA','maximumBreakoutNoiseDbA']) assert.equal(reset[key],original[key]);
assert.equal(original.exchangerType,'EN');
assert.deepEqual(context.resetFilters(reset),reset);
for (const language of ['bg','cs','da','de','en','fr','hu','is','it','nl','no','pl','ro','sl','sv']) {
  const localized = context.labels(language);
  assert.equal(localized.titles.length,4);
  assert.equal(localized.environment.length,3);
  assert.ok(localized.reset && localized.titles.every(Boolean) && localized.environment.every(Boolean));
}
console.log('PASS: all seven criteria reset, legacy flags cleared, thresholds preserved; labels in 15 languages.');
