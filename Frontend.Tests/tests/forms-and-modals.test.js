import { beforeEach, describe, expect, it, vi } from 'vitest';
import { dispatchReady, runInlineScript, runPublicScript } from './helpers/project.js';

function wizardFixture(steps = 3) {
  document.body.innerHTML = `
    <div class="stepper">${Array.from({ length: steps }, () => '<span class="step"></span>').join('')}</div>
    ${Array.from({ length: steps }, (_, i) => `<section class="step-content" data-index="${i + 1}"></section>`).join('')}
    <button id="btn-prev"></button>
    <button id="btn-next"></button>
    <button id="btn-save"></button>
  `;
}

describe('assistente de ficha de primeiro contato', () => {
  beforeEach(() => wizardFixture());

  it('inicia no primeiro passo com estado e botões coerentes', () => {
    runPublicScript('ficha/wizard.js');
    dispatchReady();

    expect(window.currentStep).toBe(1);
    expect([...document.querySelectorAll('.step-content')].map(x => x.classList.contains('active')))
      .toEqual([true, false, false]);
    expect([...document.querySelectorAll('.step')].map(x => x.classList.contains('active')))
      .toEqual([true, false, false]);
    expect(document.getElementById('btn-prev').textContent).toContain('Sair da ficha');
    expect(document.getElementById('btn-next').style.display).toBe('inline-flex');
    expect(document.getElementById('btn-save').style.display).toBe('none');
  });

  it('navega, atualiza progresso e mostra salvar somente no último passo', () => {
    runPublicScript('ficha/wizard.js');
    dispatchReady();

    window.changeStep(1);
    expect(window.currentStep).toBe(2);
    expect(document.querySelector('[data-index="2"]').classList.contains('active')).toBe(true);
    expect(document.getElementById('btn-prev').textContent).toContain('Voltar');

    document.getElementById('btn-next').click();
    expect(window.currentStep).toBe(3);
    expect(document.getElementById('btn-next').style.display).toBe('none');
    expect(document.getElementById('btn-save').style.display).toBe('inline-flex');

    document.getElementById('btn-prev').click();
    expect(window.currentStep).toBe(2);
  });

  it('impede navegação para fora dos limites', () => {
    runPublicScript('ficha/wizard.js');
    dispatchReady();

    window.changeStep(-1);
    expect(window.currentStep).toBe(1);
    window.changeStep(99);
    expect(window.currentStep).toBe(1);
  });

  it('usa o histórico ao sair pelo primeiro passo', () => {
    const back = vi.spyOn(window.history, 'back').mockImplementation(() => {});
    runPublicScript('ficha/wizard.js');
    dispatchReady();
    document.getElementById('btn-prev').click();
    expect(back).toHaveBeenCalledOnce();
  });
});

describe('modo de visualização e edição reutilizável', () => {
  it('bloqueia campos, habilita edição e submete no segundo clique', () => {
    document.body.innerHTML = `
      <form class="main-form" data-mode="edit">
        <input name="nome"><select name="tipo"><option>A</option></select><textarea></textarea>
        <button type="button" id="edit-save-btn"></button>
      </form>
    `;
    const form = document.querySelector('form');
    const submit = vi.spyOn(form, 'submit').mockImplementation(() => {});
    runPublicScript('ficha/edit-mode.js');
    dispatchReady();

    const fields = [...form.querySelectorAll('input, select, textarea')];
    expect(fields.every(field => field.disabled)).toBe(true);
    expect(document.getElementById('edit-save-btn').textContent).toContain('Editar');

    document.getElementById('edit-save-btn').click();
    expect(fields.every(field => !field.disabled)).toBe(true);
    expect(document.getElementById('edit-save-btn').textContent).toContain('Salvar');
    expect(submit).not.toHaveBeenCalled();

    document.getElementById('edit-save-btn').click();
    expect(submit).toHaveBeenCalledOnce();
  });

  it('não altera formulários de criação', () => {
    document.body.innerHTML = `
      <form class="main-form" data-mode="create">
        <input><button id="edit-save-btn"></button>
      </form>
    `;
    runPublicScript('ficha/edit-mode.js');
    dispatchReady();
    expect(document.querySelector('input').disabled).toBe(false);
    expect(document.getElementById('edit-save-btn').textContent).toBe('');
  });
});

describe('modal de exclusão reutilizável', () => {
  it('abre e cancela o modal em modo de edição', () => {
    document.body.innerHTML = `
      <form class="main-form" data-mode="edit">
        <button type="button" id="openDeleteModalBtn"></button>
        <div id="deleteConfirmationModal"><button type="button" id="cancelDeleteBtn"></button></div>
      </form>
    `;
    runPublicScript('ficha/delete-modal.js');
    dispatchReady();

    const modal = document.getElementById('deleteConfirmationModal');
    document.getElementById('openDeleteModalBtn').click();
    expect(modal.classList.contains('active')).toBe(true);
    document.getElementById('cancelDeleteBtn').click();
    expect(modal.classList.contains('active')).toBe(false);
  });

  it('fica inerte fora do modo de edição', () => {
    document.body.innerHTML = `
      <form class="main-form" data-mode="create">
        <button type="button" id="openDeleteModalBtn"></button>
        <div id="deleteConfirmationModal"></div>
      </form>
    `;
    runPublicScript('ficha/delete-modal.js');
    dispatchReady();
    document.getElementById('openDeleteModalBtn').click();
    expect(document.getElementById('deleteConfirmationModal').className).toBe('');
  });
});

describe('avaliação pessoal', () => {
  it('reflete o valor inicial e cada alteração visual do slider', () => {
    document.body.innerHTML = '<input class="metric-slider" type="range" min="1" max="5" value="3">';
    runPublicScript('personal-assessment.js');
    dispatchReady();
    const slider = document.querySelector('input');
    expect(slider.dataset.level).toBe('3');
    expect(slider.style.backgroundSize).toBe('50% 100%');

    slider.value = '5';
    slider.dispatchEvent(new Event('input'));
    expect(slider.dataset.level).toBe('5');
    expect(slider.style.backgroundSize).toBe('100% 100%');
  });

  it('abre e fecha os metadados sem submeter o formulário', () => {
    document.body.innerHTML = '<form><details class="form-footer"><summary>Metadados</summary></details></form>';
    runPublicScript('personal-assessment.js');
    dispatchReady();
    const details = document.querySelector('details');
    const summary = document.querySelector('summary');

    const open = new MouseEvent('click', { bubbles: true, cancelable: true });
    summary.dispatchEvent(open);
    expect(open.defaultPrevented).toBe(true);
    expect(details.open).toBe(true);

    summary.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
    expect(details.open).toBe(false);
  });
});

describe('relatórios', () => {
  it('instancia somente os gráficos presentes e mantém suas configurações críticas', () => {
    document.body.innerHTML = `
      <canvas id="firstContactPieChart"></canvas>
      <canvas id="networkBarChart"></canvas>
    `;
    const chart = vi.fn();
    window.Chart = chart;
    runPublicScript('reports.js');
    dispatchReady();

    expect(chart).toHaveBeenCalledTimes(2);
    expect(chart.mock.calls[0][1].type).toBe('pie');
    expect(chart.mock.calls[0][1].data.labels).toHaveLength(3);
    expect(chart.mock.calls[1][1].type).toBe('bar');
    expect(chart.mock.calls[1][1].options.indexAxis).toBe('y');
    expect(chart.mock.calls[1][1].options.scales.x.stacked).toBe(true);
  });

  it('alterna tabelas numéricas e percentuais e sincroniza o botão ativo', () => {
    document.body.innerHTML = `
      <button id="btn-numbers" class="active"></button>
      <button id="btn-percent"></button>
      <div id="table-numbers"></div>
      <div id="table-percentages" style="display:none"></div>
    `;
    window.Chart = vi.fn();
    runPublicScript('reports.js');
    dispatchReady();

    document.getElementById('btn-percent').click();
    expect(document.getElementById('table-numbers').style.display).toBe('none');
    expect(document.getElementById('table-percentages').style.display).toBe('');
    expect(document.getElementById('btn-percent').classList.contains('active')).toBe(true);

    document.getElementById('btn-numbers').click();
    expect(document.getElementById('table-numbers').style.display).toBe('');
    expect(document.getElementById('table-percentages').style.display).toBe('none');
    expect(document.getElementById('btn-numbers').classList.contains('active')).toBe(true);
  });
});
describe('tags de eixos em recursos da comunidade', () => {
  it('trata o nome vindo do banco como texto e permite remover a seleção', () => {
    const attack = '<img src=x onerror="window.__resourceTagXss=1">';
    document.body.innerHTML = `
      <div id="tag-container"></div>
      <div id="multiselect-panel"><ul><li>
        <input type="checkbox" id="eixo-9" data-tag-class="tag-pink">
      </li></ul></div>`;
    const checkbox = document.getElementById('eixo-9');
    checkbox.dataset.tagName = attack;
    runInlineScript('Views/Comunidade/Create_Recursos.cshtml');
    dispatchReady();

    checkbox.checked = true;
    document.getElementById('multiselect-panel').dispatchEvent(new Event('change', { bubbles: true }));
    const tag = document.querySelector('#tag-container .tag-item');
    expect(tag.textContent).toContain(attack);
    expect(tag.querySelector('img')).toBeNull();
    expect(window.__resourceTagXss).toBeUndefined();

    tag.querySelector('.tag-remove-btn').click();
    expect(checkbox.checked).toBe(false);
    expect(document.querySelector('#tag-container .tag-item')).toBeNull();
  });
});
