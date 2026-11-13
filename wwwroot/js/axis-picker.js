document.querySelectorAll('[data-axis-picker]').forEach(picker => {
    const tags = picker.querySelector('[data-axis-tags]');
    const panel = picker.querySelector('.multiselect-panel');
    const options = [...(panel?.querySelectorAll('input[type="checkbox"]') || [])];
    if (!tags) return;

    // Nome do eixo no idioma da pessoa: o data-tag-name vem em português do banco, então passa
    // pelo catálogo de traduções (antes as tags apareciam em português nas telas em inglês/espanhol).
    const nomeDoEixo = option => {
        const nome = option.dataset.tagName || option.value;
        return typeof window.translateText === 'function' ? window.translateText(nome) : nome;
    };

    const render = () => {
        tags.replaceChildren();
        options.forEach(option => {
            option.closest('li')?.classList.toggle('is-selected', option.checked);
            if (!option.checked) return;

            const tag = document.createElement('span');
            tag.className = 'tag-item tag-purple';
            tag.dataset.i18nText = ''; // traduzido também se o catálogo chegar depois
            tag.append(document.createTextNode(nomeDoEixo(option)));
            const remove = document.createElement('button');
            remove.type = 'button';
            remove.className = 'tag-remove-btn';
            remove.setAttribute('aria-label', `${window.translateText ? window.translateText('Remover eixo') : 'Remover eixo'} ${nomeDoEixo(option)}`);
            remove.textContent = '×';
            remove.addEventListener('click', () => {
                if (option.disabled) return;
                option.checked = false;
                option.dispatchEvent(new Event('change', { bubbles: true }));
            });
            tag.append(remove);
            tags.append(tag);
        });
    };

    picker.addEventListener('change', render);
    panel?.addEventListener('click', event => {
        const row = event.target.closest('li');
        if (!row || event.target.closest('label, input, button')) return;
        const option = row.querySelector('input[type="checkbox"]');
        if (!option || option.disabled) return;
        option.checked = !option.checked;
        option.dispatchEvent(new Event('change', { bubbles: true }));
    });
    render();
});
