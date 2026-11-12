import { readFileSync, writeFileSync } from 'node:fs';

const source = JSON.parse(readFileSync('src/locale/messages.json', 'utf8')).translations;

for (const locale of ['en', 'ru']) {
  const file = `src/locale/messages.${locale}.json`;
  let current = {};
  try {
    current = JSON.parse(readFileSync(file, 'utf8')).translations;
  } catch {
    // First run: the file doesn't exist yet.
  }

  const translations = {};
  const missing = [];
  for (const id of Object.keys(source)) {
    if (id in current) translations[id] = current[id];
    else missing.push(id);
  }

  writeFileSync(file, JSON.stringify({ locale, translations }, null, 2) + '\n');
  console.log(`${locale}: ${Object.keys(translations).length} translated, ${missing.length} missing`);
  for (const id of missing) console.log(`  ${id}: ${source[id]}`);
}
