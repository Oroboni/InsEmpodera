document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".main-form");
    if (!form) return;

    const mode = form.dataset.mode;

    if (mode === "create") {
        console.log("🟢 Modo CREATE");
    }

    if (mode === "edit") {
        console.log("🟡 Modo EDIT");
    }
});
