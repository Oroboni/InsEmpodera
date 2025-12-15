window.pageInit = function () {
    // Telefone
    initTelefoneMask("inputTelefone");

    // Data de contato
    initDateMaxToday("DtContato");

    // Mapa
    initMapSafe("mapa-principal", "input-endereco");

    // Estado inicial dos campos (se houver modo edição)
    if (typeof setPageState === "function") {
        setPageState();
    }
};
