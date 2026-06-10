/**
 * Inicializa um mapa com busca e seleção de endereço.
 * @param {string} mapId - O ID da div onde o mapa ficará.
 * @param {string} inputId - O ID do input que receberá o endereço texto.
 * @param {object|boolean} options - (Opcional) { lat, lng, zoom, readOnly, sourceInputId, mirrorInputId, manualInputId } ou apenas true para readOnly.
 */
function initMapSelector(mapId, inputId, options = {}) {
    if (typeof options === "boolean") {
        options = { readOnly: options };
    }

    // Configurações padrão (Centro do Brasil).
    const defaultLatitude = -14.2350;
    const defaultLongitude = -51.9253;
    const latitude = options.lat || defaultLatitude;
    const longitude = options.lng || defaultLongitude;
    const startZoom = options.zoom || 4;
    const isReadOnly = options.readOnly || false;
    const sourceInputId = options.sourceInputId || inputId;
    const mirrorInputId = options.mirrorInputId || null;
    const manualInputId = options.manualInputId || null;

    // Verifica se o elemento existe para evitar erros
    const mapElement = document.getElementById(mapId);
    if (!mapElement) return;

    if (!window.__empoderaMaps) {
        window.__empoderaMaps = {};
    }

    if (window.__empoderaMaps[mapId]) {
        window.__empoderaMaps[mapId].remove();
        delete window.__empoderaMaps[mapId];
    }

    if (mapElement._leaflet_id) {
        mapElement._leaflet_id = null;
        mapElement.innerHTML = "";
    }

    // 1. Inicializa o Mapa
    var map = L.map(mapId).setView([latitude, longitude], startZoom);
    window.__empoderaMaps[mapId] = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    // 2. Marcador
    var marker = null;

    const ensureMarker = () => {
        if (!marker) {
            marker = L.marker([latitude, longitude]).addTo(map);
        }
        return marker;
    };

    const clearMarker = () => {
        if (marker) {
            map.removeLayer(marker);
            marker = null;
        }
    };

    const updateInputValue = (value) => {
        const inputField = document.getElementById(inputId);
        if (inputField) {
            inputField.value = value;
            inputField.dispatchEvent(new Event("change"));
            inputField.dispatchEvent(new Event("input"));
        }

        const mirrorInput = mirrorInputId ? document.getElementById(mirrorInputId) : null;
        if (mirrorInput && !mirrorInput.value.trim()) {
            mirrorInput.value = value;
            mirrorInput.dispatchEvent(new Event("change"));
            mirrorInput.dispatchEvent(new Event("input"));
        }
    };

    const applyGeocodeResult = (geocode, fitBounds = true) => {
        if (!geocode || !geocode.center) return;

        if (fitBounds && geocode.bbox) {
            var bbox = geocode.bbox;
            var poly = L.polygon([
                bbox.getSouthEast(),
                bbox.getNorthEast(),
                bbox.getNorthWest(),
                bbox.getSouthWest()
            ]);

            map.fitBounds(poly.getBounds());
        } else {
            map.setView(geocode.center, Math.max(startZoom, 13));
        }

        ensureMarker().setLatLng(geocode.center);
        updateInputValue(geocode.name || "");
    };

    const createFallbackQueries = (query) => {
        const cleaned = (query || "")
            .replace(/[“”"]/g, "")
            .replace(/[–—]/g, "-")
            .replace(/\([^)]*\)/g, " ")
            .replace(/\b[0-9A-Z]{4,}\+[0-9A-Z]{2,}\b/gi, " ")
            .replace(/\b\d{4,6}\b/g, " ")
            .replace(/\s+-\s+/g, ", ")
            .replace(/\s{2,}/g, " ")
            .replace(/\s+,/g, ",")
            .trim()
            .replace(/^,+|,+$/g, "");

        const queries = new Set();
        if (cleaned) {
            queries.add(cleaned);
        }

        const parts = cleaned
            .split(",")
            .map((part) => part.trim())
            .filter(Boolean);

        if (parts.length >= 2) {
            queries.add(parts.join(", "));
            queries.add(parts.slice(-2).join(", "));
        }

        if (parts.length >= 3) {
            queries.add(parts.slice(-3).join(", "));
        }

        if (parts.length >= 1) {
            const firstPartWithoutNumbers = parts[0].replace(/\d+/g, " ").replace(/\s{2,}/g, " ").trim();
            const trailingCountry = parts.at(-1);
            if (firstPartWithoutNumbers && trailingCountry && firstPartWithoutNumbers !== parts[0]) {
                queries.add(`${firstPartWithoutNumbers}, ${trailingCountry}`);
            }
        }

        const cityCountryMatch = cleaned.match(/([A-Za-zÀ-ÿ'’.\-\s]+),\s*([A-Za-zÀ-ÿ'’.\-\s]+)$/);
        if (cityCountryMatch) {
            queries.add(`${cityCountryMatch[1].trim()}, ${cityCountryMatch[2].trim()}`);
        }

        return Array.from(queries).filter(Boolean);
    };

    const geocodeWithFetch = async (query) => {
        const url = `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&q=${encodeURIComponent(query)}`;
        const response = await fetch(url, {
            headers: {
                Accept: "application/json"
            }
        });

        if (!response.ok) {
            throw new Error(`Erro ao geocodificar: ${response.status}`);
        }

        const results = await response.json();
        if (!Array.isArray(results) || results.length === 0) {
            return null;
        }

        const result = results[0];
        const lat = Number(result.lat);
        const lon = Number(result.lon);

        if (Number.isNaN(lat) || Number.isNaN(lon)) {
            return null;
        }

        return {
            center: L.latLng(lat, lon),
            name: result.display_name || query
        };
    };

    const geocodeInitialAddress = async (query) => {
        const queries = createFallbackQueries(query);

        for (const candidate of queries) {
            try {
                const result = await geocodeWithFetch(candidate);
                if (result) {
                    applyGeocodeResult(result, false);
                    return true;
                }
            } catch (error) {
                console.warn("Falha no fetch de geocodificação:", candidate, error);
            }
        }

        for (const candidate of queries) {
            const pluginResult = await new Promise((resolve) => {
                geocoder.options.geocoder.geocode(candidate, function (results) {
                    resolve(results && results.length > 0 ? results[0] : null);
                });
            });

            if (pluginResult) {
                applyGeocodeResult(pluginResult, false);
                return true;
            }
        }

        return false;
    };

    const geocodeManualAddress = async (query) => {
        const trimmedQuery = (query || "").trim();
        if (!trimmedQuery) {
            clearMarker();
            map.setView([defaultLatitude, defaultLongitude], startZoom);
            const hiddenInput = document.getElementById(inputId);
            if (hiddenInput) {
                hiddenInput.value = "";
            }
            return false;
        }

        const resolved = await geocodeInitialAddress(trimmedQuery);
        if (!resolved) {
            const hiddenInput = document.getElementById(inputId);
            if (hiddenInput) {
                hiddenInput.value = trimmedQuery;
            }
        }
        return resolved;
    };

    // Se for apenas visualização (ex: Detalhes), desativa controles
    if (isReadOnly) {
        map.dragging.disable();
        map.touchZoom.disable();
        map.doubleClickZoom.disable();
        map.scrollWheelZoom.disable();
    }

    // 3. Adiciona a Lupa de Busca (Geocoding)
    var geocoder = L.Control.geocoder({
        defaultMarkGeocode: false,
        placeholder: "Buscar endereço..."
    })
    .on('markgeocode', function(e) {
        applyGeocodeResult(e.geocode);
    })
    .addTo(map);

    const sourceInput = document.getElementById(sourceInputId);
    const targetInput = document.getElementById(inputId);
    const manualInput = manualInputId ? document.getElementById(manualInputId) : null;
    const initialQuery = sourceInput?.value?.trim() || targetInput?.value?.trim() || "";

    if (initialQuery) {
        geocodeInitialAddress(initialQuery);
    } else {
        clearMarker();
    }

    if (isReadOnly) {
        return;
    }

    if (manualInput) {
        let manualSearchTimeout = null;

        manualInput.addEventListener("input", () => {
            const typedValue = manualInput.value.trim();
            if (targetInput) {
                targetInput.value = typedValue;
            }

            if (manualSearchTimeout) {
                clearTimeout(manualSearchTimeout);
            }

            manualSearchTimeout = setTimeout(async () => {
                await geocodeManualAddress(typedValue);
            }, 500);
        });

        manualInput.addEventListener("change", async () => {
            const typedValue = manualInput.value.trim();
            if (targetInput) {
                targetInput.value = typedValue;
            }
            await geocodeManualAddress(typedValue);
        });

        manualInput.addEventListener("keydown", async (event) => {
            if (event.key === "Enter") {
                event.preventDefault();
                const typedValue = manualInput.value.trim();
                if (targetInput) {
                    targetInput.value = typedValue;
                }
                await geocodeManualAddress(typedValue);
            }
        });
    }

    // 4. Clique no mapa (Opcional: move o pino manualmente)
    map.on('click', function(e) {
        ensureMarker().setLatLng(e.latlng);
        // Nota: Para pegar o nome da rua pelo clique (Geocodificação Reversa)
        // seria necessário uma chamada API extra. Por enquanto, movemos só o pino visual.
    });
}
