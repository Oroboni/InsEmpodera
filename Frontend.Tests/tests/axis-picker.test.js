import { beforeEach, describe, expect, it } from 'vitest';
import { runPublicScript } from './helpers/project.js';

describe('seletor de eixos do diário de campo', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div data-axis-picker>
        <div data-axis-tags></div>
        <details><summary>Selecione os eixos</summary><div class="multiselect-panel"><ul>
          <li><input id="axis-1" name="EixosSelecionados" type="checkbox" value="1" data-tag-name="Saúde"><label for="axis-1">Saúde</label></li>
          <li><input id="axis-2" name="EixosSelecionados" type="checkbox" value="2" data-tag-name="Educação" checked><label for="axis-2">Educação</label></li>
        </ul></div></details>
      </div>`;
    runPublicScript('axis-picker.js');
  });

  it('mostra eixo já selecionado e remove com o botão da tag', () => {
    const tags = document.querySelector('[data-axis-tags]');
    expect(tags.textContent).toContain('Educação');
    tags.querySelector('button').click();
    expect(document.getElementById('axis-2').checked).toBe(false);
    expect(tags.textContent).toBe('');
  });

  it('atualiza seleção e não interpreta nomes de eixos como HTML', () => {
    const axis = document.getElementById('axis-1');
    axis.dataset.tagName = '<img src=x onerror=alert(1)>';
    axis.checked = true;
    axis.dispatchEvent(new Event('change', { bubbles: true }));
    const tags = document.querySelector('[data-axis-tags]');
    expect(tags.textContent).toContain('<img');
    expect(tags.querySelector('img')).toBeNull();
  });
});
