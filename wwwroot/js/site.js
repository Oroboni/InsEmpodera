const campoTelefone = document.getElementById("inputTelefone");
if (campoTelefone) {
    // O campo deve preservar o formato informado; não há DDD nem país padrão.
    campoTelefone.setAttribute("autocomplete", "tel");
    campoTelefone.setAttribute("inputmode", "tel");
}
