// Campo de telefone com DDI da tela de Atores (Views/Atores/_CampoTelefone.cshtml).
// Grava em #telefoneValor no formato "+DDI número", ex.: "+55 15 95555-5644".
(function () {
    "use strict";

    function apenasDigitos(texto) {
        return (texto || "").replace(/\D/g, "");
    }

    // Brasil: "DD NNNNN-NNNN" (celular) ou "DD NNNN-NNNN" (fixo), aplicado enquanto se digita.
    function formatarBrasil(texto) {
        const d = apenasDigitos(texto).slice(0, 11);
        if (d.length <= 2) return d;
        const ddd = d.slice(0, 2);
        const numero = d.slice(2);
        const corte = numero.length > 8 ? 5 : 4;
        if (numero.length <= corte) return ddd + " " + numero;
        return ddd + " " + numero.slice(0, corte) + "-" + numero.slice(corte);
    }

    function formatarNumero(ddi, texto) {
        if (ddi === "55") return formatarBrasil(texto);
        if (ddi === "outro") return (texto || "").replace(/[^\d +]/g, "").replace(/(?!^)\+/g, "");
        return (texto || "").replace(/[^\d ]/g, "").replace(/\s{2,}/g, " ");
    }

    function init() {
        const container = document.querySelector("[data-telefone]");
        if (!container) return;

        const ddi = container.querySelector("#telefoneDdi");
        const numero = container.querySelector("#telefoneNumero");
        const valor = container.querySelector("#telefoneValor");
        const ddisConhecidos = Array.from(ddi.options)
            .map(o => o.value)
            .filter(v => v !== "outro")
            .sort((a, b) => b.length - a.length);

        function atualizarPlaceholder() {
            numero.placeholder = ddi.value === "55" ? "15 95555-5644"
                : ddi.value === "outro" ? "+DDI e número" : "Número com código de área";
        }

        function sincronizar() {
            const texto = numero.value.trim();
            if (!apenasDigitos(texto)) {
                valor.value = "";
            } else if (ddi.value === "outro") {
                valor.value = texto.startsWith("+") ? texto : "+" + texto;
            } else {
                valor.value = "+" + ddi.value + " " + texto;
            }
        }

        // Carrega o valor já salvo, inclusive números antigos sem DDI (assumidos como Brasil).
        function carregar(salvo) {
            salvo = (salvo || "").trim();
            if (!salvo) return;
            if (salvo.startsWith("+")) {
                const resto = salvo.slice(1).trim();
                const digitos = apenasDigitos(resto);
                const encontrado = ddisConhecidos.find(c => digitos.startsWith(c));
                if (encontrado) {
                    ddi.value = encontrado;
                    // Remove o DDI do início, preservando o restante do texto.
                    let consumidos = 0;
                    let i = 0;
                    while (i < resto.length && consumidos < encontrado.length) {
                        if (/\d/.test(resto[i])) consumidos++;
                        i++;
                    }
                    numero.value = formatarNumero(encontrado, resto.slice(i).trim());
                } else {
                    ddi.value = "outro";
                    numero.value = salvo;
                }
            } else {
                ddi.value = "55";
                numero.value = formatarBrasil(salvo);
            }
        }

        carregar(valor.value);
        atualizarPlaceholder();
        sincronizar();

        numero.addEventListener("input", function () {
            numero.value = formatarNumero(ddi.value, numero.value);
            sincronizar();
        });
        ddi.addEventListener("change", function () {
            numero.value = formatarNumero(ddi.value, numero.value);
            atualizarPlaceholder();
            sincronizar();
            numero.focus();
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
