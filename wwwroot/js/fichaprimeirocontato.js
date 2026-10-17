window.pageInit = function () {
    // Telefone
    initTelefoneMask("inputTelefone");

    // Data de contato
    initDateMaxToday("DtContato");

    // Mapa
    // O próprio campo de endereço aceita digitação e mostra sugestões (além da lupa do mapa).
    initMapSafe("mapa-principal", "input-endereco", {
        sourceInputId: "input-endereco",
        manualInputId: "input-endereco"
    });

    // Estado inicial dos campos (se houver modo edição)
    if (typeof setPageState === "function") {
        setPageState();
    }
};
