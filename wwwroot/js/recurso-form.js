// Formulário de recurso da comunidade: o tipo "Relacional" troca o nome livre pela escolha de um ator.
document.addEventListener('DOMContentLoaded', function () {
    const tipo = document.querySelector('[data-recurso-tipo]');
    if (!tipo) return;

    const blocoTexto = document.querySelector('[data-recurso-nome="texto"]');
    const blocoAtor = document.querySelector('[data-recurso-nome="ator"]');
    const blocoResponsavel = document.querySelector('[data-recurso-responsavel]');

    // Só o bloco visível envia valores: os dois selects de ator usam o mesmo nome (FK_id_Atores).
    function mostrar(bloco, visivel, obrigatorio) {
        bloco.hidden = !visivel;
        bloco.querySelectorAll('input, select').forEach(campo => {
            campo.disabled = !visivel;
            campo.required = visivel && obrigatorio;
        });
    }

    function atualizar() {
        const relacional = tipo.value === 'Relacional';
        mostrar(blocoTexto, !relacional, true);
        mostrar(blocoAtor, relacional, true);
        mostrar(blocoResponsavel, !relacional, false);
    }

    tipo.addEventListener('change', atualizar);
    atualizar();
});
