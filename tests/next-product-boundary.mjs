import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = fileURLToPath(new URL('../', import.meta.url));
const files = ['SSW/CLProgram.cs', 'SSW/CLNextHostForm.cs'];
for (const name of readdirSync(path.join(root, 'SSWLib'))) {
  if (/^CLNextUi.*\.vb$/.test(name)) files.push(`SSWLib/${name}`);
}
for (const file of files) {
  const source = readFileSync(path.join(root, file), 'utf8');
  assert.doesNotMatch(source, /\bCLMainForm\b|\bOpenLegacy\b|["']legacy\.open["']/,
    `Legacy UI dependency reintroduced in ${file}`);
}
const project = readFileSync(path.join(root, 'SSW/SSW.csproj'), 'utf8');
assert.match(project, /EnforceSupportedSSWProductLine/);
const serializer = readFileSync(path.join(root, 'SSWLib/CLSelectionProjectSerializer.vb'), 'utf8');
assert.match(serializer, /CLSelectionMigrationRunner\.Run/);
assert.match(serializer, /CreateMigrationBackupIfRequired/);
console.log('Next product boundary passed: no legacy UI entry/dependencies; historical-file migration and backup retained.');
