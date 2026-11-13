// Avisos padrão: o toast de sucesso some sozinho; a caixa de erros leva o foco ao primeiro campo inválido.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.toast-aviso').forEach(function (toast) {
        const fechar = function () {
            toast.classList.add('saindo');
            setTimeout(function () { toast.remove(); }, 250);
        };
        toast.querySelector('.toast-fechar')?.addEventListener('click', fechar);
        if (toast.classList.contains('sucesso')) setTimeout(fechar, 5000);
    });

    if (document.querySelector('.aviso-formulario')) {
        const campos = document.querySelectorAll('.input-validation-error:not([type=hidden])');
        campos.forEach(function (campo) { campo.closest('.input-card, .form-group')?.classList.add('campo-com-erro'); });
        if (campos.length) campos[0].focus({ preventScroll: true });
        document.querySelector('.aviso-formulario').scrollIntoView({ block: 'start' });
    }
});
