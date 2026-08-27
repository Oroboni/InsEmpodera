import { beforeEach, describe, expect, it, vi } from 'vitest';
import { dispatchReady, runPublicScript } from './helpers/project.js';

describe('interface de login', () => {
  it('alterna a visibilidade da senha nos dois sentidos', () => {
    document.body.innerHTML = `
      <input name="Password" type="password">
      <input id="showPassword" type="checkbox">
    `;
    runPublicScript('login.js');
    dispatchReady();

    const password = document.querySelector('[name="Password"]');
    const checkbox = document.getElementById('showPassword');
    checkbox.checked = true;
    checkbox.dispatchEvent(new Event('change'));
    expect(password.type).toBe('text');

    checkbox.checked = false;
    checkbox.dispatchEvent(new Event('change'));
    expect(password.type).toBe('password');
  });

  it('cria o efeito visual no ponto clicado e o remove após a animação', () => {
    vi.useFakeTimers();
    document.body.innerHTML = '<button class="login-button">Entrar</button>';
    const button = document.querySelector('button');
    vi.spyOn(button, 'getBoundingClientRect').mockReturnValue({
      width: 200, height: 50, left: 10, top: 20, right: 210, bottom: 70,
      x: 10, y: 20, toJSON() {}
    });
    runPublicScript('login.js');
    dispatchReady();

    button.dispatchEvent(new MouseEvent('click', { clientX: 60, clientY: 45, bubbles: true }));
    const ripple = button.querySelector('span');
    expect(ripple).not.toBeNull();
    expect(ripple.style.width).toBe('200px');
    expect(ripple.style.left).toBe('-50px');
    expect(ripple.style.top).toBe('-75px');
    expect(button.style.overflow).toBe('hidden');

    vi.advanceTimersByTime(600);
    expect(button.querySelector('span')).toBeNull();
  });

  it('é defensivo quando os controles opcionais não existem', () => {
    expect(() => {
      runPublicScript('login.js');
      dispatchReady();
    }).not.toThrow();
  });
});

describe('recuperação de senha', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    document.body.innerHTML = `
      <form class="recovery-form">
        <h2>Recuperar</h2>
        <p class="recovery-description">Descrição</p>
        <div class="form-group">
          <label for="emailInput">E-mail</label>
          <input class="form-input" id="emailInput">
        </div>
        <button id="recoveryBtn" class="recovery-button">ENVIAR LINK</button>
        <button type="button" class="cancel-button">Cancelar</button>
        <div id="successMessage"></div>
      </form>
    `;
    Object.defineProperty(window.navigator, 'vibrate', {
      configurable: true,
      value: vi.fn()
    });
    runPublicScript('forget.js');
    dispatchReady();
  });

  it.each([
    ['pessoa@example.org', true],
    ['nome.sobrenome+tag@dominio.com.br', true],
    ['', false],
    ['sem-arroba.example.org', false],
    ['nome@dominio', false],
    ['nome @dominio.com', false]
  ])('valida o formato de e-mail %j', (email, expected) => {
    expect(window.validateEmail(email)).toBe(expected);
  });

  it('aplica e remove o estado visual de foco corretamente', () => {
    const input = document.getElementById('emailInput');
    const group = input.parentElement;
    const label = group.querySelector('label');

    input.dispatchEvent(new FocusEvent('focus'));
    expect(group.classList.contains('focused')).toBe(true);
    expect(label.style.transform).toContain('translateY');

    input.value = 'mantem@valor.com';
    input.dispatchEvent(new FocusEvent('blur'));
    expect(group.classList.contains('focused')).toBe(true);

    input.value = '';
    input.dispatchEvent(new FocusEvent('blur'));
    expect(group.classList.contains('focused')).toBe(false);
    expect(label.style.transform).toBe('');
  });

  it('define atrasos progressivos para todos os elementos animados', () => {
    expect(document.querySelector('h2').style.animationDelay).toBe('0.6s');
    expect(document.querySelector('.recovery-description').style.animationDelay).toBe('0.7s');
    expect(document.querySelector('.form-input').style.animationDelay).toBe('0.8s');
    expect(document.querySelector('.recovery-button').style.animationDelay).toBe('0.9s');
    expect(document.querySelector('.cancel-button').style.animationDelay).toBe('1s');
  });

  it('bloqueia envio inválido, mostra apenas um erro e o remove no prazo', () => {
    const form = document.querySelector('form');
    const button = document.getElementById('recoveryBtn');

    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    expect(document.querySelectorAll('.error-message')).toHaveLength(1);
    expect(document.querySelector('.error-text').textContent).toContain('e-mail válido');
    expect(button.disabled).toBe(false);

    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    expect(document.querySelectorAll('.error-message')).toHaveLength(1);

    vi.advanceTimersByTime(4_400);
    expect(document.querySelector('.error-message')).toBeNull();
  });

  it('permite o envio real ao servidor e mantém o estado de processamento', () => {
    const input = document.getElementById('emailInput');
    const form = document.querySelector('form');
    const button = document.getElementById('recoveryBtn');
    input.value = 'pessoa@example.org';

    const event = new Event('submit', { bubbles: true, cancelable: true });
    form.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(false);
    expect(button.disabled).toBe(true);
    expect(button.classList.contains('loading')).toBe(true);
    expect(document.querySelectorAll('#loading-styles')).toHaveLength(1);
    expect(button.textContent).toBe('ENVIANDO...');
  });

  it('não intercepta o Enter e deixa o navegador enviar o formulário nativamente', () => {
    const input = document.getElementById('emailInput');
    input.value = 'pessoa@example.org';
    const event = new KeyboardEvent('keypress', {
      key: 'Enter', bubbles: true, cancelable: true
    });

    input.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(false);
    expect(document.getElementById('recoveryBtn').disabled).toBe(false);
  });

  it('recupera o botão de um erro de rede', () => {
    const errorLog = vi.spyOn(console, 'error').mockImplementation(() => {});
    const button = document.getElementById('recoveryBtn');
    button.disabled = true;
    button.classList.add('loading');

    window.handleNetworkError(new Error('offline'));
    expect(button.disabled).toBe(false);
    expect(button.textContent).toBe('ENVIAR LINK');
    expect(button.classList.contains('loading')).toBe(false);
    expect(document.querySelector('.error-text').textContent).toContain('Erro de conexão');
    expect(errorLog).toHaveBeenCalledWith('Erro de rede:', expect.any(Error));
  });

  it('injeta cada bloco de estilos dinâmicos apenas uma vez', () => {
    window.addErrorAnimationStyles();
    window.addErrorAnimationStyles();
    window.startLoadingAnimation(document.getElementById('recoveryBtn'));
    window.startLoadingAnimation(document.getElementById('recoveryBtn'));

    expect(document.querySelectorAll('#error-styles')).toHaveLength(1);
    expect(document.querySelectorAll('#loading-styles')).toHaveLength(1);
  });
});
