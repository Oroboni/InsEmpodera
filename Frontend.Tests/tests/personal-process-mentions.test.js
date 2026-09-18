import { describe, expect, it } from 'vitest';
import { runPublicScript } from './helpers/project.js';

describe('menções no diário de processo pessoal', () => {
  it('sugere atores com espaços no nome e insere referência estável', () => {
    document.body.innerHTML = `
      <textarea data-actor-mentions></textarea>
      <ul id="actor-mention-list" hidden></ul>
      <script type="application/json" id="actor-mention-data">[{"id":42,"nome":"Maria Silva"}]</script>`;
    runPublicScript('personal-process-mentions.js');
    const campo = document.querySelector('textarea');
    campo.value = 'Conversei com @Maria S';
    campo.setSelectionRange(campo.value.length, campo.value.length);
    campo.dispatchEvent(new Event('input'));
    const lista = document.getElementById('actor-mention-list');
    expect(lista.hidden).toBe(false);
    lista.querySelector('button').click();
    expect(campo.value).toBe('Conversei com @[Maria Silva](ator:42) ');
    expect(lista.hidden).toBe(true);
  });

  it('mostra nomes hostis como texto e não como HTML', () => {
    document.body.innerHTML = `
      <textarea data-actor-mentions></textarea>
      <ul id="actor-mention-list" hidden></ul>
      <script type="application/json" id="actor-mention-data"></script>`;
    document.getElementById('actor-mention-data').textContent = JSON.stringify([
      { id: 8, nome: '<img src=x onerror="window.actorXss=1">' }
    ]);
    runPublicScript('personal-process-mentions.js');
    const campo = document.querySelector('textarea');
    campo.value = '@';
    campo.setSelectionRange(1, 1);
    campo.dispatchEvent(new Event('input'));
    expect(document.querySelector('#actor-mention-list button').textContent).toContain('<img');
    expect(document.querySelector('#actor-mention-list img')).toBeNull();
    expect(window.actorXss).toBeUndefined();
  });
});
