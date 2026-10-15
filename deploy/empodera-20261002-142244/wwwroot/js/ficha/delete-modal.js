// wwwroot/js/ficha/delete-modal.js
document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".main-form");

    if (!form || form.dataset.mode !== "edit") return;

    const openBtn = document.getElementById("openDeleteModalBtn");
    const modal = document.getElementById("deleteConfirmationModal");
    const cancelBtn = document.getElementById("cancelDeleteBtn");

    openBtn?.addEventListener("click", () => {
        modal.classList.add("active");
    });

    cancelBtn?.addEventListener("click", () => {
        modal.classList.remove("active");
    });
});
