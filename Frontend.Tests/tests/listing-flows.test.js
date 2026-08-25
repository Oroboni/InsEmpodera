import { describe, expect, it, vi } from 'vitest';
import { dispatchReady, runInlineScript } from './helpers/project.js';

function visible(elements) {
  return [...elements].filter(item => item.style.display !== 'none');
}

describe.each([
  {
    name: 'atores', view: 'Views/Atores/Index.cshtml', input: 'searchInput', grid: 'atoresGrid', item: 'ator-item',
    values: ['ana silva', 'bruno costa']
  },
  {
    name: 'comunidades', view: 'Views/Comunidade/Index.cshtml', input: 'searchInput', grid: 'comunidadesGrid', item: 'comunidade-item',
    values: ['centro', 'praia']
  },
  {
    name: 'perfis de acesso', view: 'Views/AccessProfile/Index.cshtml', input: 'searchProfilesInput', grid: 'profilesGrid', item: 'profile-item',
    values: ['administrador', 'consulta']
  }
])('busca da lista de $name', ({ view, input, grid, item, values }) => {
  function arrange() {
    document.body.innerHTML = `
      <input id="${input}">
      <div id="${grid}">
        <div class="${item}" data-nome="${values[0]}"></div>
        <div class="${item}" data-nome="${values[1]}"></div>
      </div>
      <div id="noResultsMessage" style="display:none"></div>
    `;
    runInlineScript(view);
    dispatchReady();
  }

  it('filtra sem diferenciar maiúsculas e espaços laterais', () => {
    arrange();
    const search = document.getElementById(input);
    search.value = `  ${values[0].toUpperCase()}  `;
    search.dispatchEvent(new Event('input'));

    expect(visible(document.querySelectorAll(`.${item}`))).toHaveLength(1);
    expect(visible(document.querySelectorAll(`.${item}`))[0].dataset.nome).toBe(values[0]);
    expect(document.getElementById(grid).style.display).toBe('flex');
  });

  it('mostra estado vazio e Escape restaura a lista', () => {
    arrange();
    const search = document.getElementById(input);
    search.value = 'não existe';
    search.dispatchEvent(new Event('input'));
    expect(document.getElementById(grid).style.display).toBe('none');
    expect(document.getElementById('noResultsMessage').style.display).toBe('block');

    search.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(search.value).toBe('');
    expect(visible(document.querySelectorAll(`.${item}`))).toHaveLength(2);
    expect(document.getElementById('noResultsMessage').style.display).toBe('none');
  });
});

describe('listagem de atividades', () => {
  function arrange() {
    document.body.innerHTML = `
      <input id="searchActivitiesInput">
      <button class="filter-eixo-btn active" data-eixo="todos">Todos</button>
      <button class="filter-eixo-btn" data-eixo="saude">Saúde</button>
      <button class="filter-eixo-btn" data-eixo="educacao">Educação</button>
      <div id="activitiesGrid">
        <div class="activity-item" data-nome="oficina de saúde" data-eixos="saude prevencao"></div>
        <div class="activity-item" data-nome="reforço escolar" data-eixos="educacao"></div>
      </div>
      <div id="noResultsMessage" style="display:none"></div>
    `;
    runInlineScript('Views/Atividades/Index.cshtml');
    dispatchReady();
  }

  it('combina busca textual e filtro de eixo', () => {
    arrange();
    document.querySelector('[data-eixo="saude"]').click();
    expect(visible(document.querySelectorAll('.activity-item'))).toHaveLength(1);
    expect(document.querySelector('[data-eixo="saude"]').classList.contains('active')).toBe(true);

    const search = document.getElementById('searchActivitiesInput');
    search.value = 'escolar';
    search.dispatchEvent(new Event('input'));
    expect(visible(document.querySelectorAll('.activity-item'))).toHaveLength(0);
    expect(document.getElementById('noResultsMessage').style.display).toBe('block');

    document.querySelector('[data-eixo="todos"]').click();
    expect(visible(document.querySelectorAll('.activity-item'))).toHaveLength(1);
  });

  it('Escape limpa somente a busca e preserva o filtro escolhido', () => {
    arrange();
    document.querySelector('[data-eixo="educacao"]').click();
    const search = document.getElementById('searchActivitiesInput');
    search.value = 'inexistente';
    search.dispatchEvent(new Event('input'));
    search.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(search.value).toBe('');
    expect(visible(document.querySelectorAll('.activity-item'))).toHaveLength(1);
    expect(visible(document.querySelectorAll('.activity-item'))[0].dataset.eixos).toBe('educacao');
  });
});

describe('listagem de usuários', () => {
  function arrange() {
    document.body.innerHTML = `
      <input id="searchUsersInput">
      <button class="filter-eixo-btn active" data-status="todos"></button>
      <button class="filter-eixo-btn" data-status="ativos"></button>
      <button class="filter-eixo-btn" data-status="inativos"></button>
      <div id="usersGrid">
        <div class="user-item" data-nome="ana silva" data-email="ana@example.org" data-ativo="true"></div>
        <div class="user-item" data-nome="bruno costa" data-email="bruno@example.org" data-ativo="false"></div>
      </div>
      <div id="noResultsMessage" style="display:none"></div>
    `;
    runInlineScript('Views/Users/index.cshtml');
    dispatchReady();
  }

  it('busca tanto por nome quanto por e-mail', () => {
    arrange();
    const search = document.getElementById('searchUsersInput');
    search.value = 'BRUNO@EXAMPLE.ORG';
    search.dispatchEvent(new Event('input'));
    expect(visible(document.querySelectorAll('.user-item'))).toHaveLength(1);
    expect(visible(document.querySelectorAll('.user-item'))[0].dataset.nome).toBe('bruno costa');
  });

  it('combina status, busca e estado vazio', () => {
    arrange();
    document.querySelector('[data-status="ativos"]').click();
    expect(visible(document.querySelectorAll('.user-item'))).toHaveLength(1);

    const search = document.getElementById('searchUsersInput');
    search.value = 'bruno';
    search.dispatchEvent(new Event('input'));
    expect(document.getElementById('usersGrid').style.display).toBe('none');
    expect(document.getElementById('noResultsMessage').style.display).toBe('block');

    document.querySelector('[data-status="inativos"]').click();
    expect(visible(document.querySelectorAll('.user-item'))).toHaveLength(1);
    expect(document.getElementById('usersGrid').style.display).toBe('flex');
  });
});

describe('fluxos da lista de fichas de primeiro contato', () => {
  function arrange() {
    document.body.innerHTML = `
      <input id="searchInput">
      <button class="filter-eixo-btn active" data-status="todos"></button>
      <button class="filter-eixo-btn" data-status="EmProgresso"></button>
      <button class="filter-eixo-btn" data-status="Concluida"></button>
      <button class="filter-comunidade-btn active" data-comunidade="todas"></button>
      <button class="filter-comunidade-btn" data-comunidade="10"></button>
      <button class="filter-comunidade-btn" data-comunidade="20"></button>
      <div id="fichasContainer">
        <div class="ficha-item" data-nome="ana" data-status="EmProgresso" data-comunidade="10"></div>
        <div class="ficha-item" data-nome="beatriz" data-status="Concluida" data-comunidade="10"></div>
        <div class="ficha-item" data-nome="carla" data-status="Abandonada" data-comunidade="20"></div>
      </div>
      <div id="noResultsMessage" style="display:none"></div>
      <span id="totalFichas">3</span><span id="emProgressoCount">1</span>
      <span id="concluidasCount">1</span><span id="abandonadasCount">1</span>
      <button class="btn-concluir" data-id="123"></button>
      <button class="btn-abandonar" data-id="456"></button>
      <div id="modalConcluir" style="display:none"><form id="formConcluir"><button class="btn-confirm">Confirmar</button><button type="button" class="btn-cancel"></button></form></div>
      <div id="modalAbandonar" style="display:none"><form id="formAbandonar"><button class="btn-confirm">Confirmar</button><button type="button" class="btn-cancel"></button></form></div>
    `;
    runInlineScript('Views/FichaPrimeiroContato/Index.cshtml', {
      replacements: {
        '@Url.Action("Concluir")': '/FichaPrimeiroContato/Concluir',
        '@Url.Action("Abandonar")': '/FichaPrimeiroContato/Abandonar'
      }
    });
    dispatchReady();
  }

  it('combina nome, status e comunidade e recalcula todos os contadores', () => {
    arrange();
    document.querySelector('[data-status="EmProgresso"]').click();
    document.querySelector('[data-comunidade="10"]').click();
    expect(visible(document.querySelectorAll('.ficha-item'))).toHaveLength(1);
    expect(document.getElementById('totalFichas').textContent).toBe('1');
    expect(document.getElementById('emProgressoCount').textContent).toBe('1');
    expect(document.getElementById('concluidasCount').textContent).toBe('0');
    expect(document.getElementById('abandonadasCount').textContent).toBe('0');

    document.getElementById('searchInput').value = 'beatriz';
    document.getElementById('searchInput').dispatchEvent(new Event('input'));
    expect(document.getElementById('fichasContainer').style.display).toBe('none');
    expect(document.getElementById('noResultsMessage').style.display).toBe('block');
    expect(document.getElementById('totalFichas').textContent).toBe('0');
  });

  it('abre os modais com a action correspondente ao registro', () => {
    vi.useFakeTimers();
    arrange();
    document.querySelector('.btn-concluir').click();
    expect(new URL(document.getElementById('formConcluir').action).pathname)
      .toBe('/FichaPrimeiroContato/Concluir/123');
    expect(document.getElementById('modalConcluir').style.display).toBe('flex');
    vi.advanceTimersByTime(10);
    expect(document.getElementById('modalConcluir').classList.contains('active')).toBe(true);

    document.querySelector('.btn-abandonar').click();
    expect(new URL(document.getElementById('formAbandonar').action).pathname)
      .toBe('/FichaPrimeiroContato/Abandonar/456');
  });

  it('fecha ambos os modais por cancelar, fundo e Escape', () => {
    vi.useFakeTimers();
    arrange();
    const concluir = document.getElementById('modalConcluir');
    const abandonar = document.getElementById('modalAbandonar');
    concluir.style.display = abandonar.style.display = 'flex';
    concluir.classList.add('active');
    abandonar.classList.add('active');

    document.querySelector('#modalConcluir .btn-cancel').click();
    expect(concluir.classList.contains('active')).toBe(false);
    expect(abandonar.classList.contains('active')).toBe(false);
    vi.advanceTimersByTime(300);
    expect(concluir.style.display).toBe('none');
    expect(abandonar.style.display).toBe('none');

    concluir.style.display = 'flex';
    concluir.classList.add('active');
    concluir.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(concluir.classList.contains('active')).toBe(false);

    abandonar.classList.add('active');
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(abandonar.classList.contains('active')).toBe(false);
  });

  it('desabilita a confirmação e mostra processamento ao submeter', () => {
    arrange();
    const form = document.getElementById('formConcluir');
    const button = form.querySelector('.btn-confirm');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    expect(button.disabled).toBe(true);
    expect(button.textContent).toContain('Processando');
    expect(button.querySelector('.fa-spinner')).not.toBeNull();
  });
});
describe('atores vinculados à comunidade', () => {
  function arrange() {
    document.body.innerHTML = `
      <input class="search-input">
      <button type="button" class="btn-search"></button>
      <div class="forms-list">
        <div class="form-item"><span class="item-name">Ana Ávila</span></div>
        <div class="form-item"><span class="item-name">Bruno Costa</span></div>
      </div>`;
    runInlineScript('Views/Comunidade/AtoresVinculados.cshtml');
    dispatchReady();
  }

  it('filtra os elementos realmente renderizados, sem diferenciar caixa ou espaços', () => {
    arrange();
    const input = document.querySelector('.search-input');
    input.value = '  BRUNO  ';
    input.dispatchEvent(new Event('input'));
    expect(visible(document.querySelectorAll('.form-item'))).toHaveLength(1);
    expect(visible(document.querySelectorAll('.form-item'))[0].textContent).toContain('Bruno Costa');
  });

  it('o botão aplica a busca, devolve foco e Escape restaura a lista', () => {
    arrange();
    const input = document.querySelector('.search-input');
    input.value = 'ana';
    document.querySelector('.btn-search').click();
    expect(visible(document.querySelectorAll('.form-item'))).toHaveLength(1);
    expect(document.activeElement).toBe(input);

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(input.value).toBe('');
    expect(visible(document.querySelectorAll('.form-item'))).toHaveLength(2);
  });
});
describe('atividades vinculadas à comunidade', () => {
  it('filtra as classes realmente renderizadas', () => {
    document.body.innerHTML = `
      <input class="search-input">
      <div class="form-item"><span class="item-name">Oficina de Saúde</span></div>
      <div class="form-item"><span class="item-name">Reforço Escolar</span></div>`;
    runInlineScript('Views/Comunidade/AtividadesVinculadas.cshtml');
    dispatchReady();

    const input = document.querySelector('.search-input');
    input.value = '  SAÚDE ';
    input.dispatchEvent(new Event('input'));
    expect(visible(document.querySelectorAll('.form-item'))).toHaveLength(1);
    expect(visible(document.querySelectorAll('.form-item'))[0].textContent).toContain('Oficina de Saúde');
  });
});
