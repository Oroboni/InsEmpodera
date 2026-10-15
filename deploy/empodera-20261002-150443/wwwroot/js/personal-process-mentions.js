(() => {
    const campo = document.querySelector('[data-actor-mentions]');
    const lista = document.getElementById('actor-mention-list');
    const dados = document.getElementById('actor-mention-data');
    if (!campo || !lista || !dados) return;

    let atores;
    try { atores = JSON.parse(dados.textContent); } catch { return; }
    if (!Array.isArray(atores)) return;

    let inicio = -1;
    let fim = -1;

    function fechar() {
        lista.hidden = true;
        lista.replaceChildren();
        campo.removeAttribute('aria-activedescendant');
    }

    function selecionar(ator) {
        if (inicio < 0) return;
        const token = `@[${ator.nome}](ator:${ator.id})`;
        campo.setRangeText(`${token} `, inicio, fim, 'end');
        campo.dispatchEvent(new Event('change', { bubbles: true }));
        fechar();
        campo.focus();
    }

    function atualizar() {
        const antes = campo.value.slice(0, campo.selectionStart);
        const match = /(?:^|\s)@([^@\r\n]{0,60})$/.exec(antes);
        if (!match || match[1].includes('](')) { fechar(); return; }

        inicio = antes.length - match[1].length - 1;
        fim = campo.selectionStart;
        const busca = match[1].trim().toLocaleLowerCase();
        const encontrados = atores.filter(a =>
            String(a.nome).toLocaleLowerCase().includes(busca)).slice(0, 8);
        lista.replaceChildren();
        for (const ator of encontrados) {
            const item = document.createElement('li');
            const botao = document.createElement('button');
            botao.type = 'button';
            botao.className = 'actor-mention-option';
            botao.setAttribute('role', 'option');
            botao.textContent = ator.nome;
            botao.addEventListener('click', () => selecionar(ator));
            item.appendChild(botao);
            lista.appendChild(item);
        }
        lista.hidden = encontrados.length === 0;
    }

    campo.addEventListener('input', atualizar);
    campo.addEventListener('click', atualizar);
    campo.addEventListener('keydown', event => {
        if (event.key === 'Escape') fechar();
        if (event.key === 'Enter' && !lista.hidden) {
            const primeiro = lista.querySelector('button');
            if (primeiro) { event.preventDefault(); primeiro.click(); }
        }
    });
    document.addEventListener('click', event => {
        if (event.target !== campo && !lista.contains(event.target)) fechar();
    });
})();
