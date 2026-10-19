// Editor de cores (Configurações > Cores da plataforma, de cada usuário).
// - Carrega as cores em uso (tema base ou personalização salva).
// - Pré-visualiza na página inteira enquanto se edita (sem salvar).
// - Paletas prontas, gerador a partir de uma cor e aviso de contraste.
(function () {
    "use strict";

    const NEUTROS = {
        "color-gray-100": "#F7F7F7", "color-gray-200": "#F6F6F6", "color-gray-300": "#EEEEEE",
        "color-gray-400": "#CBD5E1", "color-gray-600": "#64748B", "color-gray-800": "#1E293B", "color-gray-900": "#0F172A"
    };
    const PRESETS = {
        verde: {
            "color-primary": "#2B5748", "color-primary-light": "#618764", "color-primary-dark": "#1E3F33",
            "color-primary-50": "#EEF3EB", "color-primary-100": "#DCE6D3", "color-primary-200": "#C2D2B3", "color-primary-300": "#9CB080",
            "color-primary-400": "#7E9A72", "color-primary-500": "#618764", "color-primary-600": "#2B5748", "color-primary-700": "#22463A",
            "color-primary-800": "#273338",
            "color-secondary": "#618764", "color-secondary-light": "#9CB080", "color-secondary-dark": "#2B5748",
            ...NEUTROS
        },
        roxo: {
            "color-primary": "#7A3188", "color-primary-light": "#9B4DB0", "color-primary-dark": "#5A1F66",
            "color-primary-50": "#F3E9F7", "color-primary-100": "#E0C8EF", "color-primary-200": "#CAA3E4", "color-primary-300": "#B17DD8",
            "color-primary-400": "#9B58CD", "color-primary-500": "#8640B5", "color-primary-600": "#6A2A73", "color-primary-700": "#51215A",
            "color-primary-800": "#3A153F",
            "color-secondary": "#6BC0B7", "color-secondary-light": "#9BE0DB", "color-secondary-dark": "#4A8680",
            ...NEUTROS
        }
    };

    // ---- utilitários de cor ----
    const hexValido = v => /^#[0-9a-f]{6}$/i.test(v);
    function normalizar(valor) {
        valor = (valor || "").trim();
        let m = valor.match(/^#([0-9a-f]{3})$/i);
        if (m) return "#" + m[1].split("").map(c => c + c).join("").toUpperCase();
        if (hexValido(valor)) return valor.toUpperCase();
        m = valor.match(/^rgba?\((\d+)[ ,]+(\d+)[ ,]+(\d+)/i);
        if (m) return "#" + [m[1], m[2], m[3]].map(n => (+n).toString(16).padStart(2, "0")).join("").toUpperCase();
        return null;
    }
    const paraRgb = hex => [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16));
    function paraHsl(hex) {
        let [r, g, b] = paraRgb(hex).map(v => v / 255);
        const max = Math.max(r, g, b), min = Math.min(r, g, b);
        let h = 0, s = 0; const l = (max + min) / 2;
        if (max !== min) {
            const d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            h = max === r ? (g - b) / d + (g < b ? 6 : 0) : max === g ? (b - r) / d + 2 : (r - g) / d + 4;
            h /= 6;
        }
        return [h * 360, s * 100, l * 100];
    }
    function deHsl(h, s, l) {
        s /= 100; l /= 100;
        const k = n => (n + h / 30) % 12, a = s * Math.min(l, 1 - l);
        const f = n => l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
        return "#" + [f(0), f(8), f(4)].map(x => Math.round(x * 255).toString(16).padStart(2, "0")).join("").toUpperCase();
    }
    function luminancia(hex) {
        const [r, g, b] = paraRgb(hex).map(v => { v /= 255; return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4; });
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
    function contraste(a, b) {
        const [l1, l2] = [luminancia(a), luminancia(b)].sort((x, y) => y - x);
        return (l1 + 0.05) / (l2 + 0.05);
    }

    function gerarPaleta(base) {
        const [h, s, l] = paraHsl(base);
        const sat = f => Math.min(100, Math.max(8, s * f));
        return {
            "color-primary": base,
            "color-primary-600": base,
            "color-primary-dark": deHsl(h, sat(1), Math.max(8, l * 0.72)),
            "color-primary-light": deHsl(h, sat(0.85), Math.min(70, l + 18)),
            "color-primary-50": deHsl(h, sat(0.45), 95),
            "color-primary-100": deHsl(h, sat(0.5), 89),
            "color-primary-200": deHsl(h, sat(0.55), 80),
            "color-primary-300": deHsl(h, sat(0.65), 68),
            "color-primary-400": deHsl(h, sat(0.75), 56),
            "color-primary-500": deHsl(h, sat(0.85), Math.min(60, l + 12)),
            "color-primary-700": deHsl(h, sat(1), Math.max(10, l * 0.82)),
            "color-primary-800": deHsl(h, sat(0.35), Math.min(22, Math.max(12, l * 0.6))),
            "color-secondary": deHsl(h, sat(0.7), Math.min(55, l + 15)),
            "color-secondary-light": deHsl(h, sat(0.6), 72),
            "color-secondary-dark": deHsl(h, sat(0.9), Math.max(15, l))
        };
    }

    function init() {
        const form = document.getElementById("formCores");
        if (!form) return;
        const raiz = document.documentElement;
        const pickers = [...form.querySelectorAll(".color-picker")];
        const hexes = [...form.querySelectorAll(".color-hex")];
        const avisos = document.getElementById("avisosContraste");
        const estilo = getComputedStyle(raiz);
        const atual = {};

        hexes.forEach(campo => {
            const token = campo.dataset.token;
            atual[token] = normalizar(estilo.getPropertyValue("--" + token)) || "#000000";
        });

        function preVisualizar(token, valor) {
            raiz.style.setProperty("--" + token, valor);
            if (token.startsWith("color-primary")) raiz.style.setProperty(`--${token}-rgb`, paraRgb(valor).join(" "));
        }

        function definir(token, valor, aplicar = true) {
            const v = normalizar(valor);
            if (!v) return;
            const picker = pickers.find(p => p.dataset.token === token);
            const hex = hexes.find(h => h.dataset.token === token);
            if (picker) picker.value = v.toLowerCase();
            if (hex) { hex.value = v; hex.classList.remove("is-invalid"); }
            if (aplicar) preVisualizar(token, v);
        }

        function verificarContraste() {
            const valor = t => hexes.find(h => h.dataset.token === t)?.value;
            const regras = [
                ["color-primary", "#FFFFFF", "Texto branco sobre a cor Principal (botões)"],
                ["color-primary-600", "#FFFFFF", "Texto branco sobre o Menu lateral"],
                ["color-gray-800", valor("color-gray-200"), "Texto sobre o Fundo do conteúdo"],
                ["color-primary-800", valor("color-gray-200"), "Títulos sobre o Fundo do conteúdo"]
            ];
            const problemas = regras
                .map(([t, fundo, nome]) => ({ nome, c: hexValido(valor(t) || "") && hexValido(fundo || "") ? contraste(valor(t), fundo) : 99 }))
                .filter(r => r.c < 4.5);
            avisos.replaceChildren(...problemas.map(p => {
                const li = document.createElement("li");
                li.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i> ';
                li.append(`${p.nome}: contraste ${p.c.toFixed(1)}:1 (recomendado 4,5:1 ou mais). Pode ficar difícil de ler.`);
                return li;
            }));
            avisos.hidden = problemas.length === 0;
        }

        Object.entries(atual).forEach(([t, v]) => definir(t, v, false));
        verificarContraste();

        pickers.forEach(p => p.addEventListener("input", () => { definir(p.dataset.token, p.value); verificarContraste(); }));
        hexes.forEach(h => h.addEventListener("input", () => {
            const v = normalizar(h.value);
            h.classList.toggle("is-invalid", !v);
            if (v && h.value.length >= 7) { definir(h.dataset.token, v); verificarContraste(); }
        }));

        document.querySelectorAll("[data-preset]").forEach(botao => botao.addEventListener("click", () => {
            Object.entries(PRESETS[botao.dataset.preset] || {}).forEach(([t, v]) => definir(t, v));
            verificarContraste();
        }));

        const corBase = document.getElementById("corBase");
        if (corBase) corBase.value = (atual["color-primary"] || "#2B5748").toLowerCase();
        document.getElementById("gerarPaleta")?.addEventListener("click", () => {
            Object.entries(gerarPaleta(normalizar(corBase.value))).forEach(([t, v]) => definir(t, v));
            verificarContraste();
        });

        document.getElementById("descartarCores")?.addEventListener("click", () => {
            raiz.removeAttribute("style");
            Object.entries(atual).forEach(([t, v]) => definir(t, v, false));
            verificarContraste();
        });

        form.addEventListener("submit", e => {
            const invalido = hexes.find(h => !hexValido(h.value));
            if (invalido) {
                e.preventDefault();
                invalido.classList.add("is-invalid");
                invalido.focus();
            }
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
