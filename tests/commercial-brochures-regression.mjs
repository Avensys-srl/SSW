import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import ts from '../frontend/ssw-next/node_modules/typescript/lib/typescript.js';

const main = fs.readFileSync(new URL('../frontend/ssw-next/src/main.ts', import.meta.url), 'utf8');
const labels = fs.readFileSync(new URL('../frontend/ssw-next/src/document-category-labels.ts', import.meta.url), 'utf8');
const render = main.slice(main.indexOf('const renderDocumentsStep ='), main.indexOf('const renderSummaryStep ='));
const card = main.slice(main.indexOf('const documentCard ='), main.indexOf('const summaryMetric ='));
const languages = ['bg','cs','da','de','en','fr','hu','is','it','nl','no','pl','ro','sl','sv'];
let count = 0;
for (const language of languages) {
  for (const brochures of [[], [{id:'SCHOOL_HCI',language:'FR'}], [{id:'GRAND_TERTIAIRE',language:'EN'},{id:'SECOND',language:'EN'}]]) {
    const context = vm.createContext({
      productDocuments: {brochures,commercialSheetAvailable:true}, productDocumentsLoading:false,
      dimensionalDrawing:null, languageCode:()=>language,
      messages:()=>({ui:{documents:{}}}),icon:()=>'',
      escapeHtml:value=>String(value??'').replaceAll('"','&quot;'),
    });
    const compiled = ts.transpileModule(`${labels.replace('export const','const')}\n${card}\n${render}\nglobalThis.html=renderDocumentsStep();globalThis.labels=documentCategoryLabels;`, {
      compilerOptions:{target:ts.ScriptTarget.ES2022},
    }).outputText;
    vm.runInContext(compiled, context);
    assert.ok(context.labels[language].sheet && context.labels[language].brochure);
    assert.notEqual(context.labels[language].sheet,context.labels[language].brochure);
    assert.ok(context.html.includes(context.labels[language].brochure));
    const buttons=[...context.html.matchAll(/<button\b[^>]*data-document="brochure"[^>]*>/g)].map(match=>match[0]);
    assert.equal(buttons.length,Math.max(1,brochures.length));
    for(let i=0;i<buttons.length;i++) {
      assert.equal(/\bdisabled\b/.test(buttons[i]),brochures.length===0);
      assert.ok(buttons[i].includes(`data-brochure-id="${brochures[i]?.id??''}"`));
    }
    count++;
  }
}
const ui=fs.readFileSync(new URL('../frontend/ssw-next/src/i18n/ui.ts',import.meta.url),'utf8');
assert.match(ui,/technicalSheet: \(documentCategoryLabels\[code\] \?\? documentCategoryLabels.en\).sheet/);
assert.match(main,/button.dataset.brochureId/);
console.log(`Brochure UI: ${count} localized empty/single/multiple states passed.`);
