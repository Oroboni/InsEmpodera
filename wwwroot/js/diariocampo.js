// =========================================================
// FUNÇÕES GLOBAIS DE MODAL (Disponíveis imediatamente)
// =========================================================
/* --- Lógica Atualizada para Checkboxes no Modal --- */
        
        function abrirModalAcao(tipo) {
            document.getElementById('tipoAcaoInput').value = tipo;
            
            // Limpa campos de texto
            document.getElementById('nomeAcao').value = '';
            document.getElementById('provedorAcao').value = '';
            document.getElementById('atorAcao').value = '';
            document.getElementById('quantidadeAcao').value = '1';

            // Limpa os Checkboxes do Modal (desmarca todos)
            const checkboxes = document.querySelectorAll('input[name="modalEixos"]');
            checkboxes.forEach(cb => cb.checked = false);

            document.getElementById('modalAcao').style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }

        function salvarAcaoNoGrid() {
            // 1. Capturar dados básicos
            const tipo = document.getElementById('tipoAcaoInput').value;
            const nome = document.getElementById('nomeAcao').value;
            const provedor = document.getElementById('provedorAcao').value;
            const qtd = document.getElementById('quantidadeAcao').value;

            // 2. Capturar Eixos dos Checkboxes Marcados
            const checkboxesMarcados = document.querySelectorAll('input[name="modalEixos"]:checked');
            
            // Cria arrays de IDs e Nomes
            const eixosIds = Array.from(checkboxesMarcados).map(cb => cb.value);
            const eixosNomes = Array.from(checkboxesMarcados).map(cb => cb.getAttribute('data-nome')).join(', ');

            // 3. Validações
            if (!nome) { alert("O campo Nome é obrigatório."); return; }
            if (eixosIds.length === 0) { alert("Selecione pelo menos um Eixo."); return; }
            if (!provedor) { alert("O campo Provedor Externo é obrigatório."); return; }

            // 4. Definir destino
            let containerID, emptyID, counterSpanID, badgeClass;
            
            if (tipo === 'equipe') {
                containerID = 'container-equipe';
                emptyID = 'empty-equipe';
                counterSpanID = 'count-equipe';
                badgeClass = 'tag-blue';
                countEquipe++;
                document.getElementById(counterSpanID).innerText = countEquipe;
            } else {
                containerID = 'container-institucional';
                emptyID = 'empty-institucional';
                counterSpanID = 'count-institucional';
                badgeClass = 'tag-purple';
                countInst++;
                document.getElementById(counterSpanID).innerText = countInst;
            }

            // 5. Esconder msg vazia
            document.getElementById(emptyID).style.display = 'none';

            // 6. Criar HTML do Item
            const container = document.getElementById(containerID);
            const itemDiv = document.createElement('div');
            itemDiv.className = 'action-list-item';
            
            // Estilos inline
            itemDiv.style.background = '#fff';
            itemDiv.style.padding = '10px 15px';
            itemDiv.style.borderRadius = '8px';
            itemDiv.style.marginBottom = '8px';
            itemDiv.style.border = '1px solid #e0e0e0';
            itemDiv.style.display = 'flex';
            itemDiv.style.justifyContent = 'space-between';
            itemDiv.style.alignItems = 'center';

            const eixoDisplay = eixosNomes ? `<span style="font-size:0.75rem; color:#888;">• ${eixosNomes}</span>` : '';
            const timestamp = Date.now();

            // Gera os inputs ocultos para cada eixo marcado (Backend receberá uma lista)
            let inputsEixos = '';
            eixosIds.forEach((id) => {
                inputsEixos += `<input type="hidden" name="TempAcoes[${timestamp}].FkIdEixo" value="${id}" />`;
            });

            itemDiv.innerHTML = `
                <div style="flex: 1;">
                    <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 4px;">
                        <strong style="color: #333;">${nome}</strong>
                        <span class="tag-item ${badgeClass}" style="font-size: 0.65rem; padding: 2px 6px; border-radius: 4px; background: ${tipo==='equipe'?'#e3f2fd':'#f3e5f5'}; color: ${tipo==='equipe'?'#1565c0':'#7b1fa2'}; border: 1px solid ${tipo==='equipe'?'#90caf9':'#ce93d8'};">
                            ${tipo === 'equipe' ? 'Equipe' : 'Institucional'}
                        </span>
                    </div>
                    <div style="font-size: 0.8rem; color: #666;">
                        <i class="fa-solid fa-building"></i> ${provedor} 
                        ${eixoDisplay}
                        ${qtd > 1 ? ` &bull; <strong>${qtd}x</strong>` : ''}
                    </div>
                </div>
                <button type="button" onclick="removerItemGrid(this, '${tipo}')" style="background: none; border: none; color: #ef5350; cursor: pointer; padding: 5px;" title="Remover">
                    <i class="fa-solid fa-trash"></i>
                </button>
                
                <input type="hidden" name="TempAcoes.Index" value="${timestamp}" />
                <input type="hidden" name="TempAcoes[${timestamp}].Nome" value="${nome}" />
                <input type="hidden" name="TempAcoes[${timestamp}].Provedor" value="${provedor}" />
                <input type="hidden" name="TempAcoes[${timestamp}].Tipo" value="${tipo}" />
                <input type="hidden" name="TempAcoes[${timestamp}].Quantidade" value="${qtd}" />
                ${inputsEixos}
            `;

            container.appendChild(itemDiv);
            fecharModal('modalAcao');
        }
// =========================================================
// LÓGICA APÓS CARREGAMENTO DA PÁGINA
// =========================================================
document.addEventListener("DOMContentLoaded", function () {
    
    // --- 1. LÓGICA DE MENÇÃO (@) ---
    const textarea = document.getElementById('descricaoInput');
    const mentionList = document.getElementById('mention-list');
    const AT_SYMBOL = String.fromCharCode(64); // @ seguro

    // Tenta pegar a lista de atores injetada na View
    const atoresList = typeof atoresDisponiveis !== 'undefined' ? atoresDisponiveis : [];

    if (textarea && mentionList) {
        textarea.addEventListener('input', function (e) {
            const value = this.value;
            const cursorPosition = this.selectionStart;
            const lastAtPos = value.lastIndexOf(AT_SYMBOL, cursorPosition - 1);

            if (lastAtPos !== -1) {
                const query = value.substring(lastAtPos + 1, cursorPosition);
                // Só ativa se não houver espaço após o @
                if (!query.includes(' ')) {
                    showSuggestions(query, lastAtPos);
                } else {
                    mentionList.style.display = 'none';
                }
            } else {
                mentionList.style.display = 'none';
            }
        });

        function showSuggestions(query, atPos) {
            const matches = atoresList.filter(a => {
                const nome = a.Nome || a.Text || "";
                return nome.toLowerCase().includes(query.toLowerCase());
            });

            if (matches.length === 0) {
                mentionList.style.display = 'none';
                return;
            }

            mentionList.innerHTML = matches.map(actor => {
                const nome = actor.Nome || actor.Text || "Ator";
                return `
                    <li class="mention-item" data-name="${nome}">
                        <i class="fa-solid fa-user" style="margin-right:8px; color:#aaa;"></i>
                        <span>${nome}</span>
                    </li>`;
            }).join('');

            mentionList.style.display = 'block';
            mentionList.style.width = textarea.offsetWidth + "px";
        }

        mentionList.addEventListener('click', function (e) {
            const item = e.target.closest('.mention-item');
            if (item) {
                const name = item.getAttribute('data-name');
                const text = textarea.value;
                const cursorPosition = textarea.selectionStart;
                const lastAtPos = text.lastIndexOf(AT_SYMBOL, cursorPosition - 1);
                
                const before = text.substring(0, lastAtPos);
                const after = text.substring(cursorPosition);
                
                textarea.value = before + AT_SYMBOL + name + ' ' + after;
                mentionList.style.display = 'none';
                textarea.focus();
            }
        });

        // Fechar ao clicar fora
        document.addEventListener('click', function (e) {
            if (e.target !== textarea && e.target !== mentionList) {
                mentionList.style.display = 'none';
            }
        });
    }

    // --- 2. MAPA ---
    if (typeof initMapSelector === 'function') {
        initMapSelector('mapa-diario', 'rua');
    }

    // --- 3. LÓGICA DE VIEW vs EDIT (Apenas para tela de Edição) ---
    const editSaveBtn = document.getElementById('edit-save-btn');
    const inputFields = document.querySelectorAll('.clean-input, select.clean-input, textarea, input[type="file"]');
    const editOnlyBtns = document.querySelectorAll('.edit-only-btn');
    let isEditMode = false;

    if (editSaveBtn) {
        // Estado inicial
        setPageState();

        editSaveBtn.addEventListener('click', function (e) {
            if (this.getAttribute('type') === 'button') {
                e.preventDefault();
                isEditMode = true;
                setPageState();
            }
        });
    }

    function setPageState() {
        if (isEditMode) {
            inputFields.forEach(f => {
                if (!f.hasAttribute('readonly')) f.disabled = false;
            });
            editOnlyBtns.forEach(b => b.style.display = 'inline-flex');
            
            editSaveBtn.innerHTML = '<i class="fa-solid fa-check"></i> Salvar Alterações';
            editSaveBtn.setAttribute('type', 'submit');
            editSaveBtn.classList.add('btn-save-final');
            editSaveBtn.classList.remove('btn-next');
        } else {
            if(editSaveBtn) {
                inputFields.forEach(f => f.disabled = true);
                editOnlyBtns.forEach(b => b.style.display = 'none');

                editSaveBtn.innerHTML = '<i class="fa-solid fa-edit"></i> Editar';
                editSaveBtn.setAttribute('type', 'button');
                editSaveBtn.classList.remove('btn-save-final');
                editSaveBtn.classList.add('btn-next');
            }
        }
    }

    // --- 4. FECHAR MODAIS AO CLICAR FORA ---
    document.querySelectorAll('.modal-overlay').forEach(modal => {
        modal.addEventListener('click', function (e) {
            if (e.target === this) fecharModal(this.id);
        });
    });

    // --- 5. MODAL DE EXCLUSÃO ---
    const deleteModal = document.getElementById('deleteConfirmationModal');
    const openDeleteBtn = document.getElementById('openDeleteModalBtn');
    const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');

    if (openDeleteBtn) {
        openDeleteBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (deleteModal) {
                deleteModal.style.display = 'flex';
                setTimeout(() => deleteModal.classList.add('active'), 10);
            }
        });
    }
    if (cancelDeleteBtn) {
        cancelDeleteBtn.addEventListener('click', function() {
            if (deleteModal) {
                deleteModal.classList.remove('active');
                setTimeout(() => deleteModal.style.display = 'none', 300);
            }
        });
    }
});