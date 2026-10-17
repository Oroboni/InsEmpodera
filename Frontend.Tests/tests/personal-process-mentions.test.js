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

  function montar(atores) {
    document.body.innerHTML = `
      <textarea data-actor-mentions></textarea>
      <ul id="actor-mention-list" hidden></ul>
      <script type="application/json" id="actor-mention-data"></script>`;
    document.getElementById('actor-mention-data').textContent = JSON.stringify(atores);
    runPublicScript('personal-process-mentions.js');
    return document.querySelector('textarea');
  }
  function digitar(campo, texto) {
    campo.value = texto;
    campo.setSelectionRange(texto.length, texto.length);
    campo.dispatchEvent(new Event('input'));
  }
  const nomes = () => [...document.querySelectorAll('#actor-mention-list button')].map(b => b.textContent);

  it('encontra por letras em sequência e ignora acentos, priorizando o início do nome', () => {
    const campo = montar([{ id: 1, nome: 'Isadora' }, { id: 2, nome: 'José' }, { id: 3, nome: 'Maria José' }]);
    digitar(campo, '@isao');
    expect(nomes()).toEqual(['Isadora']);
    digitar(campo, '@jose');
    expect(nomes()).toEqual(['José', 'Maria José']);
  });

  it('nas letras em sequência, prioriza as mais próximas do início', () => {
    const campo = montar([{ id: 1, nome: 'Bia Santos' }, { id: 2, nome: 'isadora' }]);
    digitar(campo, '@isao');
    expect(nomes()).toEqual(['isadora', 'Bia Santos']);
  });

  it('mostra todos ao digitar só @ e avisa quando não há resultado', () => {
    const campo = montar([{ id: 1, nome: 'Bia' }, { id: 2, nome: 'Ana' }]);
    digitar(campo, '@');
    expect(nomes()).toEqual(['Ana', 'Bia']);
    digitar(campo, '@zzz');
    const lista = document.getElementById('actor-mention-list');
    expect(lista.hidden).toBe(false);
    expect(nomes()).toEqual([]);
    expect(lista.querySelector('.actor-mention-empty').textContent).toContain('Nenhum ator encontrado');
  });

  it('navega com as setas e escolhe com Enter', () => {
    const campo = montar([{ id: 1, nome: 'Ana' }, { id: 2, nome: 'Bia' }]);
    digitar(campo, 'Oi @');
    campo.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true }));
    campo.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    expect(campo.value).toBe('Oi @[Bia](ator:2) ');
    expect(document.getElementById('actor-mention-list').hidden).toBe(true);
  });
});
