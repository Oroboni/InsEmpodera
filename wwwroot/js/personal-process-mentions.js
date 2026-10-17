// Menções a atores (@) no Diário de Processo Pessoal.
// Digitar "@" abre a lista; o texto seguinte filtra os atores sem diferenciar acentos/maiúsculas
// e aceita letras em sequência (ex.: "isao" encontra "Isadora"). Setas navegam, Enter/Tab escolhem.
(() => {
    const campo = document.querySelector('[data-actor-mentions]');
    const lista = document.getElementById('actor-mention-list');
    const dados = document.getElementById('actor-mention-data');
    if (!campo || !lista || !dados) return;

    let atores;
    try { atores = JSON.parse(dados.textContent); } catch { return; }
    if (!Array.isArray(atores)) return;

    const LIMITE = 8;
    const normalizar = texto => String(texto ?? '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLocaleLowerCase();
    const indice = atores.map(ator => ({ ator, chave: normalizar(ator.nome) }))
        .sort((a, b) => a.chave.localeCompare(b.chave));

    let inicio = -1;
    let fim = -1;
    let opcoes = [];
    let ativa = -1;

    // Quanto menor, mais relevante: início do nome, início de uma palavra, contém, letras em sequência.
    function pontuar(chave, busca) {
        if (!busca) return 0;
        if (chave.startsWith(busca)) return 0;
        if (chave.split(/\s+/).some(palavra => palavra.startsWith(busca))) return 1;
        if (chave.includes(busca)) return 2;
        // Letras em sequência: quanto mais juntas e mais perto do início, melhor.
        let posicao = 0, primeira = -1, lacunas = 0, anterior = -1;
        for (const letra of busca.replace(/\s+/g, '')) {
            posicao = chave.indexOf(letra, posicao);
            if (posicao < 0) return -1;
            if (primeira < 0) primeira = posicao; else lacunas += posicao - anterior - 1;
            anterior = posicao;
            posicao++;
        }
        return 3 + (primeira + lacunas) / 100;
    }

    function fechar() {
        lista.hidden = true;
        lista.replaceChildren();
        opcoes = [];
        ativa = -1;
        campo.removeAttribute('aria-activedescendant');
        campo.setAttribute('aria-expanded', 'false');
    }

    function marcar(posicao) {
        if (!opcoes.length) return;
        ativa = (posicao + opcoes.length) % opcoes.length;
        opcoes.forEach((botao, i) => botao.classList.toggle('is-active', i === ativa));
        campo.setAttribute('aria-activedescendant', opcoes[ativa].id);
        opcoes[ativa].scrollIntoView?.({ block: 'nearest' });
    }

    function selecionar(ator) {
        if (inicio < 0) return;
        const token = `@[${ator.nome}](ator:${ator.id})`;
        campo.setRangeText(`${token} `, inicio, fim, 'end');
        campo.dispatchEvent(new Event('change', { bubbles: true }));
        fechar();
        campo.focus();
    }

    function itemInformativo(texto, classe) {
        const item = document.createElement('li');
        item.className = classe;
        item.setAttribute('role', 'presentation');
        item.textContent = texto;
        return item;
    }

    function atualizar() {
        const antes = campo.value.slice(0, campo.selectionStart);
        const match = /(?:^|\s)@([^@\r\n]{0,60})$/.exec(antes);
        if (!match || match[1].includes('](')) { fechar(); return; }

        inicio = antes.length - match[1].length - 1;
        fim = campo.selectionStart;
        const textoBusca = match[1].trim();
        const busca = normalizar(textoBusca);
        const encontrados = indice
            .map(item => ({ ...item, pontos: pontuar(item.chave, busca) }))
            .filter(item => item.pontos >= 0)
            .sort((a, b) => a.pontos - b.pontos || a.chave.localeCompare(b.chave))
            .slice(0, LIMITE)
            .map(item => item.ator);

        lista.replaceChildren(itemInformativo(textoBusca ? `Mencionar ator: "${textoBusca}"` : 'Mencionar ator', 'actor-mention-header'));
        opcoes = encontrados.map((ator, i) => {
            const item = document.createElement('li');
            item.setAttribute('role', 'presentation');
            const botao = document.createElement('button');
            botao.type = 'button';
            botao.id = `actor-mention-opcao-${i}`;
            botao.className = 'actor-mention-option';
            botao.setAttribute('role', 'option');
            botao.textContent = ator.nome;
            botao.addEventListener('mousedown', event => event.preventDefault()); // mantém o foco no campo
            botao.addEventListener('click', () => selecionar(ator));
            item.appendChild(botao);
            lista.appendChild(item);
            return botao;
        });
        if (!opcoes.length) {
            lista.appendChild(itemInformativo(
                atores.length ? 'Nenhum ator encontrado. Confira o nome ou cadastre o ator em Atores.' : 'Nenhum ator cadastrado.',
                'actor-mention-empty'));
        }
        const estavaFechada = lista.hidden;
        lista.hidden = false;
        campo.setAttribute('aria-expanded', 'true');
        if (estavaFechada) lista.scrollIntoView?.({ block: 'nearest' });
        if (opcoes.length) marcar(0);
    }

    campo.setAttribute('aria-autocomplete', 'list');
    campo.setAttribute('aria-expanded', 'false');
    campo.addEventListener('input', atualizar);
    campo.addEventListener('click', atualizar);
    campo.addEventListener('keydown', event => {
        if (lista.hidden) return;
        if (event.key === 'Escape') { event.preventDefault(); fechar(); return; }
        if (!opcoes.length) return;
        if (event.key === 'ArrowDown') { event.preventDefault(); marcar(ativa + 1); }
        else if (event.key === 'ArrowUp') { event.preventDefault(); marcar(ativa - 1); }
        else if (event.key === 'Enter' || event.key === 'Tab') {
            event.preventDefault();
            opcoes[Math.max(ativa, 0)].click();
        }
    });
    document.addEventListener('click', event => {
        if (event.target !== campo && !lista.contains(event.target)) fechar();
    });
})();
