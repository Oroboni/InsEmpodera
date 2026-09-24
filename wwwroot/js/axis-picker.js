document.querySelectorAll('[data-axis-picker]').forEach(picker => {
    const tags = picker.querySelector('[data-axis-tags]');
    const panel = picker.querySelector('.multiselect-panel');
    const options = [...(panel?.querySelectorAll('input[type="checkbox"]') || [])];
    if (!tags) return;

    const render = () => {
        tags.replaceChildren();
        options.forEach(option => {
            option.closest('li')?.classList.toggle('is-selected', option.checked);
            if (!option.checked) return;

            const tag = document.createElement('span');
            tag.className = 'tag-item tag-purple';
            tag.append(document.createTextNode(option.dataset.tagName || option.value));
            const remove = document.createElement('button');
            remove.type = 'button';
            remove.className = 'tag-remove-btn';
            remove.setAttribute('aria-label', `Remover eixo ${option.dataset.tagName || option.value}`);
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
