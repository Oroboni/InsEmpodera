document.addEventListener('DOMContentLoaded', function () {
    
    const campoTelefone = document.getElementById('inputTelefone');

    const aplicarMascaraTelefone = (valor) => {
        if (!valor) return "";

        // 1. Remove tudo o que não é número
        valor = valor.replace(/\D/g, "");

        // 2. Limita a 11 números (DDD + 9 dígitos)
        valor = valor.substring(0, 11);

        // 3. Aplica a formatação (XX) XXXXX-XXXX
        valor = valor.replace(/^(\d{2})(\d)/g, "($1) $2");
        valor = valor.replace(/(\d)(\d{4})$/, "$1-$2");

        return valor;
    };

    if (campoTelefone) {
        
        campoTelefone.addEventListener('input', (e) => {
            e.target.value = aplicarMascaraTelefone(e.target.value);
        });

        if (campoTelefone.value) {
            campoTelefone.value = aplicarMascaraTelefone(campoTelefone.value);
        }
    }
});

