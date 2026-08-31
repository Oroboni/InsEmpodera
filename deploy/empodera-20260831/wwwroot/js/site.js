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
