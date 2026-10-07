import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import ts from '../frontend/ssw-next/node_modules/typescript/lib/typescript.js';

const main = fs.readFileSync(new URL('../frontend/ssw-next/src/main.ts', import.meta.url), 'utf8');
const render = main.slice(main.indexOf('const renderDocumentsStep ='), main.indexOf('const renderSummaryStep ='));
const card = main.slice(main.indexOf('const documentCard ='), main.indexOf('const summaryMetric ='));
for (const language of ['en', 'it', 'fr', 'de']) {
  for (const available of [true, false]) {
    const context = vm.createContext({
      productDocuments: { applicationDocumentAvailable: available },
      productDocumentsLoading: false,
      dimensionalDrawing: null,
      languageCode: () => language,
      messages: () => ({ ui: { documents: {} } }),
      icon: () => '',
      escapeHtml: (value) => String(value ?? ''),
    });
    const compiled = ts.transpileModule(`${card}\n${render}\nglobalThis.html = renderDocumentsStep();`, {
      compilerOptions: { target: ts.ScriptTarget.ES2022 },
    }).outputText;
    vm.runInContext(compiled, context);
    const button = context.html.match(/<button\b[^>]*data-document="application-document"[^>]*>/)?.[0];
    assert.ok(button, 'Application card must carry its document action.');
    assert.equal(/\bdisabled\b/.test(button), !available, `${language}: incorrect availability`);
  }
}
assert.match(main, /\| "application-document"/);
const bridge = fs.readFileSync(new URL('../frontend/ssw-next/src/bridge/index.ts', import.meta.url), 'utf8');
assert.match(bridge, /"documents\.open", \{[\s\S]*?documentType,/);
console.log('Application document card: 8 availability/localization cases passed.');
