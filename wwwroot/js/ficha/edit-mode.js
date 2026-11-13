// wwwroot/js/ficha/edit-mode.js
document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".main-form");

    if (!form || form.dataset.mode !== "edit") return;

    const editBtn = document.getElementById("edit-save-btn");
    const inputs = form.querySelectorAll("input, select, textarea");

    let isEditing = false;

    function setViewMode() {
        inputs.forEach(i => i.disabled = true);
        editBtn.innerHTML = '<i class="fa-solid fa-edit"></i> Editar';
        isEditing = false;
    }

    function setEditMode() {
        inputs.forEach(i => i.disabled = false);
        editBtn.innerHTML = '<i class="fa-solid fa-check"></i> Salvar';
        isEditing = true;
    }

    // Voltou do servidor com erros: continua editando para a pessoa corrigir.
    if (form.dataset.comErros === "true") setEditMode();
    else setViewMode();

    editBtn?.addEventListener("click", () => {
        if (!isEditing) { setEditMode(); return; }
        // Confere todas as etapas antes de enviar (form.submit() sozinho pulava a validação).
        if (typeof window.validarFormularioFicha === "function" && !window.validarFormularioFicha()) return;
        form.submit();
    });
});
