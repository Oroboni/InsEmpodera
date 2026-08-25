/**
 * Inicializa um mapa com busca e seleção de endereço.
 * @param {string} mapId - O ID da div onde o mapa ficará.
 * @param {string} inputId - O ID do input que receberá o endereço texto.
 * @param {object|boolean} options - (Opcional) { lat, lng, zoom, readOnly, sourceInputId, mirrorInputId, manualInputId, showSearchControl } ou apenas true para readOnly.
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
    const showSearchControl = options.showSearchControl !== false;
    const geocodeCacheKey = "empodera_geocode_cache_v1";

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

    let isSyncingManualInput = false;

    const autoResizeField = (field) => {
        if (!field || field.tagName !== "TEXTAREA") {
            return;
        }

        field.style.height = "auto";
        field.style.height = `${field.scrollHeight}px`;
    };

    const setFieldValue = (field, value, emitEvents = false) => {
        if (!field) {
            return;
        }

        field.value = value;
        autoResizeField(field);

        if (emitEvents) {
            field.dispatchEvent(new Event("change"));
            field.dispatchEvent(new Event("input"));
        }
    };

    const updateInputValue = (value, syncMirror = false, syncManual = false) => {
        const inputField = document.getElementById(inputId);
        setFieldValue(inputField, value);

        if (syncMirror && mirrorInputId) {
            const mirrorInput = document.getElementById(mirrorInputId);
            setFieldValue(mirrorInput, value);
        }

        if (syncManual && manualInputId) {
            const manualInput = document.getElementById(manualInputId);
            setFieldValue(manualInput, value);
        }
    };

    const applyGeocodeResult = (geocode, fitBounds = true, syncMirror = false, syncManual = false) => {
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
        updateInputValue(geocode.name || "", syncMirror, syncManual);
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

    const readGeocodeCache = () => {
        try {
            return JSON.parse(localStorage.getItem(geocodeCacheKey) || "{}");
        } catch {
            return {};
        }
    };

    const writeGeocodeCache = (cache) => {
        try {
            localStorage.setItem(geocodeCacheKey, JSON.stringify(cache));
        } catch {
            // Ignora erro de quota/localStorage indisponível
        }
    };

    const getCachedGeocode = (query) => {
        const cache = readGeocodeCache();
        const cached = cache[query];
        if (!cached) {
            return null;
        }

        return {
            center: L.latLng(cached.lat, cached.lon),
            name: cached.name || query
        };
    };

    const saveCachedGeocode = (query, result) => {
        if (!result?.center) {
            return;
        }

        const cache = readGeocodeCache();
        cache[query] = {
            lat: result.center.lat,
            lon: result.center.lng,
            name: result.name || query
        };
        writeGeocodeCache(cache);
    };

    const buildGeocodeResult = (lat, lon, name) => {
        if (Number.isNaN(lat) || Number.isNaN(lon)) {
            return null;
        }

        return {
            center: L.latLng(lat, lon),
            name: name || "",
            bbox: L.latLngBounds(
                L.latLng(lat - 0.01, lon - 0.01),
                L.latLng(lat + 0.01, lon + 0.01)
            )
        };
    };

    const geocodeWithNominatim = async (query, limit = 1) => {
        const url = `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=${limit}&q=${encodeURIComponent(query)}`;
        const response = await fetch(url, {
            headers: {
                Accept: "application/json"
            }
        });

        if (!response.ok) {
            throw new Error(`Nominatim ${response.status}`);
        }

        const results = await response.json();
        if (!Array.isArray(results) || results.length === 0) {
            return [];
        }

        return results
            .map((result) => buildGeocodeResult(
                Number(result.lat),
                Number(result.lon),
                result.display_name || query
            ))
            .filter(Boolean);
    };

    const geocodeWithArcGis = async (query, limit = 1) => {
        const url = `https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates?f=pjson&singleLine=${encodeURIComponent(query)}&maxLocations=${limit}&outFields=Match_addr`;
        const response = await fetch(url, {
            headers: {
                Accept: "application/json"
            }
        });

        if (!response.ok) {
            throw new Error(`ArcGIS ${response.status}`);
        }

        const data = await response.json();
        if (!Array.isArray(data?.candidates) || data.candidates.length === 0) {
            return [];
        }

        return data.candidates
            .map((result) => buildGeocodeResult(
                Number(result.location?.y),
                Number(result.location?.x),
                result.address || query
            ))
            .filter(Boolean);
    };

    const geocodeWithFetch = async (query) => {
        const cachedResult = getCachedGeocode(query);
        if (cachedResult) {
            return cachedResult;
        }

        const providers = [geocodeWithNominatim, geocodeWithArcGis];

        for (const provider of providers) {
            try {
                const results = await provider(query, 1);
                const result = Array.isArray(results) ? results[0] : results;
                if (result) {
                    saveCachedGeocode(query, result);
                    return result;
                }
            } catch (error) {
                console.warn("Falha de geocodificação:", query, error);
            }
        }

        return null;
    };

    const fetchSuggestions = async (query, limit = 5) => {
        const trimmedQuery = (query || "").trim();
        if (!trimmedQuery) {
            return [];
        }

        const queries = createFallbackQueries(trimmedQuery);
        const collected = [];
        const seenNames = new Set();

        const appendResults = (results) => {
            for (const result of results) {
                if (!result?.name || seenNames.has(result.name)) {
                    continue;
                }

                seenNames.add(result.name);
                collected.push(result);

                if (collected.length >= limit) {
                    break;
                }
            }
        };

        for (const candidate of queries) {
            try {
                appendResults(await geocodeWithNominatim(candidate, limit));
            } catch (error) {
                console.warn("Sugestão Nominatim falhou:", candidate, error);
            }

            if (collected.length >= limit) {
                break;
            }

            try {
                appendResults(await geocodeWithArcGis(candidate, limit));
            } catch (error) {
                console.warn("Sugestão ArcGIS falhou:", candidate, error);
            }

            if (collected.length >= limit) {
                break;
            }
        }

        return collected.slice(0, limit);
    };

    const geocodeInitialAddress = async (query) => {
        const queries = createFallbackQueries(query);

        for (const candidate of queries) {
            const result = await geocodeWithFetch(candidate);
            if (result) {
                applyGeocodeResult(result, false, false, false);
                return true;
            }
        }

        for (const candidate of queries) {
            const pluginResult = await new Promise((resolve) => {
                customGeocoder.geocode(candidate, function (results) {
                    resolve(results && results.length > 0 ? results[0] : null);
                });
            });

            if (pluginResult) {
                applyGeocodeResult(pluginResult, false, false, false);
                return true;
            }
        }

        return false;
    };

    const geocodeInitialCandidates = async (candidates) => {
        const uniqueCandidates = [...new Set((candidates || []).map((candidate) => (candidate || "").trim()).filter(Boolean))];

        for (const candidate of uniqueCandidates) {
            const resolved = await geocodeInitialAddress(candidate);
            if (resolved) {
                return true;
            }
        }

        return false;
    };

    const createSuggestionsList = (field) => {
        if (!field || !document.body) {
            return null;
        }

        const list = document.createElement("div");
        list.className = "address-suggestions";
        list.hidden = true;
        document.body.appendChild(list);
        return list;
    };

    const positionSuggestionsList = (field, list) => {
        if (!field || !list) {
            return;
        }

        const rect = field.getBoundingClientRect();
        const scrollTop = window.scrollY || document.documentElement.scrollTop || 0;
        const scrollLeft = window.scrollX || document.documentElement.scrollLeft || 0;

        list.style.top = `${rect.bottom + scrollTop + 8}px`;
        list.style.left = `${rect.left + scrollLeft}px`;
        list.style.width = `${rect.width}px`;
    };

    const splitSuggestionText = (value) => {
        const text = (value || "").trim();
        if (!text) {
            return { primary: "", secondary: "" };
        }

        const parts = text.split(",").map((part) => part.trim()).filter(Boolean);
        if (parts.length <= 1) {
            return { primary: text, secondary: "" };
        }

        return {
            primary: parts.slice(0, 2).join(", "),
            secondary: parts.slice(2).join(", ")
        };
    };

    const hideSuggestions = (list) => {
        if (!list) {
            return;
        }

        list.hidden = true;
        list.innerHTML = "";
    };

    const showSuggestions = (list) => {
        if (!list) {
            return;
        }

        list.hidden = false;
    };

    const geocodeManualAddress = async (query) => {
        const trimmedQuery = (query || "").trim();
        if (!trimmedQuery) {
            clearMarker();
            map.setView([defaultLatitude, defaultLongitude], startZoom);
            const hiddenInput = document.getElementById(inputId);
            if (hiddenInput) {
                setFieldValue(hiddenInput, "");
            }
            return false;
        }

        const resolved = await geocodeInitialAddress(trimmedQuery);
        if (!resolved) {
            const hiddenInput = document.getElementById(inputId);
            if (hiddenInput) {
                setFieldValue(hiddenInput, trimmedQuery);
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
    const customGeocoder = {
        geocode: async function(query, cb, context) {
            const result = await geocodeWithFetch(query);
            const results = result ? [result] : [];
            cb.call(context || this, results);
        },
        suggest: async function(query, cb, context) {
            const results = await fetchSuggestions(query, 5);
            cb.call(context || this, results);
        },
        reverse: function(_location, _scale, cb, context) {
            cb.call(context || this, []);
        }
    };

    if (showSearchControl) {
        L.Control.geocoder({
            defaultMarkGeocode: false,
            placeholder: "Buscar endereço...",
            geocoder: customGeocoder
        })
        .on('markgeocode', function(e) {
            isSyncingManualInput = true;
            applyGeocodeResult(e.geocode, true, true, true);
            isSyncingManualInput = false;
        })
        .addTo(map);
    }

    const sourceInput = document.getElementById(sourceInputId);
    const targetInput = document.getElementById(inputId);
    const manualInput = manualInputId ? document.getElementById(manualInputId) : null;
    autoResizeField(manualInput);
    const initialCandidates = [
        sourceInput?.value,
        manualInput?.value,
        targetInput?.value
    ];

    if (initialCandidates.some((value) => (value || "").trim())) {
        geocodeInitialCandidates(initialCandidates);
    } else {
        clearMarker();
    }

    if (isReadOnly) {
        return;
    }

    if (manualInput) {
        let manualSearchTimeout = null;
        let manualSuggestionsTimeout = null;
        const suggestionsList = createSuggestionsList(manualInput);

        const applySuggestion = async (suggestion) => {
            if (!suggestion) {
                return;
            }

            isSyncingManualInput = true;
            applyGeocodeResult(suggestion, true, true, true);
            isSyncingManualInput = false;
            hideSuggestions(suggestionsList);
        };

        const renderSuggestions = (suggestions) => {
            if (!suggestionsList) {
                return;
            }

            suggestionsList.innerHTML = "";

            if (!Array.isArray(suggestions) || suggestions.length === 0) {
                hideSuggestions(suggestionsList);
                return;
            }

            const header = document.createElement("div");
            header.className = "address-suggestions-header";
            header.textContent = "Sugestões de endereço";
            suggestionsList.appendChild(header);

            suggestions.forEach((suggestion) => {
                const button = document.createElement("button");
                button.type = "button";
                button.className = "address-suggestion-item";

                const { primary, secondary } = splitSuggestionText(suggestion.name || "");

                const primaryLine = document.createElement("span");
                primaryLine.className = "address-suggestion-primary";
                primaryLine.textContent = primary;

                button.appendChild(primaryLine);

                if (secondary) {
                    const secondaryLine = document.createElement("span");
                    secondaryLine.className = "address-suggestion-secondary";
                    secondaryLine.textContent = secondary;
                    button.appendChild(secondaryLine);
                }

                button.addEventListener("mousedown", (event) => {
                    event.preventDefault();
                });
                button.addEventListener("click", async () => {
                    await applySuggestion(suggestion);
                });
                suggestionsList.appendChild(button);
            });

            positionSuggestionsList(manualInput, suggestionsList);
            showSuggestions(suggestionsList);
        };

        const syncSuggestionsPosition = () => {
            if (suggestionsList?.hidden) {
                return;
            }

            positionSuggestionsList(manualInput, suggestionsList);
        };

        window.addEventListener("resize", syncSuggestionsPosition);
        window.addEventListener("scroll", syncSuggestionsPosition, true);

        manualInput.addEventListener("input", () => {
            if (isSyncingManualInput) {
                return;
            }

            const typedValue = manualInput.value.trim();
            if (targetInput) {
                setFieldValue(targetInput, typedValue);
            }

            if (manualSearchTimeout) {
                clearTimeout(manualSearchTimeout);
            }

            if (manualSuggestionsTimeout) {
                clearTimeout(manualSuggestionsTimeout);
            }

            manualSuggestionsTimeout = setTimeout(async () => {
                if (typedValue.length < 3) {
                    hideSuggestions(suggestionsList);
                    return;
                }

                const suggestions = await fetchSuggestions(typedValue, 5);
                renderSuggestions(suggestions);
            }, 250);

            manualSearchTimeout = setTimeout(async () => {
                await geocodeManualAddress(typedValue);
            }, 700);
        });

        manualInput.addEventListener("change", async () => {
            if (isSyncingManualInput) {
                return;
            }

            const typedValue = manualInput.value.trim();
            if (targetInput) {
                setFieldValue(targetInput, typedValue);
            }
            hideSuggestions(suggestionsList);
            await geocodeManualAddress(typedValue);
        });

        manualInput.addEventListener("keydown", async (event) => {
            if (event.key === "Enter") {
                event.preventDefault();

                if (isSyncingManualInput) {
                    return;
                }

                const typedValue = manualInput.value.trim();
                if (targetInput) {
                    setFieldValue(targetInput, typedValue);
                }
                hideSuggestions(suggestionsList);
                await geocodeManualAddress(typedValue);
            } else if (event.key === "Escape") {
                hideSuggestions(suggestionsList);
            }
        });

        manualInput.addEventListener("blur", () => {
            setTimeout(() => hideSuggestions(suggestionsList), 150);
        });

        manualInput.addEventListener("focus", async () => {
            const typedValue = manualInput.value.trim();
            if (typedValue.length < 3) {
                return;
            }

            const suggestions = await fetchSuggestions(typedValue, 5);
            renderSuggestions(suggestions);
        });

        document.addEventListener("click", (event) => {
            if (!suggestionsList || suggestionsList.hidden) {
                return;
            }

            if (event.target === manualInput || manualInput.contains(event.target) || suggestionsList.contains(event.target)) {
                return;
            }

            hideSuggestions(suggestionsList);
        });
    }

    // 4. Clique no mapa (Opcional: move o pino manualmente)
    map.on('click', function(e) {
        ensureMarker().setLatLng(e.latlng);
        // Nota: Para pegar o nome da rua pelo clique (Geocodificação Reversa)
        // seria necessário uma chamada API extra. Por enquanto, movemos só o pino visual.
    });
}
