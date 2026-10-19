// Menções a atores (@) no Diário de Processo Pessoal.
// Digitar "@" abre a lista; o texto seguinte filtra os atores sem diferenciar acentos/maiúsculas
// e aceita letras em sequência (ex.: "isao" encontra "Isadora"). Setas navegam, Enter/Tab escolhem.
// No campo a menção aparece só como "@Nome", destacada na cor do sistema; o formato salvo
// "@[Nome](ator:id)" (usado para gerar o link na leitura) é montado apenas no envio do formulário.
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

    // Nome exibido -> id do ator, para remontar o formato salvo no envio.
    const mencoes = new Map();
    const TOKEN = /@\[([^\]\r\n]{1,120})\]\(ator:([^)\s]+)\)/g;
    const escaparRegex = texto => texto.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    
    function padraoMencoes() {
        if (!mencoes.size) return null;
        const nomes = [...mencoes.keys()].sort((a, b) => b.length - a.length).map(escaparRegex);
        return new RegExp(`@(${nomes.join('|')})(?![\\p{L}\\p{N}_])`, 'gu');
    }

    // Texto do campo -> formato salvo.
    function serializar(texto) {
        const padrao = padraoMencoes();
        return padrao ? texto.replace(padrao, (_, nome) => `@[${nome}](ator:${mencoes.get(nome)})`) : texto;
    }

    // Camada atrás do campo que pinta as menções (o texto do campo fica transparente por cima).
    const editor = document.createElement('div');
    editor.className = 'actor-mention-editor';
    const fundo = document.createElement('div');
    fundo.className = 'actor-mention-backdrop';
    fundo.setAttribute('aria-hidden', 'true');
    campo.parentNode.insertBefore(editor, campo);
    editor.append(fundo, campo);

    function copiarEstilo() {
        const estilo = getComputedStyle(campo);
        ['fontFamily', 'fontSize', 'fontWeight', 'lineHeight', 'letterSpacing', 'wordSpacing', 'textAlign', 'tabSize',
            'paddingTop', 'paddingRight', 'paddingBottom', 'paddingLeft',
            'borderTopWidth', 'borderRightWidth', 'borderBottomWidth', 'borderLeftWidth', 'boxSizing']
            .forEach(prop => { fundo.style[prop] = estilo[prop]; });
    }

    function pintar() {
        const padrao = padraoMencoes();
        const texto = campo.value;
        const partes = [];
        let ultimo = 0;
        if (padrao) {
            for (const achado of texto.matchAll(padrao)) {
                partes.push(document.createTextNode(texto.slice(ultimo, achado.index)));
                const destaque = document.createElement('mark');
                destaque.className = 'actor-mention-chip';
                destaque.textContent = achado[0];
                partes.push(destaque);
                ultimo = achado.index + achado[0].length;
            }
        }
        partes.push(document.createTextNode(texto.slice(ultimo) + '\u200b'));
        fundo.replaceChildren(...partes);
        // Com barra de rolagem o campo tem menos largura útil; a camada de trás acompanha.
        const estilo = getComputedStyle(campo);
        const barra = campo.offsetWidth - campo.clientWidth - parseFloat(estilo.borderLeftWidth) - parseFloat(estilo.borderRightWidth);
        fundo.style.paddingRight = `${parseFloat(estilo.paddingRight) + Math.max(barra, 0)}px`;
        fundo.scrollTop = campo.scrollTop;
    }

    // Valor vindo do servidor: "@[Nome](ator:id)" vira "@Nome" no campo.
    campo.value = campo.value.replace(TOKEN, (_, nome, id) => { mencoes.set(nome, id); return `@${nome}`; });
    copiarEstilo();
    pintar();
    campo.addEventListener('input', pintar);
    campo.addEventListener('scroll', () => { fundo.scrollTop = campo.scrollTop; });
    window.addEventListener('resize', () => { copiarEstilo(); pintar(); });

    // No envio, o servidor recebe o formato salvo; o que a pessoa vê no campo não muda.
    campo.form?.addEventListener('formdata', event => event.formData.set(campo.name, serializar(campo.value)));

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
        mencoes.set(ator.nome, ator.id);
        campo.setRangeText(`@${ator.nome} `, inicio, fim, 'end');
        pintar();
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
        // Depois de uma menção já escolhida ("@Ana Souza continua..."), o texto seguinte não é busca.
        const jaMencionado = match && [...mencoes.keys()].some(nome => match[1].startsWith(nome) && /^\s/.test(match[1].slice(nome.length)));
        if (!match || jaMencionado) { fechar(); return; }

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
