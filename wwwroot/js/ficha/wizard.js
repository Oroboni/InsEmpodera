if (typeof window.currentStep === "undefined") {
    window.currentStep = 1;
}

let totalSteps = document.querySelectorAll(".step-content").length;

function showStep(step) {
    document.querySelectorAll(".step-content").forEach((el, index) => {
        el.classList.toggle("active", index + 1 === step);
    });

    // Atualiza o stepper do topo
    document.querySelectorAll(".step").forEach((el, index) => {
        el.classList.toggle("active", index + 1 <= step);
    });
}

function changeStep(direction) {
    const next = currentStep + direction;
    if (next < 1 || next > totalSteps) return;

    currentStep = next;
    showStep(currentStep);
    updateButtonVisibility();
}

function updateButtonVisibility() {
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const btnSave = document.getElementById("btn-save");

    if (!btnPrev || !btnNext) return;

    btnPrev.style.display = "inline-flex";
    btnNext.style.display = "inline-flex";

    if (currentStep === 1) {
        btnPrev.innerHTML =
            '<i class="fa-solid fa-arrow-left"></i> Sair da ficha';
        btnPrev.onclick = () => window.history.back();
    } else {
        btnPrev.innerHTML = '<i class="fa-solid fa-arrow-left"></i> Voltar';
        btnPrev.onclick = () => changeStep(-1);
    }

    if (currentStep === totalSteps) {
        btnNext.style.display = "none";
        btnSave && (btnSave.style.display = "inline-flex");
    } else {
        btnNext.innerHTML = 'Próximo <i class="fa-solid fa-arrow-right"></i>';
        btnNext.onclick = () => changeStep(1);
        btnSave && (btnSave.style.display = "none");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    showStep(currentStep);
    updateButtonVisibility();
});
