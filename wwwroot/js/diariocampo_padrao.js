/* ==========================================================================
   DIARIOCAMPO.JS - Lógica completa para Create/Edit
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    
    // Inicialização de Listeners Globais (ex: fechar modal clicando fora)
    const modalAcao = document.getElementById('modalAcao');
    if (modalAcao) {
        modalAcao.addEventListener('click', function(e) {
            if (e.target === modalAcao) {
                fecharModal('modalAcao');
            }
        });
    }

    // Inicialização da Lógica de Menções (@)
    initMentions();
});

/* =========================================
   1. LÓGICA DE MENÇÃO (@)
   Adaptada para o ID correto do seu HTML: 'descricaoInput'
   ========================================= */
function initMentions() {
    const textarea = document.getElementById('descricaoInput'); // ID correto do Create.cshtml
    const listContainer = document.getElementById('mention-list'); // ID correto do Create.cshtml
    
    // Verifica se a variável global atoresDisponiveis foi populada na View
    if (textarea && listContainer && typeof atoresDisponiveis !== 'undefined') {
        
        textarea.addEventListener('input', function(e) {
            const cursorPosition = this.selectionStart;
            const textBeforeCursor = this.value.substring(0, cursorPosition);
            const atSymbolIndex = textBeforeCursor.lastIndexOf('@');
            
            // Se digitou @
            if (atSymbolIndex !== -1) {
                const query = textBeforeCursor.substring(atSymbolIndex + 1);
                
                // Validação: @ deve estar no inicio ou ter espaço antes, e query sem espaço
                if ((atSymbolIndex === 0 || textBeforeCursor[atSymbolIndex - 1] === ' ' || textBeforeCursor[atSymbolIndex - 1] === '\n') && !query.includes(' ')) {
                    showMentions(query, atSymbolIndex);
                } else {
                    hideMentions();
                }
            } else {
                hideMentions();
            }
        });

        function showMentions(query, atIndex) {
            // Filtra atores
            const matches = atoresDisponiveis.filter(a => (a?.Text ?? a?.Nome ?? "").toLowerCase().includes(query.toLowerCase()));
            
            if (matches.length === 0) {
                hideMentions();
                return;
            }

            listContainer.replaceChildren();
            listContainer.style.display = 'block';

            matches.forEach(ator => {
                const nome = String(ator.Text ?? ator.Nome ?? '');
                const li = document.createElement('li');
                li.className = 'mention-item';

                const icon = document.createElement('i');
                icon.className = 'fa-solid fa-user';
                li.append(icon, document.createTextNode(` ${nome}`));
                li.addEventListener('click', function () {
                    insertMention(nome, atIndex, query.length);
                });
                listContainer.appendChild(li);
            });
        }
        function hideMentions() {
            listContainer.style.display = 'none';
        }

        function insertMention(name, atIndex, queryLength) {
            const text = textarea.value;
            const before = text.substring(0, atIndex);
            const after = text.substring(atIndex + 1 + queryLength);
            
            // Insere o nome e foca
            const separator = after.startsWith(' ') ? '' : ' ';
            textarea.value = before + '@' + name + separator + after;
            hideMentions();
            textarea.focus();
            
            // Ajusta cursor para depois do nome
            const newCursorPos = atIndex + 1 + name.length + separator.length;
            textarea.setSelectionRange(newCursorPos, newCursorPos);
        }
    }
}

/* =========================================
   2. LÓGICA DE MODAIS (Abertura/Fechamento)
   ========================================= */

// Torna as funções acessíveis globalmente (window) para o onclick do HTML funcionar
window.abrirModalAcao = function() {
    const modal = document.getElementById('modalAcao');
    if (modal) {
        // Usa flex para centralizar conforme seu CSS .modal-overlay
        modal.style.display = 'flex'; 
        modal.classList.add('active'); // Caso use animação
        document.body.style.overflow = 'hidden'; // Trava rolagem do fundo
    }
};

window.fecharModal = function(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        modal.classList.remove('active');
        document.body.style.overflow = 'auto'; // Destrava rolagem
        
        if (modalId === 'modalAcao') {
            limparFormularioAcao();
        }
    }
};

window.limparFormularioAcao = function() {
    const ids = ['tipoAcao', 'nomeAcao', 'eixoAcao', 'provedorAcao'];
    ids.forEach(id => {
        const el = document.getElementById(id);
        if(el) el.value = '';
    });
    
    const qtd = document.getElementById('quantidadeAcao');
    if(qtd) qtd.value = '1';
    
    toggleTipoAcao(); // Reseta visibilidade do provedor
};

/* =========================================
   3. LÓGICA DO FORMULÁRIO DE AÇÃO
   ========================================= */

window.toggleTipoAcao = function() {
    const tipo = document.getElementById('tipoAcao').value;
    const provedorCard = document.getElementById('provedorCard'); // ID da div wrapper ou input
    const provedorRequired = document.getElementById('provedorRequired');
    
    // Lógica para mostrar/ocultar asterisco ou campo
    if (provedorRequired) {
        provedorRequired.style.display = (tipo === 'institucional') ? 'inline' : 'none';
    }
    
    // Se quiser ocultar o campo todo quando for Equipe (opcional, baseado no seu HTML)
    // O seu HTML atual mostra o campo sempre, apenas muda a obrigatoriedade visual
};

window.salvarAcao = function() {
    // 1. Obter valores
    const tipo = document.getElementById('tipoAcao').value;
    const nome = document.getElementById('nomeAcao').value;
    const eixo = document.getElementById('eixoAcao').value;
    const provedor = document.getElementById('provedorAcao').value;
    const quantidade = document.getElementById('quantidadeAcao').value;

    // 2. Validação
    if (!tipo) return alert('Selecione o tipo de ação');
    if (!nome) return alert('O nome da ação é obrigatório');
    if (!eixo) return alert('Selecione um eixo');
    if (tipo === 'institucional' && !provedor) return alert('Para ações institucionais, o provedor é obrigatório');

    // 3. Define o Container
    const containerId = (tipo === 'equipe') ? 'atividadesContainer' : 'acoesInstitucionaisContainer';
    const container = document.getElementById(containerId);
    
    // 4. Remove estado vazio se existir
    const emptyState = container.querySelector('.empty-state-acoes');
    if (emptyState) emptyState.remove();

    // 5. Cria o elemento HTML da linha
    const item = document.createElement('div');
    item.className = 'action-list-item';
    
    const isTeam = tipo === 'equipe';
    const index = Date.now();
    item.innerHTML = `
        <div data-action-content style="flex: 1;">
            <div style="display:flex; align-items:center; gap:8px; margin-bottom:4px;">
                <strong data-action-name></strong>
                <span data-action-badge class="tag-item" style="font-size:0.75rem"><i></i><span></span></span>
                <em data-action-quantity style="margin-left:5px; color:#666"></em>
            </div>
            <small data-action-provider class="text-muted"><i class="fa-solid fa-building"></i><span></span></small>
        </div>
        <button type="button" class="btn-action btn-delete" title="Remover" aria-label="Remover ação">
            <i class="fa-solid fa-trash"></i>
        </button>`;

    item.querySelector('[data-action-name]').textContent = nome;
    const badge = item.querySelector('[data-action-badge]');
    badge.classList.add(isTeam ? 'tag-blue' : 'tag-purple');
    badge.querySelector('i').className = isTeam ? 'fa-solid fa-users' : 'fa-solid fa-building';
    badge.querySelector('span').textContent = isTeam ? ' Equipe' : ' Institucional';
    item.querySelector('[data-action-quantity]').textContent = Number(quantidade) > 1 ? `(${quantidade}x)` : '';

    const provider = item.querySelector('[data-action-provider]');
    provider.style.display = provedor ? '' : 'none';
    provider.querySelector('span').textContent = ` ${provedor}`;
    item.querySelector('.btn-delete').addEventListener('click', () => item.remove());

    function appendHiddenInput(name, value) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = String(value ?? '');
        item.querySelector('[data-action-content]').appendChild(input);
    }
    appendHiddenInput(`Acoes[${index}].Nome`, nome);
    appendHiddenInput(`Acoes[${index}].Tipo`, tipo);
    // 6. Adiciona e fecha
    container.appendChild(item);
    fecharModal('modalAcao');
};

/* =========================================
   4. BUSCA DE CEP (ViaCEP)
   ========================================= */
window.buscarCEP = function() {
    const cepInput = document.getElementById('cep');
    const ruaInput = document.getElementById('rua');
    
    if (!cepInput) return;
    
    let cep = cepInput.value.replace(/\D/g, '');

    if (cep !== "") {
        let validacep = /^[0-9]{8}$/;

        if(validacep.test(cep)) {
            ruaInput.value = "Pesquisando...";
            
            fetch(`https://viacep.com.br/ws/${cep}/json/`)
                .then(response => response.json())
                .then(data => {
                    if (!data.erro) {
                        ruaInput.value = `${data.logradouro}, ${data.bairro}, ${data.localidade} - ${data.uf}`;
                        // Se houver inputs separados para bairro/cidade, preencha-os aqui
                        // document.getElementById('bairro').value = data.bairro;
                    } else {
                        ruaInput.value = "";
                        alert("CEP não encontrado.");
                    }
                })
                .catch(() => {
                    ruaInput.value = "";
                    alert("Erro ao buscar CEP.");
                });
        } else {
            alert("Formato de CEP inválido.");
        }
    }
};