/**
 * Inicializa um mapa com busca e seleção de endereço.
 * @param {string} mapId - O ID da div onde o mapa ficará.
 * @param {string} inputId - O ID do input que receberá o endereço texto.
 * @param {object} options - (Opcional) { lat, lng, zoom, readOnly }
 */
function initMapSelector(mapId, inputId, options = {}) {
    
    // Configurações padrão (Centro do Brasil).
    const startLat = options.lat || -14.2350;
    const startLng = options.lng || -51.9253;
    const startZoom = options.zoom || 4;
    const isReadOnly = options.readOnly || false;

    // Verifica se o elemento existe para evitar erros
    if (!document.getElementById(mapId)) return;

    // 1. Inicializa o Mapa
    var map = L.map(mapId).setView([startLat, startLng], startZoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    // 2. Marcador
    var marker = L.marker([startLat, startLng]).addTo(map);

    // Se for apenas visualização (ex: Detalhes), desativa controles
    if (isReadOnly) {
        map.dragging.disable();
        map.touchZoom.disable();
        map.doubleClickZoom.disable();
        map.scrollWheelZoom.disable();
        return; // Para por aqui, não adiciona busca
    }

    // 3. Adiciona a Lupa de Busca (Geocoding)
    var geocoder = L.Control.geocoder({
        defaultMarkGeocode: false,
        placeholder: "Buscar endereço..."
    })
    .on('markgeocode', function(e) {
        var bbox = e.geocode.bbox;
        var poly = L.polygon([
            bbox.getSouthEast(),
            bbox.getNorthEast(),
            bbox.getNorthWest(),
            bbox.getSouthWest()
        ]);

        map.fitBounds(poly.getBounds());
        marker.setLatLng(e.geocode.center);

        // Preenche o Input automaticamente
        const inputField = document.getElementById(inputId);
        if (inputField) {
            inputField.value = e.geocode.name;
            // Dispara evento de 'change' caso tenha validação ou outros scripts ouvindo
            inputField.dispatchEvent(new Event('change')); 
        }
    })
    .addTo(map);

    // 4. Clique no mapa (Opcional: move o pino manualmente)
    map.on('click', function(e) {
        marker.setLatLng(e.latlng);
        // Nota: Para pegar o nome da rua pelo clique (Geocodificação Reversa)
        // seria necessário uma chamada API extra. Por enquanto, movemos só o pino visual.
    });
}