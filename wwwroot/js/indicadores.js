// Seção "Indicadores" recolhível das telas de listagem.
// O título (botão [data-indicadores]) mostra/esconde os cartões logo abaixo dele.
// A preferência vale para todas as telas e fica salva neste navegador.
(function () {
    "use strict";

    const CHAVE = "empodera.indicadores";
    const CLASSE = "indicadores-recolhidos";

    function init() {
        const botoes = document.querySelectorAll("[data-indicadores]");
        if (!botoes.length) return;
        const raiz = document.documentElement;

        const sincronizar = () => {
            const recolhidos = raiz.classList.contains(CLASSE);
            botoes.forEach(botao => botao.setAttribute("aria-expanded", String(!recolhidos)));
        };

        botoes.forEach((botao, indice) => {
            const alvo = botao.nextElementSibling;
            if (alvo) {
                if (!alvo.id) alvo.id = `indicadores-${indice + 1}`;
                botao.setAttribute("aria-controls", alvo.id);
            }
            botao.addEventListener("click", () => {
                const recolhidos = raiz.classList.toggle(CLASSE);
                try { localStorage.setItem(CHAVE, recolhidos ? "recolhidos" : "visiveis"); } catch { /* sem armazenamento */ }
                sincronizar();
            });
        });

        sincronizar();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
