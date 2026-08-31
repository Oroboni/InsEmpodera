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

            // Create usa checkboxes múltiplos; Edit usa um select simples.
            const checkboxes = document.querySelectorAll('input[name="modalEixos"]');
            checkboxes.forEach(cb => cb.checked = false);
            const eixoSelect = document.getElementById('eixoAcao');
            if (eixoSelect) eixoSelect.value = '';

            const modal = document.getElementById('modalAcao');
            modal.style.display = 'flex';
            modal.classList.add('active');
            document.body.style.overflow = 'hidden';
            setTimeout(() => document.getElementById('nomeAcao')?.focus(), 0);
        }

        function fecharModal(modalId) {
            const modal = document.getElementById(modalId);
            if (!modal) return;
            modal.style.display = 'none';
            modal.classList.remove('active');
            document.body.style.overflow = 'auto';
        }

        function removerItemGrid(btn, tipo) {
            const item = btn?.closest?.('.action-list-item');
            if (!item) return;
            item.remove();

            const isEquipe = tipo === 'equipe';
            const countKey = isEquipe ? 'countEquipe' : 'countInst';
            const counterId = isEquipe ? 'count-equipe' : 'count-institucional';
            const emptyId = isEquipe ? 'empty-equipe' : 'empty-institucional';
            window[countKey] = Math.max(0, Number(window[countKey] || 0) - 1);
            const counter = document.getElementById(counterId);
            if (counter) counter.innerText = window[countKey];
            const empty = document.getElementById(emptyId);
            if (empty && window[countKey] === 0) empty.style.display = 'block';
        }

        function salvarAcaoNoGrid() {
            // 1. Capturar dados básicos
            const tipo = document.getElementById('tipoAcaoInput').value;
            const nome = document.getElementById('nomeAcao').value;
            const provedor = document.getElementById('provedorAcao').value;
            const qtd = document.getElementById('quantidadeAcao').value;
            const atorId = document.getElementById('atorAcao')?.value ?? '';

            // 2. Capturar Eixos dos Checkboxes Marcados
            const checkboxesMarcados = document.querySelectorAll('input[name="modalEixos"]:checked');
            let eixosIds = Array.from(checkboxesMarcados).map(cb => cb.value);
            let eixosNomes = Array.from(checkboxesMarcados).map(cb => cb.getAttribute('data-nome') || '');

            const eixoSelect = document.getElementById('eixoAcao');
            if (eixosIds.length === 0 && eixoSelect?.value) {
                eixosIds = [eixoSelect.value];
                eixosNomes = [eixoSelect.options[eixoSelect.selectedIndex]?.textContent?.trim() || ''];
            }
            eixosNomes = eixosNomes.filter(Boolean).join(', ');
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
                window.countEquipe = Number(window.countEquipe || 0) + 1;
                const counter = document.getElementById(counterSpanID);
                if (counter) counter.innerText = window.countEquipe;
            } else {
                containerID = 'container-institucional';
                emptyID = 'empty-institucional';
                counterSpanID = 'count-institucional';
                badgeClass = 'tag-purple';
                window.countInst = Number(window.countInst || 0) + 1;
                const counter = document.getElementById(counterSpanID);
                if (counter) counter.innerText = window.countInst;
            }

            // 5. Esconder msg vazia
            const emptyState = document.getElementById(emptyID);
            if (emptyState) emptyState.style.display = 'none';

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

            const timestamp = Date.now();
            itemDiv.innerHTML = `
                <div style="flex: 1;">
                    <div data-action-heading style="display: flex; align-items: center; gap: 8px; margin-bottom: 4px;">
                        <strong data-action-name style="color: #333;"></strong>
                        <span data-action-type class="tag-item" style="font-size: 0.65rem; padding: 2px 6px; border-radius: 4px;"></span>
                    </div>
                    <div data-action-details style="font-size: 0.8rem; color: #666;">
                        <i class="fa-solid fa-building"></i>
                        <span data-action-provider></span>
                        <span data-action-axes style="font-size:0.75rem; color:#888;"></span>
                        <span data-action-quantity></span>
                    </div>
                </div>
                <button type="button" data-action-remove style="background: none; border: none; color: #ef5350; cursor: pointer; padding: 5px;" title="Remover">
                    <i class="fa-solid fa-trash"></i>
                </button>`;

            itemDiv.querySelector('[data-action-name]').textContent = nome;
            itemDiv.querySelector('[data-action-provider]').textContent = ` ${provedor}`;
            itemDiv.querySelector('[data-action-axes]').textContent = eixosNomes ? ` • ${eixosNomes}` : '';
            itemDiv.querySelector('[data-action-quantity]').textContent = Number(qtd) > 1 ? ` • ${qtd}x` : '';

            const badge = itemDiv.querySelector('[data-action-type]');
            badge.classList.add(badgeClass);
            badge.textContent = tipo === 'equipe' ? 'Equipe' : 'Institucional';
            badge.style.background = tipo === 'equipe' ? '#e3f2fd' : '#f3e5f5';
            badge.style.color = tipo === 'equipe' ? '#1565c0' : '#7b1fa2';
            badge.style.border = `1px solid ${tipo === 'equipe' ? '#90caf9' : '#ce93d8'}`;

            const removeButton = itemDiv.querySelector('[data-action-remove]');
            removeButton.setAttribute('aria-label', `Remover ação ${nome}`);
            removeButton.addEventListener('click', () => removerItemGrid(removeButton, tipo));

            function appendHiddenInput(name, value) {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                input.value = String(value ?? '');
                itemDiv.appendChild(input);
            }

            appendHiddenInput('TempAcoes.Index', timestamp);
            appendHiddenInput(`TempAcoes[${timestamp}].Nome`, nome);
            appendHiddenInput(`TempAcoes[${timestamp}].Provedor`, provedor);
            appendHiddenInput(`TempAcoes[${timestamp}].Tipo`, tipo);
            appendHiddenInput(`TempAcoes[${timestamp}].Quantidade`, qtd);
            appendHiddenInput(`TempAcoes[${timestamp}].FkIdAtor`, atorId);
            eixosIds.forEach(id => appendHiddenInput(`TempAcoes[${timestamp}].FkIdEixo`, id));
            container.appendChild(itemDiv);
            fecharModal('modalAcao');
        }
// =========================================================
// LÓGICA APÓS CARREGAMENTO DA PÁGINA
// =========================================================
document.addEventListener("DOMContentLoaded", function () {
    const countEquipeElement = document.getElementById('count-equipe');
    if (countEquipeElement) countEquipeElement.innerText = Number(window.countEquipe || 0);
    const countInstElement = document.getElementById('count-institucional');
    if (countInstElement) countInstElement.innerText = Number(window.countInst || 0);
    
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

            mentionList.replaceChildren();
            matches.forEach(actor => {
                const nome = String(actor.Nome || actor.Text || "Ator");
                const item = document.createElement('li');
                item.className = 'mention-item';
                item.dataset.name = nome;

                const icon = document.createElement('i');
                icon.className = 'fa-solid fa-user';
                icon.style.marginRight = '8px';
                icon.style.color = '#aaa';

                const label = document.createElement('span');
                label.textContent = nome;
                item.append(icon, label);
                mentionList.appendChild(item);
            });
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
                
                const separator = after.startsWith(' ') ? '' : ' ';
                textarea.value = before + AT_SYMBOL + name + separator + after;
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
    if (typeof initMapSelector === 'function' && document.getElementById('mapa-diario')) {
        initMapSelector('mapa-diario', 'rua', {
            sourceInputId: 'rua',
            manualInputId: 'rua',
            showSearchControl: false
        });
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

    // --- 4. FECHAR MODAL DE AÇÃO ---
    const actionModal = document.getElementById('modalAcao');
    if (actionModal) {
        actionModal.addEventListener('click', function (e) {
            if (e.target === this) fecharModal('modalAcao');
        });
    }

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
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        if (actionModal?.style.display === 'flex') fecharModal('modalAcao');
        if (deleteModal?.classList.contains('active')) {
            deleteModal.classList.remove('active');
            setTimeout(() => deleteModal.style.display = 'none', 300);
        }
    });
});
