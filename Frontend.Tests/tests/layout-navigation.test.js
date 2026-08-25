import { describe, expect, it, vi } from 'vitest';
import { dispatchReady, readProjectFile, runInlineScript } from './helpers/project.js';

function layoutFixture() {
  document.body.innerHTML = `
    <div id="sidebar"></div>
    <div id="sidebarBackdrop"></div>
    <button id="sidebarToggle" type="button" aria-label="Abrir Menu"></button>
  `;
  runInlineScript('Views/Shared/_Layout.cshtml');
  dispatchReady();
}

describe('menu móvel realmente carregado pelo layout', () => {
  it('abre e fecha pelo botão mantendo classes e ARIA sincronizados', () => {
    layoutFixture();
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    const toggle = document.getElementById('sidebarToggle');

    toggle.click();
    expect(sidebar.classList.contains('open')).toBe(true);
    expect(backdrop.classList.contains('show')).toBe(true);
    expect(document.body.classList.contains('sidebar-open')).toBe(true);
    expect(toggle.getAttribute('aria-label')).toBe('Fechar Menu');
    expect(toggle.getAttribute('aria-expanded')).toBe('true');

    toggle.click();
    expect(sidebar.classList.contains('open')).toBe(false);
    expect(backdrop.classList.contains('show')).toBe(false);
    expect(document.body.classList.contains('sidebar-open')).toBe(false);
    expect(toggle.getAttribute('aria-label')).toBe('Abrir Menu');
    expect(toggle.getAttribute('aria-expanded')).toBe('false');
  });

  it('impede que o clique do botão se propague', () => {
    layoutFixture();
    const bodyClick = vi.fn();
    document.body.addEventListener('click', bodyClick);
    document.getElementById('sidebarToggle').click();
    expect(bodyClick).not.toHaveBeenCalled();
  });

  it('fecha pelo backdrop somente quando está aberto', () => {
    layoutFixture();
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    const toggle = document.getElementById('sidebarToggle');

    backdrop.click();
    expect(sidebar.classList.contains('open')).toBe(false);
    toggle.click();
    backdrop.click();
    expect(sidebar.classList.contains('open')).toBe(false);
    expect(toggle.getAttribute('aria-expanded')).toBe('false');
  });

  it('fecha com Escape e ignora outras teclas', () => {
    layoutFixture();
    const sidebar = document.getElementById('sidebar');
    document.getElementById('sidebarToggle').click();

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    expect(sidebar.classList.contains('open')).toBe(true);
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(sidebar.classList.contains('open')).toBe(false);
  });

  it('falha de forma segura e observável se o contrato do markup for quebrado', () => {
    const warning = vi.spyOn(console, 'warn').mockImplementation(() => {});
    document.body.innerHTML = '<div id="sidebar"></div>';
    runInlineScript('Views/Shared/_Layout.cshtml');
    dispatchReady();
    expect(warning).toHaveBeenCalledWith('Elementos do menu não encontrados');
  });
});

describe('contrato da navegação real', () => {
  it('usa links MVC no layout e não carrega o asset AJAX legado', () => {
    const layout = readProjectFile('Views/Shared/_Layout.cshtml');
    expect(layout).toContain('asp-controller="Comunidade" asp-action="Index"');
    expect(layout).toContain('asp-controller="Atores" asp-action="Index"');
    expect(layout).toContain('asp-controller="FichaPrimeiroContato" asp-action="Index"');
    expect(layout).toContain('asp-controller="Diariocampo" asp-action="Index"');
    expect(layout).not.toMatch(/<script[^>]+sidebar\.js/i);
  });
});