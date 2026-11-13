// Assistente da ficha: só avança com a etapa preenchida, leva ao campo com erro e filtra atores pela comunidade.
if (typeof window.currentStep === "undefined") {
    window.currentStep = 1;
}

let totalSteps = document.querySelectorAll(".step-content").length;

const textosWizard = {
    pt: { obrigatorio: "Preencha este campo.", futura: "A data não pode ser futura.", invalido: "Valor inválido.", sair: "Sair da ficha", voltar: "Voltar", proximo: "Próximo", semAtores: "Nenhum ator ligado a esta comunidade" },
    en: { obrigatorio: "Please fill in this field.", futura: "The date cannot be in the future.", invalido: "Invalid value.", sair: "Leave form", voltar: "Back", proximo: "Next", semAtores: "No actors linked to this community" },
    es: { obrigatorio: "Complete este campo.", futura: "La fecha no puede ser futura.", invalido: "Valor no válido.", sair: "Salir de la ficha", voltar: "Volver", proximo: "Siguiente", semAtores: "Ningún actor vinculado a esta comunidad" }
};
const tw = textosWizard[(document.documentElement.lang || "pt").slice(0, 2)] || textosWizard.pt;

function stepElement(step) {
    return document.querySelectorAll(".step-content")[step - 1];
}

function mensagemDoCampo(campo) {
    if (campo.validity.valueMissing) return tw.obrigatorio;
    if (campo.validity.rangeOverflow && campo.type === "date") return tw.futura;
    return campo.validationMessage || tw.invalido;
}

function marcarCampo(campo, mensagem) {
    const card = campo.closest(".input-card, .form-group");
    const span = campo.form?.querySelector(`[data-valmsg-for="${campo.name}"]`);
    campo.classList.toggle("input-validation-error", !!mensagem);
    card?.classList.toggle("campo-com-erro", !!mensagem);
    if (span) {
        span.textContent = mensagem || "";
        span.classList.toggle("field-validation-error", !!mensagem);
        span.classList.toggle("field-validation-valid", !mensagem);
    }
}

// Valida os campos da etapa (ou do formulário inteiro); devolve o primeiro inválido.
function validarCampos(container) {
    let primeiro = null;
    const radiosVistos = new Set();
    container.querySelectorAll("input, select, textarea").forEach((campo) => {
        if (campo.disabled || !campo.willValidate || campo.type === "hidden") return;
        if (campo.type === "radio") {
            if (radiosVistos.has(campo.name)) return;
            radiosVistos.add(campo.name);
        }
        const valido = campo.checkValidity();
        marcarCampo(campo, valido ? "" : mensagemDoCampo(campo));
        if (!valido && !primeiro) primeiro = campo;
    });
    return primeiro;
}

function irParaCampo(campo) {
    const etapa = campo.closest(".step-content");
    const indice = Array.from(document.querySelectorAll(".step-content")).indexOf(etapa) + 1;
    if (indice > 0 && indice !== window.currentStep) {
        window.currentStep = indice;
        showStep(indice);
        updateButtonVisibility();
    }
    campo.closest(".input-card, .form-group")?.scrollIntoView({ behavior: "smooth", block: "center" });
    campo.focus({ preventScroll: true });
}

function showStep(step) {
    document.querySelectorAll(".step-content").forEach((el, index) => {
        el.classList.toggle("active", index + 1 === step);
    });
    document.querySelectorAll(".step").forEach((el, index) => {
        el.classList.toggle("active", index + 1 <= step);
    });
}

function changeStep(direction) {
    const next = window.currentStep + direction;
    if (next < 1 || next > totalSteps) return;

    if (direction > 0) {
        const invalido = validarCampos(stepElement(window.currentStep));
        if (invalido) {
            irParaCampo(invalido);
            return;
        }
    }

    window.currentStep = next;
    showStep(window.currentStep);
    updateButtonVisibility();
    document.querySelector(".stepper-container")?.scrollIntoView({ behavior: "smooth", block: "start" });
}

// Monta o conteúdo do botão com nós de texto (sem innerHTML com variáveis).
function rotuloBotao(botao, texto, icone, iconeDepois = false) {
    const i = document.createElement("i");
    i.className = `fa-solid ${icone}`;
    i.setAttribute("aria-hidden", "true");
    const t = document.createTextNode(iconeDepois ? `${texto} ` : ` ${texto}`);
    botao.replaceChildren(...(iconeDepois ? [t, i] : [i, t]));
}

function updateButtonVisibility() {
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const btnSave = document.getElementById("btn-save");

    if (!btnPrev || !btnNext) return;

    btnPrev.style.display = "inline-flex";
    btnNext.style.display = "inline-flex";

    if (window.currentStep === 1) {
        rotuloBotao(btnPrev, tw.sair, "fa-arrow-left");
        btnPrev.onclick = () => window.history.back();
    } else {
        rotuloBotao(btnPrev, tw.voltar, "fa-arrow-left");
        btnPrev.onclick = () => changeStep(-1);
    }

    if (window.currentStep === totalSteps) {
        btnNext.style.display = "none";
        btnSave && (btnSave.style.display = "inline-flex");
    } else {
        rotuloBotao(btnNext, tw.proximo, "fa-arrow-right", true);
        btnNext.onclick = () => changeStep(1);
        btnSave && (btnSave.style.display = "none");
    }
}

// Atores: mostra só os ligados à comunidade escolhida (mantém o já selecionado).
function configurarFiltroDeAtores() {
    document.querySelectorAll("select[data-filtro-comunidade]").forEach((seletorAtor) => {
        const seletorComunidade = document.querySelector(seletorAtor.dataset.filtroComunidade);
        if (!seletorComunidade) return;
        const opcoes = Array.from(seletorAtor.options).filter((o) => o.value);
        const aviso = document.createElement("option");
        aviso.value = "";
        aviso.disabled = true;
        aviso.textContent = tw.semAtores;

        const aplicar = () => {
            const comunidade = seletorComunidade.value;
            let visiveis = 0;
            opcoes.forEach((opcao) => {
                const lista = (opcao.dataset.comunidades || "").split(",").filter(Boolean);
                const mostrar = !comunidade || lista.includes(comunidade);
                opcao.hidden = !mostrar;
                opcao.disabled = !mostrar;
                if (mostrar) visiveis++;
            });
            const atual = seletorAtor.selectedOptions[0];
            if (atual && atual.value && atual.disabled) seletorAtor.value = "";
            if (comunidade && visiveis === 0) seletorAtor.appendChild(aviso);
            else aviso.remove();
        };
        seletorComunidade.addEventListener("change", aplicar);
        aplicar();
    });
}

document.addEventListener("DOMContentLoaded", () => {
    configurarFiltroDeAtores();

    const form = document.getElementById("wizardForm");
    if (form) {
        // Corrige o campo assim que a pessoa mexe nele.
        form.addEventListener("change", (ev) => {
            const campo = ev.target;
            if (campo.classList?.contains("input-validation-error") || campo.type === "radio") {
                const grupo = campo.type === "radio" ? form.querySelector(`[name="${campo.name}"]`) : campo;
                marcarCampo(grupo, grupo.checkValidity() ? "" : mensagemDoCampo(grupo));
            }
        });
        // Usado pelo modo de edição (edit-mode.js), que envia com form.submit().
        window.validarFormularioFicha = () => {
            const invalido = validarCampos(form);
            if (invalido) irParaCampo(invalido);
            return !invalido;
        };
        // No envio, confere todas as etapas (as ocultas o navegador ignora) e leva ao primeiro erro.
        form.addEventListener("submit", (ev) => {
            const invalido = validarCampos(form);
            if (invalido) {
                ev.preventDefault();
                ev.stopImmediatePropagation();
                irParaCampo(invalido);
            }
        }, true);
    }

    if (totalSteps === 0) return;

    // Voltando do servidor com erro: abre na etapa do primeiro campo marcado.
    const campoComErro = document.querySelector(".step-content .input-validation-error:not([type=hidden])");
    if (campoComErro) {
        const etapa = campoComErro.closest(".step-content");
        window.currentStep = Array.from(document.querySelectorAll(".step-content")).indexOf(etapa) + 1 || 1;
    }

    showStep(window.currentStep);
    updateButtonVisibility();
});
