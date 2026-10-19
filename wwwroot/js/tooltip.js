// Tooltip global para elementos com [data-tooltip].
// Fica em position: fixed no <body>, então nunca é cortado por contêineres com overflow,
// e se reposiciona para caber na tela (abre para baixo e/ou desliza para os lados).
(function () {
    "use strict";

    const MARGEM = 8;      // distância mínima das bordas da janela
    const DISTANCIA = 10;  // distância entre o ícone e o balão
    let balao = null;
    let alvoAtual = null;

    function criarBalao() {
        balao = document.createElement("div");
        balao.className = "app-tooltip";
        balao.id = "app-tooltip";
        balao.setAttribute("role", "tooltip");
        balao.hidden = true;
        document.body.appendChild(balao);
    }

    function posicionar(alvo) {
        const r = alvo.getBoundingClientRect();
        const largura = balao.offsetWidth;
        const altura = balao.offsetHeight;
        const vw = document.documentElement.clientWidth;
        const vh = document.documentElement.clientHeight;

        // Em cima por padrão; embaixo se não couber.
        let emCima = r.top - altura - DISTANCIA >= MARGEM;
        if (!emCima && r.bottom + altura + DISTANCIA > vh - MARGEM) emCima = r.top > vh - r.bottom;
        const top = emCima ? r.top - altura - DISTANCIA : r.bottom + DISTANCIA;

        // Centralizado no ícone, mas sempre dentro da janela.
        const centro = r.left + r.width / 2;
        const left = Math.min(Math.max(centro - largura / 2, MARGEM), vw - largura - MARGEM);

        balao.style.top = `${Math.round(top)}px`;
        balao.style.left = `${Math.round(left)}px`;
        balao.dataset.lado = emCima ? "cima" : "baixo";
        // Seta continua apontando para o ícone mesmo quando o balão desliza.
        balao.style.setProperty("--seta-x", `${Math.round(Math.min(Math.max(centro - left, 14), largura - 14))}px`);
    }

    function mostrar(alvo) {
        const texto = alvo.getAttribute("data-tooltip");
        if (!texto) return;
        if (!balao) criarBalao();
        alvoAtual = alvo;
        balao.textContent = texto;
        balao.hidden = false;
        balao.classList.remove("is-visible");
        alvo.setAttribute("aria-describedby", balao.id);
        posicionar(alvo);
        requestAnimationFrame(() => balao.classList.add("is-visible"));
    }

    function esconder() {
        if (!balao || !alvoAtual) return;
        alvoAtual.removeAttribute("aria-describedby");
        alvoAtual = null;
        balao.classList.remove("is-visible");
        balao.hidden = true;
    }

    function alvoDe(evento) {
        return evento.target instanceof Element ? evento.target.closest("[data-tooltip]") : null;
    }

    function init() {
        document.documentElement.classList.add("js-tooltips");
        // Ícones de ajuda passam a ser alcançáveis pelo teclado.
        document.querySelectorAll(".tooltip-icon[data-tooltip]").forEach(el => {
            if (!el.hasAttribute("tabindex")) el.setAttribute("tabindex", "0");
            if (!el.hasAttribute("aria-label")) el.setAttribute("aria-label", "Ajuda");
        });

        document.addEventListener("mouseover", e => { const a = alvoDe(e); if (a && a !== alvoAtual) mostrar(a); });
        document.addEventListener("mouseout", e => {
            const a = alvoDe(e);
            if (a && a === alvoAtual && !a.contains(e.relatedTarget)) esconder();
        });
        document.addEventListener("focusin", e => { const a = alvoDe(e); if (a) mostrar(a); });
        document.addEventListener("focusout", e => { if (alvoDe(e) === alvoAtual) esconder(); });
        // Toque (celular): abre no toque e fecha ao tocar fora.
        document.addEventListener("click", e => {
            const a = alvoDe(e);
            if (a) {
                // Dentro de um <label>, o toque levaria o foco para o campo e fecharia o balão.
                if (a.closest("label")) e.preventDefault();
                mostrar(a);
            }
            else esconder();
        });
        document.addEventListener("keydown", e => { if (e.key === "Escape") esconder(); });
        window.addEventListener("scroll", () => { if (alvoAtual) posicionar(alvoAtual); }, true);
        window.addEventListener("resize", () => { if (alvoAtual) posicionar(alvoAtual); });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
