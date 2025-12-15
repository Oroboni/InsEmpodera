// ==========================================
// SISTEMA DE WIZARD E NAVEGAÇÃO
// ==========================================

let currentStep = 1;
const totalSteps = 3;

function changeStep(direction) {
    const steps = document.querySelectorAll(".step-content");
    const headerSteps = document.querySelectorAll(".step");
    const lines = document.querySelectorAll(".line");
    
    // Ocultar step atual
    steps[currentStep - 1].classList.remove("active");
    headerSteps[currentStep - 1].classList.remove("active");
    
    // Atualizar currentStep
    currentStep += direction;
    
    // Garantir que currentStep está dentro dos limites
    if (currentStep < 1) currentStep = 1;
    if (currentStep > totalSteps) currentStep = totalSteps;
    
    // Mostrar novo step
    steps[currentStep - 1].classList.add("active");
    headerSteps[currentStep - 1].classList.add("active");
    
    // Atualizar linhas de progresso
    for (let i = 0; i < lines.length; i++) {
        if (i < currentStep - 1) {
            lines[i].classList.add("active");
        } else {
            lines[i].classList.remove("active");
        }
    }
    
    // Atualizar visibilidade dos botões
    updateButtonVisibility();
    
    // Scroll para o topo
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function updateButtonVisibility() {
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const btnSave = document.getElementById("btn-save");
    const editSaveBtn = document.getElementById("edit-save-btn");
    const deleteBtn = document.getElementById("openDeleteModalBtn");
    
    const isEditMode = editSaveBtn && editSaveBtn.getAttribute("type") === "submit";
    
    // Botão Voltar
    if (btnPrev) {
        btnPrev.style.display = currentStep === 1 ? "none" : "inline-block";
    }
    
    // Botão Próximo
    if (btnNext) {
        btnNext.style.display = currentStep === totalSteps ? "none" : "inline-block";
    }
    
    // Botões centrais (Editar/Salvar e Deletar)
    if (editSaveBtn) {
        editSaveBtn.style.display = currentStep === totalSteps ? "none" : "inline-block";
    }
    if (deleteBtn) {
        deleteBtn.style.display = currentStep === totalSteps ? "none" : "inline-block";
    }
    
    // Botão Atualizar Ficha (só aparece no último step em modo edição)
    if (btnSave) {
        btnSave.style.display = (currentStep === totalSteps && isEditMode) ? "inline-block" : "none";
    }
}

// ==========================================
// FILTROS E BUSCA (INDEX)
// ==========================================

document.addEventListener("DOMContentLoaded", function () {
    console.log("🔵 DOMContentLoaded - Filtros e Busca");
    
    const searchInput = document.getElementById("searchInput");
    const fichasContainer = document.getElementById("fichasContainer");
    const noResultsMessage = document.getElementById("noResultsMessage");
    const fichaItems = document.querySelectorAll(".ficha-item");
    const filterStatusButtons = document.querySelectorAll(".filter-eixo-btn");
    const filterComunidadeButtons = document.querySelectorAll(".filter-comunidade-btn");

    // Se não há fichaItems, não é a página Index
    if (fichaItems.length === 0) {
        console.log("⚪ Não é página Index - pulando inicialização de filtros");
    }

    let currentStatusFilter = "todos";
    let currentComunidadeFilter = "todas";

    // Função para filtrar fichas
    function filterFichas() {
        if (!searchInput) return;
        
        const searchTerm = searchInput.value.toLowerCase().trim();
        let visibleCount = 0;

        fichaItems.forEach((item) => {
            const nome = item.getAttribute("data-nome") || "";
            const status = item.getAttribute("data-status") || "";
            const comunidade = item.getAttribute("data-comunidade") || "";

            const matchesSearch = nome.includes(searchTerm);
            
            let matchesStatus = true;
            if (currentStatusFilter !== "todos") {
                matchesStatus = status === currentStatusFilter;
            }

            let matchesComunidade = true;
            if (currentComunidadeFilter !== "todas") {
                matchesComunidade = comunidade === currentComunidadeFilter;
            }

            const shouldShow = matchesSearch && matchesStatus && matchesComunidade;

            if (shouldShow) {
                item.style.display = "flex";
                visibleCount++;
            } else {
                item.style.display = "none";
            }
        });

        // Mostrar/ocultar mensagem de "nenhum resultado"
        if (visibleCount === 0 && fichaItems.length > 0) {
            if (fichasContainer) fichasContainer.style.display = "none";
            if (noResultsMessage) noResultsMessage.style.display = "block";
        } else {
            if (fichasContainer) fichasContainer.style.display = "flex";
            if (noResultsMessage) noResultsMessage.style.display = "none";
        }

        updateCounters();
    }

    // Função para atualizar contadores
    function updateCounters() {
        const visibleItems = Array.from(fichaItems).filter(
            (item) => item.style.display !== "none"
        );

        const totalVisible = visibleItems.length;
        const emProgressoVisible = visibleItems.filter(
            (item) => item.getAttribute("data-status") === "EmProgresso"
        ).length;
        const concluidasVisible = visibleItems.filter(
            (item) => item.getAttribute("data-status") === "Concluida"
        ).length;
        const abandonadasVisible = visibleItems.filter(
            (item) => item.getAttribute("data-status") === "Abandonada"
        ).length;

        const totalEl = document.getElementById("totalFichas");
        const progressoEl = document.getElementById("emProgressoCount");
        const concluidasEl = document.getElementById("concluidasCount");
        const abandonadasEl = document.getElementById("abandonadasCount");

        if (totalEl) totalEl.textContent = totalVisible;
        if (progressoEl) progressoEl.textContent = emProgressoVisible;
        if (concluidasEl) concluidasEl.textContent = concluidasVisible;
        if (abandonadasEl) abandonadasEl.textContent = abandonadasVisible;
    }

    // Event listener para busca
    if (searchInput) {
        searchInput.addEventListener("input", filterFichas);
        searchInput.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                searchInput.value = "";
                filterFichas();
            }
        });
    }

    // Event listeners para filtros de status
    filterStatusButtons.forEach((button) => {
        button.addEventListener("click", function () {
            filterStatusButtons.forEach((btn) => btn.classList.remove("active"));
            this.classList.add("active");
            currentStatusFilter = this.getAttribute("data-status");
            filterFichas();
        });
    });

    // Event listeners para filtros de comunidade
    filterComunidadeButtons.forEach((button) => {
        button.addEventListener("click", function () {
            filterComunidadeButtons.forEach((btn) => btn.classList.remove("active"));
            this.classList.add("active");
            currentComunidadeFilter = this.getAttribute("data-comunidade");
            filterFichas();
        });
    });

    // ==========================================
    // MODAIS CONCLUIR E ABANDONAR (INDEX)
    // ==========================================
    const modalConcluir = document.getElementById("modalConcluir");
    const modalAbandonar = document.getElementById("modalAbandonar");
    const formConcluir = document.getElementById("formConcluir");
    const formAbandonar = document.getElementById("formAbandonar");

    // Botões de concluir
    document.querySelectorAll(".btn-concluir").forEach((btn) => {
        btn.addEventListener("click", function () {
            if (!this.disabled) {
                const id = this.getAttribute("data-id");
                if (formConcluir) {
                    formConcluir.action = '/FichaPrimeiroContato/Concluir/' + id;
                    modalConcluir.style.display = "flex";
                    setTimeout(() => modalConcluir.classList.add("active"), 10);
                }
            }
        });
    });

    // Botões de abandonar
    document.querySelectorAll(".btn-abandonar").forEach((btn) => {
        btn.addEventListener("click", function () {
            if (!this.disabled) {
                const id = this.getAttribute("data-id");
                if (formAbandonar) {
                    formAbandonar.action = '/FichaPrimeiroContato/Abandonar/' + id;
                    modalAbandonar.style.display = "flex";
                    setTimeout(() => modalAbandonar.classList.add("active"), 10);
                }
            }
        });
    });

    // Fechar modais
    document.querySelectorAll(".btn-cancel").forEach((btn) => {
        btn.addEventListener("click", function () {
            closeModal(modalConcluir);
            closeModal(modalAbandonar);
        });
    });

    // Fechar ao clicar fora
    [modalConcluir, modalAbandonar].forEach((modal) => {
        if (modal) {
            modal.addEventListener("click", function (e) {
                if (e.target === modal) {
                    closeModal(modal);
                }
            });
        }
    });

    // Fechar com ESC (apenas para modais Index)
    if (modalConcluir || modalAbandonar) {
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                if (modalConcluir) closeModal(modalConcluir);
                if (modalAbandonar) closeModal(modalAbandonar);
            }
        });
    }

    function closeModal(modal) {
        if (modal) {
            modal.classList.remove("active");
            setTimeout(() => {
                modal.style.display = "none";
            }, 300);
        }
    }

    // Loading nos botões de submit dos modais
    [formConcluir, formAbandonar].forEach((form) => {
        if (form) {
            form.addEventListener("submit", function () {
                const btn = this.querySelector(".btn-confirm");
                if (btn) {
                    btn.disabled = true;
                    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Processando...';
                }
            });
        }
    });
});

// ==========================================
// MODAL DELETE E MODO EDIÇÃO (EDIT)
// ==========================================

document.addEventListener("DOMContentLoaded", function () {
    console.log("🟢 DOMContentLoaded - Modal Delete e Modo Edição");
    
    const form = document.querySelector(".main-form");
    
    // Debug: verificar se encontrou o formulário
    console.log("Form encontrado:", form ? "✅ Sim" : "❌ Não");
    
    if (!form) {
        console.log("⚪ Não é página Edit - pulando inicialização");
        return;
    }
    
    const inputFields = form.querySelectorAll('input:not([type="hidden"]), select, textarea');
    const editSaveBtn = document.getElementById("edit-save-btn");
    const deleteModal = document.getElementById("deleteConfirmationModal");
    const openDeleteBtn = document.getElementById("openDeleteModalBtn");
    
    // Debug: verificar elementos
    console.log("Edit/Save Button:", editSaveBtn ? "✅ Encontrado" : "❌ Não encontrado");
    console.log("Delete Modal:", deleteModal ? "✅ Encontrado" : "❌ Não encontrado");
    console.log("Open Delete Button:", openDeleteBtn ? "✅ Encontrado" : "❌ Não encontrado");
    console.log("Input Fields:", inputFields.length);
    
    // Buscar status do Model (via window.fichaStatus ou data-attribute)
    const statusFromWindow = window.fichaStatus;
    const statusFromAttribute = document.querySelector("[data-status-model]")?.getAttribute("data-status-model");
    const statusFromModel = statusFromWindow || statusFromAttribute;
    const isEmProgresso = statusFromModel === "EmProgresso";
    
    console.log("Status do Model (window):", statusFromWindow || "Não encontrado");
    console.log("Status do Model (attribute):", statusFromAttribute || "Não encontrado");
    console.log("Status Final:", statusFromModel || "Não encontrado");
    console.log("É Em Progresso?", isEmProgresso ? "✅ Sim" : "❌ Não");
    
    let isSubmitting = false;
    
    // Configurar estado inicial
    function setInitialState() {
        console.log("🔧 Configurando estado inicial...");
        
        if (isEmProgresso) {
            // Modo edição ativo
            enableEditing();
            if (editSaveBtn) {
                editSaveBtn.innerHTML = '<i class="fa-solid fa-check"></i> Salvar Alterações';
                editSaveBtn.setAttribute("type", "submit");
                editSaveBtn.classList.remove("btn-next");
                editSaveBtn.classList.add("btn-save-final");
                console.log("✅ Modo Edição ativado");
            }
        } else {
            // Modo visualização
            disableEditing();
            if (editSaveBtn) {
                editSaveBtn.innerHTML = '<i class="fa-solid fa-edit"></i> Editar';
                editSaveBtn.setAttribute("type", "button");
                editSaveBtn.classList.remove("btn-save-final");
                editSaveBtn.classList.add("btn-next");
                console.log("✅ Modo Visualização ativado");
            }
        }
        
        // Atualizar visibilidade dos botões
        updateButtonVisibility();
    }
    
    function enableEditing() {
        console.log("🔓 Habilitando edição de", inputFields.length, "campos");
        inputFields.forEach((field) => {
            if (!field.hasAttribute("readonly")) {
                field.disabled = false;
            }
        });
    }
    
    function disableEditing() {
        console.log("🔒 Desabilitando edição de", inputFields.length, "campos");
        inputFields.forEach((field) => {
            field.disabled = true;
        });
    }
    
    // Inicializar
    setInitialState();
    
    // Handler do botão Editar/Salvar
    if (editSaveBtn) {
        console.log("📌 Adicionando listener ao botão Edit/Save");
        
        editSaveBtn.addEventListener("click", function (e) {
            console.log("🖱️ Botão Edit/Save clicado!");
            console.log("Tipo atual:", editSaveBtn.getAttribute("type"));
            
            if (editSaveBtn.getAttribute("type") === "submit") {
                console.log("💾 Modo Salvar - validando formulário...");
                
                // Modo Salvar Alterações - submeter formulário
                if (!isSubmitting && form.checkValidity()) {
                    console.log("✅ Formulário válido - submetendo...");
                    isSubmitting = true;
                    editSaveBtn.disabled = true;
                    editSaveBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Processando...';
                    form.submit();
                } else if (!form.checkValidity()) {
                    console.log("❌ Formulário inválido!");
                    form.reportValidity();
                }
            } else {
                console.log("✏️ Modo Editar - ativando edição...");
                
                // Modo Editar - ativar edição
                e.preventDefault();
                enableEditing();
                
                editSaveBtn.innerHTML = '<i class="fa-solid fa-check"></i> Salvar Alterações';
                editSaveBtn.setAttribute("type", "submit");
                editSaveBtn.classList.remove("btn-next");
                editSaveBtn.classList.add("btn-save-final");
                
                console.log("✅ Edição ativada - botão mudou para Salvar");
                
                // Atualizar visibilidade dos botões
                updateButtonVisibility();
                
                // Focar no primeiro campo
                const firstInput = form.querySelector('input:not([disabled]):not([type="hidden"]):not([readonly])');
                if (firstInput) firstInput.focus();
            }
        });
    } else {
        console.log("⚠️ Botão Edit/Save não encontrado!");
    }
    
    // ==========================================
    // MODAL DE EXCLUSÃO
    // ==========================================
    
    const cancelDeleteBtn = document.getElementById("cancelDeleteBtn");
    const deleteForm = document.getElementById("deleteForm");
    const confirmDeleteBtn = document.getElementById("confirmDeleteBtn");
    
    console.log("Delete Form:", deleteForm ? "✅ Encontrado" : "❌ Não encontrado");
    
    if (openDeleteBtn) {
        console.log("📌 Adicionando listener ao botão Open Delete");
        
        openDeleteBtn.addEventListener("click", function (e) {
            console.log("🖱️ Botão Delete clicado!");
            e.preventDefault();
            
            if (deleteModal) {
                console.log("✅ Abrindo modal de exclusão");
                deleteModal.style.display = "flex";
                setTimeout(() => deleteModal.classList.add("active"), 10);
            } else {
                console.log("❌ Modal de exclusão não encontrado!");
            }
        });
    } else {
        console.log("⚠️ Botão Open Delete não encontrado!");
    }
    
    if (cancelDeleteBtn) {
        cancelDeleteBtn.addEventListener("click", function () {
            console.log("❌ Cancelando exclusão");
            closeDeleteModal();
        });
    }
    
    // Loading no botão de delete
    if (deleteForm) {
        deleteForm.addEventListener("submit", function () {
            console.log("🗑️ Submetendo formulário de exclusão");
            if (confirmDeleteBtn && !confirmDeleteBtn.disabled) {
                confirmDeleteBtn.disabled = true;
                confirmDeleteBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Deletando...';
            }
        });
    }
    
    // Fechar modal ao clicar fora
    if (deleteModal) {
        window.addEventListener("click", function (e) {
            if (e.target === deleteModal) {
                console.log("👆 Clicou fora do modal - fechando");
                closeDeleteModal();
            }
        });
        
        // Fechar modal com ESC
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && deleteModal.classList.contains("active")) {
                console.log("⌨️ ESC pressionado - fechando modal");
                closeDeleteModal();
            }
        });
    }
    
    function closeDeleteModal() {
        if (deleteModal) {
            console.log("🚪 Fechando modal de exclusão");
            deleteModal.classList.remove("active");
            setTimeout(() => {
                deleteModal.style.display = "none";
            }, 300);
        }
    }
    
    // Prevenir envio duplo
    form.addEventListener("submit", function (e) {
        if (isSubmitting) {
            console.log("⚠️ Envio duplo prevenido!");
            e.preventDefault();
            return false;
        }
    });
});

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
            select.clean-input:disabled,
            textarea.clean-input:disabled,
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

// Executar ao carregar
addErrorStyles();