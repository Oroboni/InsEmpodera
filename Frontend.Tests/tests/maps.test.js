import { beforeEach, describe, expect, it, vi } from 'vitest';
import { flushPromises, readProjectFile, runPublicScript } from './helpers/project.js';

function leafletDouble() {
  const handlers = {};
  const map = {
    setView: vi.fn(function () { return this; }),
    fitBounds: vi.fn(),
    removeLayer: vi.fn(),
    remove: vi.fn(),
    on: vi.fn((event, callback) => { handlers[event] = callback; return map; }),
    dragging: { disable: vi.fn() },
    touchZoom: { disable: vi.fn() },
    doubleClickZoom: { disable: vi.fn() },
    scrollWheelZoom: { disable: vi.fn() }
  };
  const marker = {
    addTo: vi.fn(() => marker),
    setLatLng: vi.fn(() => marker)
  };
  const control = {
    handlers: {},
    on: vi.fn((event, callback) => { control.handlers[event] = callback; return control; }),
    addTo: vi.fn(() => control)
  };
  const geocoderFactory = vi.fn(options => {
    control.options = options;
    return control;
  });
  const L = {
    map: vi.fn(() => map),
    tileLayer: vi.fn(() => ({ addTo: vi.fn() })),
    marker: vi.fn(() => marker),
    polygon: vi.fn(() => ({ getBounds: vi.fn(() => 'bounds') })),
    latLng: vi.fn((lat, lng) => ({ lat, lng })),
    latLngBounds: vi.fn((southWest, northEast) => ({
      southWest,
      northEast,
      getSouthEast: () => ({ lat: southWest.lat, lng: northEast.lng }),
      getNorthEast: () => northEast,
      getNorthWest: () => ({ lat: northEast.lat, lng: southWest.lng }),
      getSouthWest: () => southWest
    })),
    Control: { geocoder: geocoderFactory }
  };
  return { L, map, marker, control, handlers, geocoderFactory };
}

function mapFixture({ manual = false, mirror = false, value = '' } = {}) {
  document.body.innerHTML = `
    <div id="mapa"></div>
    <input id="endereco" value="${value}">
    ${manual ? `<textarea id="manual">${value}</textarea>` : ''}
    ${mirror ? '<input id="espelho">' : ''}
  `;
}

describe('integração de mapas', () => {
  let fake;

  beforeEach(() => {
    fake = leafletDouble();
    window.L = fake.L;
    window.fetch = vi.fn();
    runPublicScript('app-maps.js');
  });

  it('não inicializa Leaflet quando o contêiner não existe', () => {
    document.body.innerHTML = '<input id="endereco">';
    expect(() => window.initMapSelector('mapa', 'endereco')).not.toThrow();
    expect(fake.L.map).not.toHaveBeenCalled();
  });

  it('inicializa centro, zoom, camada, controle e registro global', () => {
    mapFixture();
    window.initMapSelector('mapa', 'endereco');
    expect(fake.L.map).toHaveBeenCalledWith('mapa');
    expect(fake.map.setView).toHaveBeenCalledWith([-14.235, -51.9253], 4);
    expect(fake.L.tileLayer).toHaveBeenCalledWith(
      'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
      { attribution: '&copy; OpenStreetMap' }
    );
    expect(fake.geocoderFactory).toHaveBeenCalledWith(expect.objectContaining({
      defaultMarkGeocode: false,
      placeholder: 'Buscar endereço...'
    }));
    expect(window.__empoderaMaps.mapa).toBe(fake.map);
  });

  it('respeita coordenadas e zoom configurados', () => {
    mapFixture();
    window.initMapSelector('mapa', 'endereco', { lat: -3.7, lng: -38.5, zoom: 12 });
    expect(fake.map.setView).toHaveBeenCalledWith([-3.7, -38.5], 12);
  });

  it('remove instância antiga antes de reconstruir o mesmo mapa', () => {
    mapFixture();
    const oldMap = { remove: vi.fn() };
    window.__empoderaMaps = { mapa: oldMap };
    document.getElementById('mapa')._leaflet_id = 99;
    document.getElementById('mapa').innerHTML = '<span>antigo</span>';
    window.initMapSelector('mapa', 'endereco');
    expect(oldMap.remove).toHaveBeenCalledOnce();
    expect(document.getElementById('mapa').innerHTML).toBe('');
    expect(window.__empoderaMaps.mapa).toBe(fake.map);
  });

  it('desabilita interações e não registra clique no modo somente leitura', () => {
    mapFixture();
    window.initMapSelector('mapa', 'endereco', true);
    expect(fake.map.dragging.disable).toHaveBeenCalledOnce();
    expect(fake.map.touchZoom.disable).toHaveBeenCalledOnce();
    expect(fake.map.doubleClickZoom.disable).toHaveBeenCalledOnce();
    expect(fake.map.scrollWheelZoom.disable).toHaveBeenCalledOnce();
    expect(fake.map.on).not.toHaveBeenCalledWith('click', expect.any(Function));
  });

  it('move um único marcador a cada clique no mapa', () => {
    mapFixture();
    window.initMapSelector('mapa', 'endereco');
    const point = { lat: -3.7, lng: -38.5 };
    fake.handlers.click({ latlng: point });
    fake.handlers.click({ latlng: { lat: -4, lng: -39 } });
    expect(fake.L.marker).toHaveBeenCalledTimes(1);
    expect(fake.marker.setLatLng).toHaveBeenNthCalledWith(1, point);
    expect(fake.marker.setLatLng).toHaveBeenNthCalledWith(2, { lat: -4, lng: -39 });
  });

  it('aplica resultado do controle ao alvo, espelho e campo manual', () => {
    mapFixture({ manual: true, mirror: true });
    window.initMapSelector('mapa', 'endereco', {
      manualInputId: 'manual', mirrorInputId: 'espelho'
    });
    const geocode = {
      name: 'Rua A, Fortaleza', center: { lat: -3.7, lng: -38.5 },
      bbox: {
        getSouthEast: () => 1, getNorthEast: () => 2,
        getNorthWest: () => 3, getSouthWest: () => 4
      }
    };
    fake.control.handlers.markgeocode({ geocode });
    expect(document.getElementById('endereco').value).toBe('Rua A, Fortaleza');
    expect(document.getElementById('espelho').value).toBe('Rua A, Fortaleza');
    expect(document.getElementById('manual').value).toBe('Rua A, Fortaleza');
    expect(fake.map.fitBounds).toHaveBeenCalledWith('bounds');
    expect(fake.marker.setLatLng).toHaveBeenCalledWith(geocode.center);
  });

  it('usa Nominatim, grava cache e evita nova chamada para a mesma busca', async () => {
    mapFixture();
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([{ lat: '-3.7', lon: '-38.5', display_name: 'Fortaleza, CE' }])
    });
    window.initMapSelector('mapa', 'endereco');
    const geocoder = fake.control.options.geocoder;
    const call = query => new Promise(resolve => geocoder.geocode(query, resolve));

    const first = await call('Fortaleza');
    const second = await call('Fortaleza');
    expect(first[0]).toEqual(expect.objectContaining({ name: 'Fortaleza, CE' }));
    expect(second[0]).toEqual(expect.objectContaining({ name: 'Fortaleza, CE' }));
    expect(window.fetch).toHaveBeenCalledTimes(1);
    expect(JSON.parse(localStorage.getItem('empodera_geocode_cache_v1')).Fortaleza)
      .toEqual({ lat: -3.7, lon: -38.5, name: 'Fortaleza, CE' });
  });

  it('faz fallback para ArcGIS quando Nominatim falha', async () => {
    mapFixture();
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    window.fetch = vi.fn()
      .mockRejectedValueOnce(new Error('Nominatim offline'))
      .mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue({
          candidates: [{ location: { y: -3.7, x: -38.5 }, address: 'Fortaleza, CE' }]
        })
      });
    window.initMapSelector('mapa', 'endereco');
    const geocoder = fake.control.options.geocoder;
    const results = await new Promise(resolve => geocoder.geocode('Fortaleza', resolve));
    expect(results[0].name).toBe('Fortaleza, CE');
    expect(window.fetch).toHaveBeenCalledTimes(2);
    expect(window.fetch.mock.calls[1][0]).toContain('geocode.arcgis.com');
  });

  it('executa o fallback do endereço inicial mesmo sem controle de busca visível', async () => {
    mapFixture({ value: 'Endereço sem resultado' });
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([])
    });

    expect(() => window.initMapSelector('mapa', 'endereco', { showSearchControl: false }))
      .not.toThrow();

    await vi.waitFor(() => expect(window.fetch).toHaveBeenCalledTimes(4));
    expect(fake.geocoderFactory).not.toHaveBeenCalled();
    expect(document.getElementById('endereco').value).toBe('Endereço sem resultado');
  });

  it('sincroniza texto digitado imediatamente e geocodifica no change', async () => {
    mapFixture({ manual: true, mirror: true });
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([{ lat: '-8.05', lon: '-34.9', display_name: 'Recife, PE' }])
    });
    window.initMapSelector('mapa', 'endereco', {
      manualInputId: 'manual', mirrorInputId: 'espelho', showSearchControl: false
    });
    const manual = document.getElementById('manual');
    manual.value = 'Recife';
    manual.dispatchEvent(new Event('input'));
    expect(document.getElementById('endereco').value).toBe('Recife');

    manual.dispatchEvent(new Event('change'));
    await flushPromises();
    await flushPromises();
    expect(document.getElementById('endereco').value).toBe('Recife, PE');
    expect(fake.marker.setLatLng).toHaveBeenCalledWith({ lat: -8.05, lng: -34.9 });
  });

  it('renderiza sugestões deduplicadas e aplica a escolhida', async () => {
    vi.useFakeTimers();
    mapFixture({ manual: true, mirror: true });
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([
        { lat: '-3.7', lon: '-38.5', display_name: 'Rua A, Centro, Fortaleza, Brasil' },
        { lat: '-3.7', lon: '-38.5', display_name: 'Rua A, Centro, Fortaleza, Brasil' }
      ])
    });
    window.initMapSelector('mapa', 'endereco', {
      manualInputId: 'manual', mirrorInputId: 'espelho', showSearchControl: false
    });
    const manual = document.getElementById('manual');
    manual.value = 'Rua A';
    manual.dispatchEvent(new Event('input'));
    await vi.advanceTimersByTimeAsync(250);
    await flushPromises();

    const list = document.querySelector('.address-suggestions');
    expect(list.hidden).toBe(false);
    expect(list.querySelectorAll('.address-suggestion-item')).toHaveLength(1);
    expect(list.querySelector('.address-suggestion-primary').textContent).toBe('Rua A, Centro');
    expect(list.querySelector('.address-suggestion-secondary').textContent).toBe('Fortaleza, Brasil');

    list.querySelector('.address-suggestion-item').click();
    await flushPromises();
    expect(document.getElementById('endereco').value).toBe('Rua A, Centro, Fortaleza, Brasil');
    expect(document.getElementById('espelho').value).toBe('Rua A, Centro, Fortaleza, Brasil');
    expect(manual.value).toBe('Rua A, Centro, Fortaleza, Brasil');
    expect(list.hidden).toBe(true);
  });

  it('oculta sugestões com Escape e clique fora', async () => {
    mapFixture({ manual: true });
    window.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue([{ lat: '1', lon: '2', display_name: 'Lugar, País' }])
    });
    window.initMapSelector('mapa', 'endereco', { manualInputId: 'manual', showSearchControl: false });
    const manual = document.getElementById('manual');
    manual.value = 'Lugar';
    manual.dispatchEvent(new FocusEvent('focus'));
    await flushPromises();
    const list = document.querySelector('.address-suggestions');
    expect(list.hidden).toBe(false);

    manual.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(list.hidden).toBe(true);

    manual.dispatchEvent(new FocusEvent('focus'));
    await flushPromises();
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(list.hidden).toBe(true);
  });

  it('possui uma referência válida para o fallback do plugin de geocodificação', () => {
    const source = readProjectFile('wwwroot/js/app-maps.js');
    const usesPluginFallback = source.includes('geocoder.options.geocoder.geocode');
    const declaresGeocoder = /(?:const|let|var)\s+geocoder\s*=/.test(source);
    expect(usesPluginFallback && !declaresGeocoder).toBe(false);
  });
});
