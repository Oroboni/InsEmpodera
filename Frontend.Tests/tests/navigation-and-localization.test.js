import { describe, expect, it, vi } from 'vitest';
import { dispatchReady, flushPromises, readProjectFile, runPublicScript } from './helpers/project.js';

function sidebarFixture() {
  document.body.innerHTML = `
    <nav class="section-1">
      <li data-page="home"></li>
      <li data-page="Usuarios"></li>
      <li data-page="DiariosDeCampo"></li>
    </nav>
    <main id="content">Inicial</main>
  `;
}

describe('asset legado de navegação AJAX (não carregado pelo layout)', () => {
  it('carrega a rota por AJAX, extrai #content, ativa o item e grava o histórico', async () => {
    sidebarFixture();
    const pushState = vi.spyOn(window.history, 'pushState');
    window.fetch = vi.fn().mockResolvedValue({
      text: vi.fn().mockResolvedValue('<html><body><main id="content"><h1>Usuários</h1></main></body></html>')
    });
    runPublicScript('sidebar.js');

    document.querySelector('[data-page="Usuarios"]').click();
    await flushPromises();

    expect(window.fetch).toHaveBeenCalledWith('/Home/Usuarios', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    expect(document.querySelector('#content h1').textContent).toBe('Usuários');
    expect(document.querySelector('[data-page="Usuarios"]').classList.contains('active')).toBe(true);
    expect(document.querySelector('[data-page="home"]').classList.contains('active')).toBe(false);
    expect(pushState).toHaveBeenCalledWith({ page: 'Usuarios' }, '', '/usuarios');
  });

  it('aceita uma resposta sem #content e usa o corpo inteiro', async () => {
    sidebarFixture();
    window.fetch = vi.fn().mockResolvedValue({
      text: vi.fn().mockResolvedValue('<p id="resultado">Página parcial</p>')
    });
    runPublicScript('sidebar.js');
    window.loadPage('home');
    await flushPromises();
    expect(document.getElementById('resultado').textContent).toBe('Página parcial');
  });

  it('mostra retorno 404 para página desconhecida sem chamar a rede', () => {
    sidebarFixture();
    window.fetch = vi.fn();
    runPublicScript('sidebar.js');
    window.loadPage('NaoExiste');
    expect(document.getElementById('content').textContent).toContain('404');
    expect(window.fetch).not.toHaveBeenCalled();
  });

  it('mostra retorno 404 quando a requisição falha', async () => {
    sidebarFixture();
    vi.spyOn(console, 'error').mockImplementation(() => {});
    window.fetch = vi.fn().mockRejectedValue(new Error('offline'));
    runPublicScript('sidebar.js');
    window.loadPage('Comunidades');
    await flushPromises();
    expect(document.getElementById('content').textContent).toContain('Página não encontrada');
  });

  it('restaura a página via popstate sem criar nova entrada no histórico', async () => {
    sidebarFixture();
    const pushState = vi.spyOn(window.history, 'pushState');
    window.fetch = vi.fn().mockResolvedValue({
      text: vi.fn().mockResolvedValue('<main id="content">Diários</main>')
    });
    runPublicScript('sidebar.js');

    window.dispatchEvent(new PopStateEvent('popstate', { state: { page: 'DiariosDeCampo' } }));
    await flushPromises();
    expect(window.fetch).toHaveBeenCalledWith('/Home/DiariosDeCampo', expect.any(Object));
    expect(pushState).not.toHaveBeenCalled();
  });

  it('inicializa o estado do histórico a partir da URL atual', () => {
    sidebarFixture();
    window.history.replaceState(null, '', '/usuarios');
    const replaceState = vi.spyOn(window.history, 'replaceState');
    window.fetch = vi.fn();
    runPublicScript('sidebar.js');
    dispatchReady();
    expect(replaceState).toHaveBeenCalledWith({ page: 'Usuarios' }, '', '/usuarios');
  });

  it('mantém a grafia canônica da rota de diários de campo', () => {
    const source = readProjectFile('wwwroot/js/sidebar.js');
    expect(source).toContain("'diariosdecampo': 'DiariosDeCampo'");
  });
});

describe('localização dinâmica da interface', () => {
  it('solicita o catálogo da página e traduz ocorrências mais longas primeiro', async () => {
    window.history.replaceState(null, '', '/usuarios?pagina=1');
    document.body.innerHTML = '<button>AB A</button><span data-i18n-text>Salvar</span>';
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({ AB: 'Longo', A: 'Curto', Salvar: 'Save' })
    });
    runPublicScript('localization.js');
    await flushPromises();

    expect(window.fetch).toHaveBeenCalledWith('/Language/Catalog?page=%2Fusuarios', {
      credentials: 'same-origin'
    });
    expect(document.querySelector('button').textContent).toBe('Longo Curto');
    expect(document.querySelector('[data-i18n-text]').textContent).toBe('Save');
    expect(window.translateText('AB A')).toBe('Longo Curto');
  });

  it('traduz alertas e confirmações antes de chamar a implementação nativa', async () => {
    const nativeAlert = window.alert;
    const nativeConfirm = window.confirm;
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({ Excluir: 'Delete', Confirmar: 'Confirm' })
    });
    runPublicScript('localization.js');
    await vi.waitFor(() => expect(window.translateText('Excluir item')).toBe('Delete item'));

    window.alert('Excluir item');
    window.confirm('Confirmar ação');
    expect(nativeAlert).toHaveBeenCalledWith('Delete item');
    expect(nativeConfirm).toHaveBeenCalledWith('Confirm ação');
  });

  it('traduz conteúdo e atributos adicionados depois do carregamento', async () => {
    document.body.innerHTML = '<div id="host"></div>';
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({ Novo: 'New', Remover: 'Remove' })
    });
    runPublicScript('localization.js');
    await flushPromises();

    const button = document.createElement('button');
    button.textContent = 'Novo';
    button.title = 'Remover item';
    document.getElementById('host').appendChild(button);
    await flushPromises();
    expect(button.textContent).toBe('New');
    expect(button.title).toBe('Remove item');

    button.setAttribute('aria-label', 'Remover');
    await flushPromises();
    expect(button.getAttribute('aria-label')).toBe('Remove');
  });

  it('preserva o português se o catálogo não puder ser carregado', async () => {
    document.body.innerHTML = '<button>Salvar</button>';
    window.fetch = vi.fn().mockRejectedValue(new Error('offline'));
    runPublicScript('localization.js');
    await flushPromises();
    expect(document.querySelector('button').textContent).toBe('Salvar');
    expect(window.translateText('Salvar')).toBe('Salvar');
  });
});
