import { beforeEach, describe, expect, it, vi } from 'vitest';
import { dispatchReady, flushPromises, runPublicScript } from './helpers/project.js';

function legacyDiaryFixture() {
  document.body.innerHTML = `
    <textarea id="descricaoInput"></textarea><ul id="mention-list"></ul>
    <div id="modalAcao" style="display:none">
      <select id="tipoAcao"><option value=""></option><option value="equipe">Equipe</option><option value="institucional">Institucional</option></select>
      <input id="nomeAcao"><select id="eixoAcao"><option value=""></option><option value="1">Saúde</option></select>
      <input id="provedorAcao"><span id="provedorRequired"></span><div id="provedorCard"></div>
      <input id="quantidadeAcao" value="1">
    </div>
    <div id="atividadesContainer"><div class="empty-state-acoes">Vazio</div></div>
    <div id="acoesInstitucionaisContainer"><div class="empty-state-acoes">Vazio</div></div>
    <input id="cep"><input id="rua">
  `;
  window.atoresDisponiveis = [
    { Text: 'Ana Silva' }, { Text: 'Beatriz Souza' }, { Text: 'Carlos Lima' }
  ];
  runPublicScript('diariocampo_padrao.js');
  dispatchReady();
}

describe('diário de campo — fluxo padrão', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    legacyDiaryFixture();
  });

  it('abre e fecha o modal sincronizando classe, rolagem e limpeza', () => {
    const modal = document.getElementById('modalAcao');
    window.abrirModalAcao();
    expect(modal.style.display).toBe('flex');
    expect(modal.classList.contains('active')).toBe(true);
    expect(document.body.style.overflow).toBe('hidden');

    document.getElementById('tipoAcao').value = 'institucional';
    document.getElementById('nomeAcao').value = 'Visita';
    document.getElementById('eixoAcao').value = '1';
    document.getElementById('provedorAcao').value = 'Parceiro';
    document.getElementById('quantidadeAcao').value = '4';
    window.fecharModal('modalAcao');

    expect(modal.style.display).toBe('none');
    expect(modal.classList.contains('active')).toBe(false);
    expect(document.body.style.overflow).toBe('auto');
    expect(document.getElementById('nomeAcao').value).toBe('');
    expect(document.getElementById('quantidadeAcao').value).toBe('1');
  });

  it('fecha o modal ao clicar apenas no próprio fundo', () => {
    const modal = document.getElementById('modalAcao');
    const close = vi.spyOn(window, 'fecharModal');
    modal.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(close).toHaveBeenCalledWith('modalAcao');

    close.mockClear();
    document.getElementById('nomeAcao').dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(close).not.toHaveBeenCalled();
  });

  it('indica provedor obrigatório somente em ação institucional', () => {
    const type = document.getElementById('tipoAcao');
    type.value = 'equipe';
    window.toggleTipoAcao();
    expect(document.getElementById('provedorRequired').style.display).toBe('none');
    type.value = 'institucional';
    window.toggleTipoAcao();
    expect(document.getElementById('provedorRequired').style.display).toBe('inline');
  });

  it.each([
    ['', 'Nome', '1', '', 'Selecione o tipo de ação'],
    ['equipe', '', '1', '', 'O nome da ação é obrigatório'],
    ['equipe', 'Nome', '', '', 'Selecione um eixo'],
    ['institucional', 'Nome', '1', '', 'o provedor é obrigatório']
  ])('bloqueia ação incompleta sem alterar a lista', (tipo, nome, eixo, provedor, mensagem) => {
    document.getElementById('tipoAcao').value = tipo;
    document.getElementById('nomeAcao').value = nome;
    document.getElementById('eixoAcao').value = eixo;
    document.getElementById('provedorAcao').value = provedor;
    window.salvarAcao();
    expect(window.alert).toHaveBeenCalledWith(expect.stringContaining(mensagem));
    expect(document.querySelectorAll('.action-list-item')).toHaveLength(0);
  });

  it('adiciona uma ação de equipe com campos enviados ao backend e remove o estado vazio', () => {
    vi.setSystemTime(new Date('2026-08-24T12:00:00Z'));
    document.getElementById('tipoAcao').value = 'equipe';
    document.getElementById('nomeAcao').value = 'Roda de conversa';
    document.getElementById('eixoAcao').value = '1';
    document.getElementById('quantidadeAcao').value = '2';
    window.salvarAcao();

    const item = document.querySelector('#atividadesContainer .action-list-item');
    expect(item).not.toBeNull();
    expect(item.textContent).toContain('Roda de conversa');
    expect(item.textContent).toContain('Equipe');
    expect(item.textContent).toContain('(2x)');
    expect(document.querySelector('#atividadesContainer .empty-state-acoes')).toBeNull();
    expect(item.querySelector('input[name$=".Nome"]').value).toBe('Roda de conversa');
    expect(item.querySelector('input[name$=".Tipo"]').value).toBe('equipe');
    expect(document.getElementById('modalAcao').style.display).toBe('none');
  });

  it('filtra menções, insere a escolha no cursor e oculta sugestões inválidas', () => {
    const textarea = document.getElementById('descricaoInput');
    const list = document.getElementById('mention-list');
    textarea.value = 'Convidar @an';
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    textarea.dispatchEvent(new Event('input'));
    expect(list.style.display).toBe('block');
    expect(list.querySelectorAll('.mention-item')).toHaveLength(1);
    expect(list.textContent).toContain('Ana Silva');

    list.querySelector('.mention-item').click();
    expect(textarea.value).toBe('Convidar @Ana Silva ');
    expect(textarea.selectionStart).toBe(textarea.value.length);
    expect(list.style.display).toBe('none');

    textarea.value = 'email@an';
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    textarea.dispatchEvent(new Event('input'));
    expect(list.style.display).toBe('none');
  });

  it('valida CEP antes da rede', () => {
    window.fetch = vi.fn();
    document.getElementById('cep').value = '123';
    window.buscarCEP();
    expect(window.alert).toHaveBeenCalledWith('Formato de CEP inválido.');
    expect(window.fetch).not.toHaveBeenCalled();
  });

  it('preenche endereço encontrado pelo ViaCEP', async () => {
    window.fetch = vi.fn().mockResolvedValue({
      json: vi.fn().mockResolvedValue({
        logradouro: 'Rua A', bairro: 'Centro', localidade: 'Fortaleza', uf: 'CE'
      })
    });
    document.getElementById('cep').value = '60.000-000';
    window.buscarCEP();
    expect(document.getElementById('rua').value).toBe('Pesquisando...');
    await flushPromises();
    expect(window.fetch).toHaveBeenCalledWith('https://viacep.com.br/ws/60000000/json/');
    expect(document.getElementById('rua').value).toBe('Rua A, Centro, Fortaleza - CE');
  });

  it('limpa o campo e informa CEP inexistente ou falha de rede', async () => {
    window.fetch = vi.fn().mockResolvedValue({ json: vi.fn().mockResolvedValue({ erro: true }) });
    document.getElementById('cep').value = '60000000';
    window.buscarCEP();
    await flushPromises();
    expect(document.getElementById('rua').value).toBe('');
    expect(window.alert).toHaveBeenCalledWith('CEP não encontrado.');

    window.alert.mockClear();
    window.fetch.mockRejectedValueOnce(new Error('offline'));
    window.buscarCEP();
    await flushPromises();
    expect(window.alert).toHaveBeenCalledWith('Erro ao buscar CEP.');
  });

  it('renderiza nomes de menção hostis somente como texto', () => {
    const attack = '<img src=x onerror="window.__legacyMentionXss=1">';
    window.atoresDisponiveis.splice(0, window.atoresDisponiveis.length, { Text: attack });
    const textarea = document.getElementById('descricaoInput');
    textarea.value = '@';
    textarea.setSelectionRange(1, 1);
    textarea.dispatchEvent(new Event('input'));

    const list = document.getElementById('mention-list');
    expect(list.textContent).toContain(attack);
    expect(list.querySelector('img')).toBeNull();
    expect(window.__legacyMentionXss).toBeUndefined();
  });

  it('renderiza dados hostis da ação como texto e mantém valores exatos nos inputs', () => {
    const attack = '<img src=x onerror="window.__legacyActionXss=1">';
    document.getElementById('tipoAcao').value = 'institucional';
    document.getElementById('nomeAcao').value = attack;
    document.getElementById('eixoAcao').value = '1';
    document.getElementById('provedorAcao').value = attack;
    window.salvarAcao();

    const item = document.querySelector('#acoesInstitucionaisContainer .action-list-item');
    expect(item.textContent).toContain(attack);
    expect(item.querySelector('img')).toBeNull();
    expect(item.querySelector('input[name$=".Nome"]').value).toBe(attack);
    expect(window.__legacyActionXss).toBeUndefined();
  });
});

function currentDiaryFixture() {
  document.body.innerHTML = `
    <textarea id="descricaoInput" class="clean-input"></textarea><ul id="mention-list"></ul>
    <input id="tipoAcaoInput"><input id="nomeAcao"><input id="provedorAcao"><input id="atorAcao"><input id="quantidadeAcao" value="1">
    <label><input type="checkbox" name="modalEixos" value="11" data-nome="Saúde"></label>
    <label><input type="checkbox" name="modalEixos" value="22" data-nome="Educação"></label>
    <div id="modalAcao" class="modal-overlay" style="display:none"></div>
    <div id="container-equipe"></div><div id="empty-equipe"></div><span id="count-equipe">0</span>
    <div id="container-institucional"></div><div id="empty-institucional"></div><span id="count-institucional">0</span>
    <div id="mapa-diario"></div><input id="rua" class="clean-input">
    <button id="edit-save-btn" type="button"></button><button class="edit-only-btn"></button>
    <button id="openDeleteModalBtn"></button>
    <div id="deleteConfirmationModal" class="modal-overlay"><button id="cancelDeleteBtn"></button></div>
  `;
  window.atoresDisponiveis = [{ Nome: 'Ana Silva' }, { Text: 'Bruno Costa' }];
  window.countEquipe = 0;
  window.countInst = 0;
  window.initMapSelector = vi.fn();
  runPublicScript('diariocampo.js');
  vi.spyOn(window, 'fecharModal');
  dispatchReady();
}

describe('diário de campo — fluxo atual', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    currentDiaryFixture();
  });

  it('abre o modal limpando valores e seleções anteriores', () => {
    document.getElementById('nomeAcao').value = 'Anterior';
    document.querySelector('[name="modalEixos"]').checked = true;
    window.abrirModalAcao('equipe');
    expect(document.getElementById('tipoAcaoInput').value).toBe('equipe');
    expect(document.getElementById('nomeAcao').value).toBe('');
    expect(document.getElementById('quantidadeAcao').value).toBe('1');
    expect([...document.querySelectorAll('[name="modalEixos"]')].every(x => !x.checked)).toBe(true);
    expect(document.getElementById('modalAcao').style.display).toBe('flex');
    expect(document.body.style.overflow).toBe('hidden');
  });

  it.each([
    ['', true, 'Parceiro', 'O campo Nome é obrigatório.'],
    ['Ação', false, 'Parceiro', 'Selecione pelo menos um Eixo.'],
    ['Ação', true, '', 'O campo Provedor Externo é obrigatório.']
  ])('valida os dados antes de criar a ação', (nome, eixo, provedor, message) => {
    document.getElementById('tipoAcaoInput').value = 'equipe';
    document.getElementById('nomeAcao').value = nome;
    document.getElementById('provedorAcao').value = provedor;
    document.querySelector('[name="modalEixos"]').checked = eixo;
    window.salvarAcaoNoGrid();
    expect(window.alert).toHaveBeenCalledWith(message);
    expect(document.querySelectorAll('.action-list-item')).toHaveLength(0);
  });

  it('gera coleção compatível com model binding para cada eixo selecionado', () => {
    vi.setSystemTime(new Date('2026-08-24T12:00:00Z'));
    document.getElementById('tipoAcaoInput').value = 'equipe';
    document.getElementById('nomeAcao').value = 'Acompanhamento';
    document.getElementById('provedorAcao').value = 'Instituição X';
    document.getElementById('quantidadeAcao').value = '3';
    document.querySelectorAll('[name="modalEixos"]').forEach(x => { x.checked = true; });
    window.salvarAcaoNoGrid();

    const item = document.querySelector('#container-equipe .action-list-item');
    expect(item).not.toBeNull();
    expect(item.textContent).toContain('Acompanhamento');
    expect(item.textContent).toContain('Saúde, Educação');
    expect(item.querySelector('[name="TempAcoes.Index"]')).not.toBeNull();
    expect(item.querySelector('[name$=".Nome"]').value).toBe('Acompanhamento');
    expect([...item.querySelectorAll('[name$=".FkIdEixo"]')].map(x => x.value)).toEqual(['11', '22']);
    expect(document.getElementById('count-equipe').innerText).toBe(1);
    expect(document.getElementById('empty-equipe').style.display).toBe('none');
    expect(window.fecharModal).toHaveBeenCalledWith('modalAcao');
  });

  it('filtra e insere menção usando Nome ou Text', () => {
    const textarea = document.getElementById('descricaoInput');
    textarea.value = 'Falou com @bru hoje';
    textarea.setSelectionRange(14, 14);
    textarea.dispatchEvent(new Event('input'));
    const list = document.getElementById('mention-list');
    expect(list.style.display).toBe('block');
    expect(list.textContent).toContain('Bruno Costa');

    list.querySelector('span').click();
    expect(textarea.value).toBe('Falou com @Bruno Costa hoje');
    expect(list.style.display).toBe('none');
  });

  it('inicializa mapa com o contrato correto', () => {
    expect(window.initMapSelector).toHaveBeenCalledWith('mapa-diario', 'rua', {
      sourceInputId: 'rua', manualInputId: 'rua', showSearchControl: false
    });
  });

  it('alterna do modo leitura para edição preservando readonly', () => {
    const regular = document.getElementById('descricaoInput');
    const readonly = document.getElementById('rua');
    readonly.setAttribute('readonly', 'readonly');
    // O estado inicial já foi aplicado no DOMContentLoaded.
    expect(regular.disabled).toBe(true);
    expect(document.querySelector('.edit-only-btn').style.display).toBe('none');

    document.getElementById('edit-save-btn').click();
    expect(regular.disabled).toBe(false);
    expect(readonly.disabled).toBe(true);
    expect(document.querySelector('.edit-only-btn').style.display).toBe('inline-flex');
    expect(document.getElementById('edit-save-btn').type).toBe('submit');
    expect(document.getElementById('edit-save-btn').textContent).toContain('Salvar Alterações');
  });

  it('abre e cancela o modal de exclusão com a transição prevista', () => {
    const modal = document.getElementById('deleteConfirmationModal');
    document.getElementById('openDeleteModalBtn').click();
    expect(modal.style.display).toBe('flex');
    vi.advanceTimersByTime(10);
    expect(modal.classList.contains('active')).toBe(true);

    document.getElementById('cancelDeleteBtn').click();
    expect(modal.classList.contains('active')).toBe(false);
    vi.advanceTimersByTime(300);
    expect(modal.style.display).toBe('none');
  });

  it('renderiza menção hostil como texto sem criar elemento ou executar evento', () => {
    const attack = '<img src=x onerror="window.__currentMentionXss=1">';
    window.atoresDisponiveis.push({ Nome: attack });
    const textarea = document.getElementById('descricaoInput');
    textarea.value = '@';
    textarea.setSelectionRange(1, 1);
    textarea.dispatchEvent(new Event('input'));

    const list = document.getElementById('mention-list');
    const item = [...list.querySelectorAll('.mention-item')].find(option => option.textContent.includes(attack));
    expect(item).toBeDefined();
    expect(item.querySelector('img')).toBeNull();
    expect(window.__currentMentionXss).toBeUndefined();
  });

  it('protege card e model binding contra XSS, preservando ator, múltiplos eixos e chave única', () => {
    vi.setSystemTime(new Date('2026-08-24T15:00:00Z'));
    const nameAttack = '<img src=x onerror="window.__currentActionXss=1">';
    const providerAttack = '<svg onload="window.__currentProviderXss=1">';
    const axisAttack = '<script>window.__currentAxisXss=1</script>';
    document.getElementById('tipoAcaoInput').value = 'institucional';
    document.getElementById('nomeAcao').value = nameAttack;
    document.getElementById('provedorAcao').value = providerAttack;
    document.getElementById('atorAcao').value = '77';
    const axes = [...document.querySelectorAll('[name="modalEixos"]')];
    axes.forEach(option => { option.checked = true; });
    axes[0].dataset.nome = axisAttack;
    window.salvarAcaoNoGrid();

    const item = document.querySelector('#container-institucional .action-list-item');
    expect(item.textContent).toContain(nameAttack);
    expect(item.textContent).toContain(providerAttack);
    expect(item.textContent).toContain(axisAttack);
    expect(item.querySelector('img')).toBeNull();
    expect(item.querySelector('svg')).toBeNull();
    expect(item.querySelector('script')).toBeNull();

    const index = item.querySelector('[name="TempAcoes.Index"]').value;
    const boundInputs = [...item.querySelectorAll('input[type="hidden"]')]
      .filter(input => input.name !== 'TempAcoes.Index');
    expect(boundInputs.every(input => input.name.startsWith(`TempAcoes[${index}].`))).toBe(true);
    expect(item.querySelector('[name$=".Nome"]').value).toBe(nameAttack);
    expect(item.querySelector('[name$=".Provedor"]').value).toBe(providerAttack);
    expect(item.querySelector('[name$=".FkIdAtor"]').value).toBe('77');
    expect([...item.querySelectorAll('[name$=".FkIdEixo"]')].map(input => input.value)).toEqual(['11', '22']);
    expect(window.__currentActionXss).toBeUndefined();
    expect(window.__currentProviderXss).toBeUndefined();
    expect(window.__currentAxisXss).toBeUndefined();
  });
});
