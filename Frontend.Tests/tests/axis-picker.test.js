import { beforeEach, describe, expect, it } from 'vitest';
import { readProjectFile, runPublicScript } from './helpers/project.js';

const axisViews = [
  'Views/Atividades/Create.cshtml',
  'Views/Atividades/Edit.cshtml',
  'Views/Comunidade/Create_Atividades.cshtml',
  'Views/Comunidade/Edit_Atividades.cshtml',
  'Views/Comunidade/Create_Recursos.cshtml',
  'Views/Comunidade/ComunidadeDetalhesRecursos.cshtml',
  'Views/PersonalProcess/Create.cshtml',
  'Views/PersonalProcess/Edit.cshtml',
  'Views/Diariocampo/create.cshtml',
  'Views/Diariocampo/edit.cshtml'
];

describe('contrato do seletor de eixos nas telas', () => {
  it.each(axisViews)('%s usa o componente compartilhado para todos os campos de eixos', view => {
    const source = readProjectFile(view);
    const pickers = [...source.matchAll(/<[^>]+\bdata-axis-picker(?=[\s>])/g)];
    const tags = [...source.matchAll(/<[^>]+\bdata-axis-tags(?=[\s>])/g)];
    expect(pickers.length).toBeGreaterThan(0);
    expect(tags).toHaveLength(pickers.length);
    expect(source).toContain('~/js/axis-picker.js');
    expect(source).not.toContain('id="tag-container"');
  });
});

describe('seletor compartilhado de eixos', () => {
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

  it('seleciona ao clicar no espaço livre da linha, não apenas no quadrado', () => {
    const row = document.getElementById('axis-1').closest('li');
    row.click();
    expect(document.getElementById('axis-1').checked).toBe(true);
    expect(document.querySelector('[data-axis-tags]').textContent).toContain('Saúde');
  });

  it('não altera uma opção desabilitada ao clicar na linha', () => {
    const axis = document.getElementById('axis-1');
    axis.disabled = true;
    axis.closest('li').click();
    expect(axis.checked).toBe(false);
  });
});
