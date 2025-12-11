// ==========================================
// FICHAPRIMEIROCONTATO.JS - UNIFICADO
// ==========================================

document.addEventListener("DOMContentLoaded", function () {
    // ==========================================
    // 1. INICIALIZAÇÃO CONDICIONAL
    // ==========================================

    // Verifica se estamos na página de listagem (Index)
    const isIndexPage = document.querySelector(".forms-list") !== null;

    // Verifica se estamos na página de criação/edição (Form Wizard)
    const isFormPage = document.getElementById("wizardForm") !== null;

    // ==========================================
    // 2. FUNÇÕES DE INDEX (LISTAGEM)
    // ==========================================
    if (isIndexPage) {
        initIndexPage();
    }

    // ==========================================
    // 3. FUNÇÕES DE FORMULÁRIO (CREATE/EDIT)
    // ==========================================
    if (isFormPage) {
        initFormPage();
    }

    // ==========================================
    // 4. FUNÇÕES COMUNS A TODAS AS PÁGINAS
    // ==========================================
    initCommonFeatures();
});

// ==========================================
// FUNÇÕES PARA PÁGINA DE LISTAGEM (INDEX)
// ==========================================

function initIndexPage() {
    const searchInput = document.getElementById("searchInput");
    const fichasContainer = document.getElementById("fichasContainer");
    const formItems = document.querySelectorAll(".form-item");
    const noRecordsMessage = document.querySelector(".no-records");
    const comunidadeFilter = document.getElementById("comunidadeFilter");
    const statusFilter = document.getElementById("statusFilter");
    const filterForm = document.getElementById("filterForm");

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            filterFichas(this.value.toLowerCase().trim());
        });

        searchInput.addEventListener("search", function () {
            if (this.value === "") {
                filterFichas("");
            }
        });

        if (searchInput.value) {
            filterFichas(searchInput.value.toLowerCase().trim());
        }
    }

    if (comunidadeFilter) {
        comunidadeFilter.addEventListener("change", submitFilter);
    }

    if (statusFilter) {
        statusFilter.addEventListener("change", submitFilter);

        // Mantém o filtro selecionado ao carregar a página
        const urlParams = new URLSearchParams(window.location.search);
        const statusParam = urlParams.get("status");
        if (statusParam) {
            statusFilter.value = statusParam;
        }
    }

    initModais();

    function filterFichas(query) {
        let hasVisibleItems = false;
        formItems.forEach((item) => {
            const searchText = item.getAttribute("data-search") || "";
            const isVisible = searchText.includes(query);
            item.style.display = isVisible ? "flex" : "none";
            if (isVisible) hasVisibleItems = true;
        });
        if (noRecordsMessage) {
            noRecordsMessage.style.display = hasVisibleItems ? "none" : "block";
        }
    }

    function submitFilter() {
        setTimeout(() => {
            if (filterForm) {
                filterForm.submit();
            }
        }, 100);
    }
}

// ==========================================
// FUNÇÕES PARA PÁGINA DE FORMULÁRIO (CREATE/EDIT)
// ==========================================

function initFormPage() {
    // Variáveis globais do Wizard
    window.currentStep = 1;
    window.totalSteps = 3;

    // Detectar modo da página
    const form = document.getElementById("wizardForm");
    const idFichaInput = document.getElementById("IdFicha");
    const isEditPage = idFichaInput && idFichaInput.value && idFichaInput.value !== "0";
    const isCreatePage = !isEditPage;

    // Inicializar mapa
    if (typeof initMapSelector === 'function') {
        initMapSelector('mapa-principal', 'input-endereco');
    } else {
        console.error("ERRO: initMapSelector não encontrada");
    }

    // Se for página de edição, configurar modo visualização/edição
    if (isEditPage) {
        initEditMode();
    }

    // Configuração de validação
    setupValidation();

    // Inicializa Wizard
    updateButtons();

    // Validação final ao enviar
    if (form) {
        form.addEventListener("submit", function (e) {
            if (!validateAllSteps()) {
                e.preventDefault();
            }
        });
    }
}

// ==========================================
// MODO DE EDIÇÃO (VISUALIZAÇÃO/EDIÇÃO)
// ==========================================

function initEditMode() {
    const form = document.getElementById("wizardForm");
    const inputFields = form.querySelectorAll('.clean-input, select.clean-input, textarea.clean-input');
    const radioInputs = form.querySelectorAll('input[type="radio"]');
    const checkboxInputs = form.querySelectorAll('input[type="checkbox"]');
    const btnSave = document.getElementById('btn-save');
    const btnNext = document.getElementById('btn-next');
    const btnPrev = document.getElementById('btn-prev');
    
    // Verificar se a ficha está bloqueada
    const alertWarning = document.querySelector('.alert-warning');
    const isLocked = alertWarning !== null;
    
    let isEditMode = false;
    let isSubmitting = false;

    function setFieldsState(disabled) {
        inputFields.forEach(field => field.disabled = disabled);
        radioInputs.forEach(field => field.disabled = disabled);
        checkboxInputs.forEach(field => field.disabled = disabled);
    }

    function updateEditButtons() {
        if (isLocked) {
            
            btnSave.innerHTML = '<i class="fa-solid fa-times"></i> Fechar';
            btnSave.style.display = '';
            btnSave.setAttribute('type', 'button');
            btnSave.onclick = function(e) {
                e.preventDefault();
                window.location.href = '/FichaPrimeiroContato/Index';
            };
            setFieldsState(true);
            
            // Permitir navegação entre steps mesmo bloqueado
            if (btnNext) btnNext.style.display = window.currentStep < window.totalSteps ? 'inline-flex' : 'none';
            if (btnPrev) btnPrev.style.display = window.currentStep > 1 ? 'inline-flex' : 'none';
            return;
        }

        if (isEditMode) {
            // Modo edição: salvar
            btnSave.innerHTML = '<i class="fa-solid fa-check"></i> Salvar Alterações';
            btnSave.setAttribute('type', 'submit');
            btnSave.onclick = null;
            setFieldsState(false);
            
            // Mostrar navegação normal
            updateButtons();
        } else {
            // Modo visualização: editar
            btnSave.innerHTML = '<i class="fa-solid fa-edit"></i> Editar';
            btnSave.setAttribute('type', 'button');
            btnSave.onclick = function(e) {
                e.preventDefault();
                isEditMode = true;
                updateEditButtons();
                
                // Focar no primeiro campo editável
                const firstInput = form.querySelector('input:not([type="hidden"]):not([disabled]), select:not([disabled]), textarea:not([disabled])');
                if (firstInput) firstInput.focus();
            };
            setFieldsState(true);
            
            // Permitir navegação entre steps
            if (btnNext) btnNext.style.display = window.currentStep < window.totalSteps ? 'inline-flex' : 'none';
            if (btnPrev) btnPrev.style.display = window.currentStep > 1 ? 'inline-flex' : 'none';
        }
    }

    // Handler de submit do formulário
    form.addEventListener('submit', function(e) {
        if (!isEditMode || isSubmitting || isLocked) {
            e.preventDefault();
            return;
        }

        if (!form.checkValidity() || !validateAllSteps()) {
            e.preventDefault();
            return;
        }

        isSubmitting = true;
        btnSave.disabled = true;
        btnSave.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Salvando...';
    });

    // Inicializar no modo visualização
    updateEditButtons();

    // Sobrescrever a função updateButtons para respeitar o modo de edição
    const originalUpdateButtons = window.updateButtons;
    window.updateButtons = function() {
        if (isEditMode && !isLocked) {
            // Modo edição: usar navegação normal do wizard
            originalUpdateButtons();
        } else {
            // Modo visualização ou bloqueado: usar botões customizados
            updateEditButtons();
        }
    };

    // Prevenir validação quando em modo visualização
    const originalChangeStep = window.changeStep;
    window.changeStep = function(direction) {
        if (!isEditMode && !isLocked) {
            // Modo visualização: permitir navegação sem validação
            const nextStep = window.currentStep + direction;
            if (nextStep < 1 || nextStep > window.totalSteps) return;

            const stepAtual = document.getElementById(`step-${window.currentStep}`);
            if (stepAtual) stepAtual.classList.remove("active");

            window.currentStep = nextStep;

            const stepNovo = document.getElementById(`step-${window.currentStep}`);
            if (stepNovo) stepNovo.classList.add("active");

            // Atualizar header steps
            for (let i = 1; i <= window.totalSteps; i++) {
                const stepEl = document.getElementById(`header-step-${i}`);
                if (stepEl) {
                    if (i <= window.currentStep) {
                        stepEl.classList.add("active");
                    } else {
                        stepEl.classList.remove("active");
                    }
                }
            }

            // Atualizar linhas
            for (let i = 1; i < window.totalSteps; i++) {
                const lineEl = document.getElementById(`line-${i}`);
                if (lineEl) {
                    if (window.currentStep > i) {
                        lineEl.classList.add("active");
                    } else {
                        lineEl.classList.remove("active");
                    }
                }
            }

            updateEditButtons();

            // Scroll suave
            const stepperContainer = document.querySelector(".stepper-container");
            if (stepperContainer) {
                stepperContainer.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            }
        } else {
            // Modo edição ou bloqueado: usar navegação original com validação
            originalChangeStep(direction);
        }
    };
}

// ==========================================
// FUNÇÕES COMUNS
// ==========================================

function initCommonFeatures() {
    // Modais (funcionam em todas as páginas que os tenham)
    initModais();

    // Estilos de erro dinâmicos
    addErrorStyles();

    // Fechar modais ao clicar fora
    window.addEventListener("click", function (event) {
        if (event.target.classList.contains("custom-modal")) {
            event.target.style.display = "none";
        }
    });
}

// ==========================================
// FUNÇÕES DE MODAL
// ==========================================

function initModais() {
    document.querySelectorAll(".btn-concluir").forEach((btn) => {
        btn.addEventListener("click", function (e) {
            if (
                this.classList.contains("gray") ||
                this.hasAttribute("disabled")
            ) {
                e.preventDefault();
                return;
            }

            const id = this.getAttribute("data-id");
            const form = document.getElementById("formConcluir");
            if (form) {
                const currentUrl = window.location.href;
                form.action =
                    "/FichaPrimeiroContato/Concluir/" +
                    id +
                    "?returnUrl=" +
                    encodeURIComponent(currentUrl);
                showModal("modalConcluir");
            }
        });
    });

    document.querySelectorAll(".btn-abandonar").forEach((btn) => {
        btn.addEventListener("click", function (e) {
            if (
                this.classList.contains("gray") ||
                this.hasAttribute("disabled")
            ) {
                e.preventDefault();
                return;
            }

            const id = this.getAttribute("data-id");
            const form = document.getElementById("formAbandonar");
            if (form) {
                const currentUrl = window.location.href;
                form.action =
                    "/FichaPrimeiroContato/Abandonar/" +
                    id +
                    "?returnUrl=" +
                    encodeURIComponent(currentUrl);
                showModal("modalAbandonar");
            }
        });
    });

    document.querySelectorAll(".btn-cancel").forEach((btn) => {
        btn.addEventListener("click", () => {
            hideAllModals();
        });
    });
}

function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "flex";
    }
}

function hideAllModals() {
    document.querySelectorAll(".custom-modal").forEach((m) => {
        m.style.display = "none";
    });
}

// ==========================================
// FUNÇÕES DE VALIDAÇÃO (WIZARD)
// ==========================================

function validateDateField(dateInput) {
    const selectedDate = new Date(dateInput.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const existingError = dateInput.parentElement.querySelector(".date-error");
    if (existingError) existingError.remove();

    dateInput.classList.remove("field-error");

    if (!dateInput.value) return true;

    if (selectedDate > today) {
        showDateError(
            dateInput,
            "A data não pode ser futura. Selecione uma data até hoje."
        );
        return false;
    }

    const tenYearsAgo = new Date();
    tenYearsAgo.setFullYear(today.getFullYear() - 10);

    if (selectedDate < tenYearsAgo) {
        showDateError(
            dateInput,
            "A data parece muito antiga. Por favor, verifique."
        );
        return false;
    }

    return true;
}

function showDateError(field, message) {
    const parent = field.parentElement;
    const errorSpan = document.createElement("span");
    errorSpan.className = "text-danger custom-error date-error";
    errorSpan.textContent = message;
    errorSpan.style.display = "block";
    errorSpan.style.marginTop = "5px";
    errorSpan.style.fontSize = "0.875rem";
    errorSpan.style.color = "#dc3545";

    parent.appendChild(errorSpan);
    field.classList.add("field-error");
    field.focus();
}

function setupValidation() {
    document.querySelectorAll("[required]").forEach((field) => {
        field.addEventListener("invalid", function (e) {
            e.preventDefault();

            if (this.type === "date" && this.id === "DtContato") {
                if (!validateDateField(this)) {
                    return;
                }
            }

            const existingError =
                this.parentElement.querySelector(".custom-error");
            if (existingError) existingError.remove();

            const errorSpan = document.createElement("span");
            errorSpan.className = "text-danger custom-error";
            errorSpan.textContent = "Campo obrigatório.";

            this.parentElement.appendChild(errorSpan);
            this.classList.add("field-error");

            if (!document.querySelector(".field-error")) {
                this.focus();
            }
        });

        field.addEventListener("input", function () {
            this.classList.remove("field-error");
            const errorSpan = this.parentElement.querySelector(".custom-error");
            if (errorSpan) errorSpan.remove();
        });
    });

    const radioGroups = [
        "NovoParceiro",
        "FornecidoParceiro",
        "SLer",
        "SCalc",
        "SComp",
    ];

    radioGroups.forEach((groupName) => {
        const radioButtons = document.querySelectorAll(
            `input[name="${groupName}"]`
        );
        const groupContainer = radioButtons[0]?.closest(".input-content");

        if (groupContainer) {
            groupContainer.classList.add("required-radio-group");

            const hasRequired = Array.from(radioButtons).some((radio) =>
                radio.hasAttribute("required")
            );

            if (!hasRequired) {
                radioButtons[0].setAttribute("required", "");
            }

            radioButtons.forEach((radio) => {
                radio.addEventListener("change", function () {
                    groupContainer.classList.remove("field-error");
                    const errorSpan =
                        groupContainer.querySelector(".custom-error");
                    if (errorSpan) errorSpan.remove();

                    radioButtons.forEach((r) => {
                        if (r !== radioButtons[0])
                            r.removeAttribute("required");
                    });
                });
            });
        }
    });
}

function validateCurrentStep() {
    const stepAtualEl = document.getElementById(`step-${window.currentStep}`);
    let isValid = true;

    if (stepAtualEl) {
        stepAtualEl
            .querySelectorAll(".custom-error")
            .forEach((error) => error.remove());
        stepAtualEl
            .querySelectorAll(".field-error")
            .forEach((field) => field.classList.remove("field-error"));

        const campos = stepAtualEl.querySelectorAll("input, select, textarea");

        for (const campo of campos) {
            if (campo.type === "hidden" || campo.disabled) continue;

            if (campo.type === "date" && campo.id === "DtContato") {
                if (!validateDateField(campo)) {
                    isValid = false;
                    continue;
                }
            }

            if (campo.hasAttribute("required")) {
                if (!campo.value.trim()) {
                    showFieldError(campo, "Campo obrigatório.");
                    isValid = false;

                    if (!document.querySelector(".field-error:focus")) {
                        campo.focus();
                    }
                }
            }

            if (campo.tagName === "SELECT" && campo.hasAttribute("required")) {
                if (campo.selectedIndex <= 0) {
                    showFieldError(campo, "Campo obrigatório.");
                    isValid = false;

                    if (!document.querySelector(".field-error:focus")) {
                        campo.focus();
                    }
                }
            }
        }

        const radioGroups = stepAtualEl.querySelectorAll(
            ".required-radio-group"
        );

        radioGroups.forEach((group) => {
            const groupName = group.querySelector('input[type="radio"]')?.name;
            const radioButtons = document.querySelectorAll(
                `input[name="${groupName}"]`
            );
            const isChecked = Array.from(radioButtons).some(
                (radio) => radio.checked
            );

            if (!isChecked) {
                const campoFicticio = document.createElement("div");
                campoFicticio.className = "radio-group-error";

                showFieldError(campoFicticio, "Campo obrigatório.", group);
                isValid = false;

                if (!document.querySelector(".field-error:focus")) {
                    radioButtons[0].focus();
                }
            }
        });
    }

    return isValid;
}

function showFieldError(field, message, container = null) {
    const parent = container || field.parentElement;
    let errorSpan = parent.querySelector(".custom-error");

    if (!errorSpan) {
        errorSpan = document.createElement("span");
        errorSpan.className = "text-danger custom-error";
        parent.appendChild(errorSpan);
    }

    errorSpan.textContent = message;
    errorSpan.style.display = "block";
    errorSpan.style.marginTop = "5px";
    errorSpan.style.fontSize = "0.875rem";

    if (field.classList) {
        field.classList.add("field-error");
    }
}

function changeStep(direction) {
    if (direction > 0) {
        if (!validateCurrentStep()) {
            const firstError = document.querySelector(".field-error");
            if (firstError) {
                firstError.scrollIntoView({
                    behavior: "smooth",
                    block: "center",
                    inline: "nearest",
                });
            }
            return;
        }
    }

    const nextStep = window.currentStep + direction;
    if (nextStep < 1 || nextStep > window.totalSteps) return;

    const stepAtual = document.getElementById(`step-${window.currentStep}`);
    if (stepAtual) stepAtual.classList.remove("active");

    window.currentStep = nextStep;

    const stepNovo = document.getElementById(`step-${window.currentStep}`);
    if (stepNovo) stepNovo.classList.add("active");

    for (let i = 1; i <= window.totalSteps; i++) {
        const stepEl = document.getElementById(`header-step-${i}`);
        if (stepEl) {
            if (i <= window.currentStep) {
                stepEl.classList.add("active");
            } else {
                stepEl.classList.remove("active");
            }
        }
    }

    for (let i = 1; i < window.totalSteps; i++) {
        const lineEl = document.getElementById(`line-${i}`);
        if (lineEl) {
            if (window.currentStep > i) {
                lineEl.classList.add("active");
            } else {
                lineEl.classList.remove("active");
            }
        }
    }

    updateButtons();

    const stepperContainer = document.querySelector(".stepper-container");
    if (stepperContainer) {
        stepperContainer.scrollIntoView({
            behavior: "smooth",
            block: "start",
        });
    }
}

function updateButtons() {
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const btnSave = document.getElementById("btn-save");

    if (!btnPrev || !btnNext || !btnSave) return;

    if (window.currentStep === 1) {
        btnPrev.style.display = "none";
    } else {
        btnPrev.style.display = "inline-flex";
    }

    if (window.currentStep === window.totalSteps) {
        btnNext.style.display = "none";
        btnSave.style.display = "inline-flex";
    } else {
        btnNext.style.display = "inline-flex";
        btnSave.style.display = "none";
    }
}

function validateAllSteps() {
    let allStepsValid = true;
    const originalStep = window.currentStep;

    window.currentStep = 1;
    if (!validateCurrentStep()) {
        allStepsValid = false;
    }

    window.currentStep = 2;
    if (allStepsValid && !validateCurrentStep()) {
        allStepsValid = false;
    }

    window.currentStep = 3;
    if (allStepsValid && !validateCurrentStep()) {
        allStepsValid = false;
    }

    window.currentStep = originalStep;
    updateButtons();

    if (!allStepsValid) {
        const firstError = document.querySelector(".field-error");
        if (firstError) {
            firstError.scrollIntoView({
                behavior: "smooth",
                block: "center",
            });

            if (!document.querySelector(".global-error-message")) {
                const globalError = document.createElement("div");
                globalError.className =
                    "alert alert-danger global-error-message";
                globalError.textContent =
                    "Por favor, corrija os campos obrigatórios antes de enviar.";
                globalError.style.margin = "10px 0";
                globalError.style.padding = "10px";
                globalError.style.borderRadius = "5px";

                const form = document.getElementById("wizardForm");
                if (form) {
                    form.insertBefore(globalError, form.firstChild);

                    setTimeout(() => {
                        globalError.remove();
                    }, 5000);
                }
            }
        }
        return false;
    }

    return true;
}

// ==========================================
// ESTILOS DINÂMICOS
// ==========================================

function addErrorStyles() {
    if (!document.querySelector("#dynamic-error-styles")) {
        const style = document.createElement("style");
        style.id = "dynamic-error-styles";
        style.textContent = `
            .field-error {
                border-color: #dc3545 !important;
                box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25) !important;
            }
            
            .required-radio-group.field-error {
                border: 2px solid #dc3545;
                padding: 10px;
                border-radius: 5px;
                background-color: rgba(220, 53, 69, 0.05);
            }
            
            .custom-error {
                display: block;
                margin-top: 5px;
                font-size: 0.875rem;
                color: #dc3545;
            }
            
            select.clean-input:required:invalid {
                color: #6c757d;
            }
            
            select.clean-input:required:valid {
                color: #212529;
            }
            
            input[type="date"].field-error::-webkit-calendar-picker-indicator {
                filter: invert(27%) sepia(86%) saturate(2840%) hue-rotate(342deg) brightness(92%) contrast(97%);
            }
            
            .global-error-message {
                animation: fadeIn 0.3s ease-in;
            }
            
            @keyframes fadeIn {
                from { opacity: 0; transform: translateY(-10px); }
                to { opacity: 1; transform: translateY(0); }
            }

            /* Estilos para campos desabilitados */
            .clean-input:disabled,
            select.clean-input:disabled

            input[type="radio"]:disabled,
            input[type="checkbox"]:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }

            input[type="radio"]:disabled + .radio-label,
            input[type="checkbox"]:disabled + .radio-label {
                color: #6c757d;
                opacity: 0.7;
                cursor: not-allowed;
            }

            .clean-input:disabled:hover,
            select.clean-input:disabled:hover,
            textarea.clean-input:disabled:hover {
                border-color: #e0e0e0;
            }

            .input-card:has(.clean-input:disabled) .icon-box {
                opacity: 0.6;
            }
        `;
        document.head.appendChild(style);
    }
}

// ==========================================
// EXPORTA FUNÇÕES PARA USO EXTERNO
// ==========================================
if (typeof window !== "undefined") {
    window.FichaPrimeiroContato = {
        changeStep: changeStep,
        validateCurrentStep: validateCurrentStep,
        showModal: showModal,
        hideAllModals: hideAllModals,
        filterFichas: function (query) {
            const formItems = document.querySelectorAll(".form-item");
            const noRecordsMessage = document.querySelector(".no-records");

            if (formItems.length > 0) {
                let hasVisibleItems = false;

                formItems.forEach((item) => {
                    const searchText = item.getAttribute("data-search") || "";
                    const isVisible = searchText.includes(
                        query.toLowerCase().trim()
                    );

                    item.style.display = isVisible ? "flex" : "none";

                    if (isVisible) {
                        hasVisibleItems = true;
                    }
                });

                if (noRecordsMessage) {
                    noRecordsMessage.style.display = hasVisibleItems
                        ? "none"
                        : "block";
                }
            }
        },
    };
}