// Ações declarativas no lugar de onclick/onchange/onsubmit inline (a política de segurança não permite script inline):
//   data-ir="/url"                 clique navega para a URL
//   data-acao="funcao"             clique chama window.funcao(...data-args); data-com-elemento passa o próprio elemento antes
//   data-args='["a", 1]'           argumentos em JSON (só valores, nunca código)
//   data-enviar-ao-mudar           ao mudar o campo, envia o formulário dele
//   data-confirmar="mensagem"      no <form>, pede confirmação antes de enviar
//   data-fechar-janela             fecha a aba (cadastro aberto em nova janela)
//   data-voltar                    volta para a página anterior
(function () {
    const traduzir = texto => (typeof window.translateText === 'function' ? window.translateText(texto) : texto);

    document.addEventListener('click', function (evento) {
        const alvo = evento.target.closest('[data-ir], [data-acao], [data-fechar-janela], [data-voltar]');
        if (!alvo || alvo.disabled) return;

        if (alvo.hasAttribute('data-ir')) {
            evento.preventDefault();
            window.location.href = alvo.getAttribute('data-ir');
        } else if (alvo.hasAttribute('data-acao')) {
            const funcao = window[alvo.getAttribute('data-acao')];
            if (typeof funcao !== 'function') return;
            let args = [];
            try { args = JSON.parse(alvo.getAttribute('data-args') || '[]'); } catch (e) { args = []; }
            if (alvo.hasAttribute('data-com-elemento')) args.unshift(alvo);
            funcao.apply(alvo, args);
        } else if (alvo.hasAttribute('data-fechar-janela')) {
            window.close();
        } else if (alvo.hasAttribute('data-voltar')) {
            evento.preventDefault();
            window.history.back();
        }
    });

    document.addEventListener('change', function (evento) {
        const campo = evento.target.closest('[data-enviar-ao-mudar]');
        if (campo && campo.getAttribute('data-enviar-ao-mudar') !== 'false' && campo.form) campo.form.submit();
    });

    document.addEventListener('submit', function (evento) {
        const form = evento.target;
        const mensagem = form.getAttribute && form.getAttribute('data-confirmar');
        if (mensagem && !window.confirm(traduzir(mensagem))) evento.preventDefault();
    }, true);
})();
