import { beforeEach, describe, expect, it, vi } from 'vitest';
import { dispatchReady, runPublicScript } from './helpers/project.js';

describe('utilitários globais de formulário', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-08-24T12:00:00.000Z'));
  });

  it('preserva o telefone informado, inclusive códigos internacionais e ramais', () => {
    document.body.innerHTML = '<input id="telefone" value="85999998888">';
    runPublicScript('form-utils.js');

    window.initTelefoneMask('telefone');
    const input = document.getElementById('telefone');
    expect(input.value).toBe('85999998888');

    input.value = '+55 (85) 3232-1234 ramal 99';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    expect(input.value).toBe('+55 (85) 3232-1234 ramal 99');
  });

  it('não limita o telefone a onze dígitos e ignora elemento ausente', () => {
    runPublicScript('form-utils.js');
    expect(() => window.initTelefoneMask('inexistente')).not.toThrow();

    document.body.innerHTML = '<input id="telefone">';
    window.initTelefoneMask('telefone');
    const input = document.getElementById('telefone');
    input.value = '123456789012345';
    input.dispatchEvent(new Event('input'));
    expect(input.value).toBe('123456789012345');
  });

  it('define hoje como limite e rejeita datas futuras com texto localizado', () => {
    window.translateText = vi.fn(() => 'Future dates are not allowed.');
    document.body.innerHTML = '<input id="data" type="date">';
    runPublicScript('form-utils.js');

    window.initDateMaxToday('data');
    const input = document.getElementById('data');
    expect(input.max).toBe('2026-08-24');

    input.value = '2026-08-25';
    input.dispatchEvent(new Event('change'));
    expect(input.validationMessage).toBe('Future dates are not allowed.');
    expect(window.translateText).toHaveBeenCalledWith('A data não pode ser futura.');

    input.value = '2026-08-23';
    input.dispatchEvent(new Event('change'));
    expect(input.validationMessage).toBe('');
  });

  it('valida a data após digitação assíncrona e aceita campo vazio', () => {
    document.body.innerHTML = '<input id="data" type="date">';
    runPublicScript('form-utils.js');
    window.initDateMaxToday('data');

    const input = document.getElementById('data');
    input.value = '2026-08-30';
    input.dispatchEvent(new Event('input'));
    vi.advanceTimersByTime(100);
    expect(input.checkValidity()).toBe(false);

    input.value = '';
    expect(() => window.validateDateField(input)).not.toThrow();
    expect(input.validationMessage).toBe('');
  });

  it('inicializa mapa apenas quando função, mapa e campo existem', () => {
    document.body.innerHTML = '<div id="mapa"></div><input id="endereco">';
    window.initMapSelector = vi.fn();
    runPublicScript('form-utils.js');

    window.initMapSafe('mapa', 'endereco');
    expect(window.initMapSelector).toHaveBeenCalledWith('mapa', 'endereco');

    window.initMapSelector.mockClear();
    window.initMapSafe('mapa-inexistente', 'endereco');
    expect(window.initMapSelector).not.toHaveBeenCalled();
  });

  it('configura automaticamente o campo DtContato presente no carregamento do script', () => {
    document.body.innerHTML = '<input id="DtContato" type="date" value="2026-08-25">';
    runPublicScript('form-utils.js');
    const input = document.getElementById('DtContato');

    expect(input.max).toBe('2026-08-24');
    expect(input.checkValidity()).toBe(false);
  });
});

describe('campo de telefone compartilhado', () => {
  it('não altera o valor inicial nem o digitado e tolera páginas sem telefone', () => {
    document.body.innerHTML = '<input id="inputTelefone" value="85987654321">';
    runPublicScript('site.js');
    const input = document.getElementById('inputTelefone');
    expect(input.value).toBe('85987654321');

    input.value = '85 3333 2222';
    input.dispatchEvent(new Event('input'));
    expect(input.value).toBe('85 3333 2222');

    document.body.innerHTML = '';
    expect(() => runPublicScript('site.js')).not.toThrow();
  });
});

describe('telefone internacional de atores', () => {
  it('não presume país e preserva um código explícito', () => {
    document.body.innerHTML = '<input id="inputTelefone" value="85987654321">';
    runPublicScript('site.js');
    const input = document.getElementById('inputTelefone');
    expect(input.value).toBe('85987654321');

    input.value = '+(1) 2025550199';
    input.dispatchEvent(new Event('input'));
    expect(input.value).toBe('+(1) 2025550199');
  });
});

describe('inicialização da página de ficha', () => {
  it('encadeia telefone, data, mapa e estado da página', () => {
    window.initTelefoneMask = vi.fn();
    window.initDateMaxToday = vi.fn();
    window.initMapSafe = vi.fn();
    window.setPageState = vi.fn();
    runPublicScript('fichaprimeirocontato.js');

    window.pageInit();
    expect(window.initTelefoneMask).toHaveBeenCalledWith('inputTelefone');
    expect(window.initDateMaxToday).toHaveBeenCalledWith('DtContato');
    expect(window.initMapSafe).toHaveBeenCalledWith('mapa-principal', 'input-endereco');
    expect(window.setPageState).toHaveBeenCalledOnce();
  });

  it('dispara pageInit ao DOM estar pronto', () => {
    window.pageInit = vi.fn();
    runPublicScript('page-init.js');
    dispatchReady();
    expect(window.pageInit).toHaveBeenCalledOnce();
  });
});
