// Foto de perfil (Usuários > Novo/Editar): pré-visualização, validação no navegador e remoção.
(function () {
    "use strict";
    const MAXIMO = 2 * 1024 * 1024;
    const TIPOS = ["image/jpeg", "image/png", "image/webp"];

    function init() {
        const caixa = document.querySelector("[data-user-photo]");
        if (!caixa) return;
        const entrada = caixa.querySelector("#fotoArquivo");
        const previa = caixa.querySelector("[data-photo-preview]");
        const remover = caixa.querySelector("[data-photo-remove]");
        const flagRemover = caixa.querySelector("[data-photo-remove-flag]");
        const erro = caixa.querySelector("[data-photo-error]");
        const rotulo = caixa.querySelector("[data-photo-choose-label]");
        const iniciaisOriginais = (previa.querySelector("[data-photo-initials]")?.textContent || "").trim();

        function mostrarImagem(src) {
            previa.replaceChildren();
            const img = document.createElement("img");
            img.src = src;
            img.alt = "Pré-visualização da foto";
            previa.appendChild(img);
        }

        function mostrarIniciais() {
            const nome = document.querySelector('input[name="Nome"]')?.value || "";
            const partes = nome.trim().split(/\s+/).filter(Boolean);
            const span = document.createElement("span");
            span.textContent = partes.length ? partes.slice(0, 2).map(p => p[0].toUpperCase()).join("") : (iniciaisOriginais || "?");
            previa.replaceChildren(span);
        }

        entrada.addEventListener("change", () => {
            erro.hidden = true;
            const arquivo = entrada.files?.[0];
            if (!arquivo) return;
            const problema = !TIPOS.includes(arquivo.type) ? "Envie uma imagem JPG, PNG ou WEBP."
                : arquivo.size > MAXIMO ? "A foto deve ter no máximo 2 MB." : null;
            if (problema) {
                erro.textContent = problema;
                erro.hidden = false;
                entrada.value = "";
                return;
            }
            // data: (e não blob:) porque a política de segurança do site só permite imagens 'self' e data:.
            const leitor = new FileReader();
            leitor.addEventListener("load", () => mostrarImagem(leitor.result));
            leitor.readAsDataURL(arquivo);
            flagRemover.value = "false";
            remover.hidden = false;
            rotulo.textContent = "Trocar foto";
        });

        remover.addEventListener("click", () => {
            entrada.value = "";
            flagRemover.value = "true";
            remover.hidden = true;
            rotulo.textContent = "Escolher foto";
            mostrarIniciais();
        });

        // Atualiza as iniciais enquanto o nome é digitado (quando não há foto).
        document.querySelector('input[name="Nome"]')?.addEventListener("input", () => {
            if (!previa.querySelector("img")) mostrarIniciais();
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
