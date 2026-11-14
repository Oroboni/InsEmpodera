// Modo edição padrão (P4): nas telas de detalhe com o botão Editar/Salvar (#edit-save-btn), mostra o selo
// "Modo edição" e o botão "Cancelar edição" enquanto a tela está editável (o botão exibe o ícone de salvar).
// Cancelar recarrega a tela pelo endereço (GET), descartando o que não foi salvo.
(function () {
    const botao = document.getElementById('edit-save-btn');
    if (!botao) return;
    const t = texto => (typeof window.translateText === 'function' ? window.translateText(texto) : texto);

    const selo = document.createElement('div');
    selo.className = 'modo-edicao-selo';
    selo.setAttribute('role', 'status');
    selo.dataset.i18nText = '';
    const icone = document.createElement('i');
    icone.className = 'fa-solid fa-pen-to-square';
    icone.setAttribute('aria-hidden', 'true');
    selo.append(icone, document.createTextNode(' ' + t('Modo edição: as alterações só valem depois de salvar.')));
    selo.hidden = true;
    const cabecalho = document.querySelector('.modern-container .header-section') || document.querySelector('.header-section');
    if (cabecalho) cabecalho.after(selo);

    const cancelar = document.createElement('button');
    cancelar.type = 'button';
    cancelar.className = 'wizard-btn btn-prev btn-cancelar-edicao';
    const iconeCancelar = document.createElement('i');
    iconeCancelar.className = 'fa-solid fa-xmark';
    iconeCancelar.setAttribute('aria-hidden', 'true');
    cancelar.append(iconeCancelar, document.createTextNode(' ' + t('Cancelar edição')));
    cancelar.hidden = true;
    cancelar.addEventListener('click', () => { window.location.href = window.location.pathname + window.location.search; });
    botao.before(cancelar);

    const atualizar = () => {
        const editando = !!botao.querySelector('.fa-check, .fa-floppy-disk, .fa-save');
        selo.hidden = !editando;
        cancelar.hidden = !editando;
    };
    new MutationObserver(atualizar).observe(botao, { childList: true, subtree: true, attributes: true });
    // Os scripts das telas ajustam o botão no carregamento; confere depois deles.
    document.addEventListener('DOMContentLoaded', () => setTimeout(atualizar, 0));
    atualizar();
})();
