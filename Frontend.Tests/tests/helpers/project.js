import { readFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const currentDirectory = dirname(fileURLToPath(import.meta.url));
export const projectRoot = resolve(currentDirectory, '..', '..', '..');

export function readProjectFile(relativePath) {
  return readFileSync(join(projectRoot, relativePath), 'utf8');
}

export function runPublicScript(relativePath) {
  const scriptPath = join(projectRoot, 'wwwroot', 'js', relativePath);
  const source = readFileSync(scriptPath, 'utf8');
  return window.eval(`${source}\n//# sourceURL=${pathToFileURL(scriptPath).href}`);
}

export function extractInlineScripts(viewPath) {
  const source = readProjectFile(viewPath);
  return [...source.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/gi)]
    .map(match => match[1])
    .filter(script => script.trim());
}

export function runInlineScript(viewPath, { replacements = {} } = {}) {
  let source = extractInlineScripts(viewPath).join('\n');
  for (const [search, replacement] of Object.entries(replacements)) {
    source = source.replaceAll(search, replacement);
  }
  return window.eval(`${source}\n//# sourceURL=${viewPath}`);
}

export function dispatchReady() {
  document.dispatchEvent(new Event('DOMContentLoaded', { bubbles: true }));
}

export async function flushPromises() {
  for (let turn = 0; turn < 10; turn += 1) {
    await Promise.resolve();
  }
}
