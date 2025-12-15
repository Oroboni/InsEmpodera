function validateDateField(input) {
    if (!input || !input.value) return;

    const selectedDate = new Date(input.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate > today) {
        input.setCustomValidity("A data não pode ser futura.");
    } else {
        input.setCustomValidity("");
    }
}

function initTelefoneMask(inputId) {
    const campo = document.getElementById(inputId);
    if (!campo) return;

    const aplicarMascara = (valor) => {
        valor = valor.replace(/\D/g, "").substring(0, 11);
        valor = valor.replace(/^(\d{2})(\d)/, "($1) $2");
        valor = valor.replace(/(\d)(\d{4})$/, "$1-$2");
        return valor;
    };

    campo.addEventListener("input", (e) => {
        e.target.value = aplicarMascara(e.target.value);
    });

    if (campo.value) {
        campo.value = aplicarMascara(campo.value);
    }
}

const dataContatoInput = document.getElementById("DtContato");
if (dataContatoInput) {
    const today = new Date().toISOString().split("T")[0];
    dataContatoInput.setAttribute("max", today);

    dataContatoInput.addEventListener("change", function () {
        validateDateField(this);
    });

    if (dataContatoInput.value) {
        validateDateField(dataContatoInput);
    }

    dataContatoInput.addEventListener("input", function () {
        setTimeout(() => {
            validateDateField(this);
        }, 100);
    });
}

function initDateMaxToday(inputId) {
    const input = document.getElementById(inputId);
    if (!input) return;

    input.max = new Date().toISOString().split("T")[0];

    input.addEventListener("change", () => validateDateField(input));
    input.addEventListener("input", () => {
        setTimeout(() => validateDateField(input), 100);
    });

    if (input.value) {
        validateDateField(input);
    }
}

function initMapSafe(mapId, inputId) {
    if (
        typeof initMapSelector === "function" &&
        document.getElementById(mapId) &&
        document.getElementById(inputId)
    ) {
        initMapSelector(mapId, inputId);
    }
}

function validateDateField(input) {
    if (!input) return;

    const selectedDate = new Date(input.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selectedDate > today) {
        input.setCustomValidity("A data não pode ser futura.");
    } else {
        input.setCustomValidity("");
    }
}
