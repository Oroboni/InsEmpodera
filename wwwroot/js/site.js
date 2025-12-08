const campoTelefone = document.getElementById("inputTelefone");
if (campoTelefone) {
    const aplicarMascaraTelefone = (valor) => {
        if (!valor) return "";
        valor = valor.replace(/\D/g, "");
        valor = valor.substring(0, 11);
        valor = valor.replace(/^(\d{2})(\d)/g, "($1) $2");
        valor = valor.replace(/(\d)(\d{4})$/, "$1-$2");
        return valor;
    };

    campoTelefone.addEventListener("input", (e) => {
        e.target.value = aplicarMascaraTelefone(e.target.value);
    });

    if (campoTelefone.value) {
        campoTelefone.value = aplicarMascaraTelefone(campoTelefone.value);
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

if (typeof initMapSelector === "function") {
    initMapSelector("mapa-principal", "input-endereco");
}
