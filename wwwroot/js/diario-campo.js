//indexe

document.addEventListener('DOMContentLoaded', function() {
    const searchForm = document.querySelector('.search-form');
    if (!searchForm) return;
    const input = searchForm.querySelector('input[name="search"]');
    input.addEventListener('keypress', function(e){
        if (e.key === 'Enter') searchForm.submit();
    });
});

// diario

document.addEventListener('DOMContentLoaded', function() {
    // Buscar CEP
    const btnCep = document.getElementById('btnBuscarCep');
    if (btnCep) {
        btnCep.addEventListener('click', async () => {
            const cepInput = document.getElementById('Localizacao');
            const cep = cepInput.value.replace(/\D/g,'');
            if (!cep) return alert('Informe o CEP.');
            try {
                const res = await fetch(`/DiarioCampo/BuscarCep?cep=${cep}`);
                if (!res.ok) throw new Error('Erro no ViaCEP');
                const j = await res.json();
                const out = document.getElementById('enderecoResult');
                if (j.erro) out.innerText = 'CEP não encontrado';
                else out.innerText = `${j.logradouro}, ${j.bairro} - ${j.localidade}/${j.uf}`;
            } catch (err) {
                console.error(err);
                alert('Não foi possível buscar o CEP.');
            }
        });
    }

    // Upload de anexos preview (no Create/Edit)
    const anexosInput = document.getElementById('anexos');
    if (anexosInput) {
        const preview = document.getElementById('anexosPreview');
        anexosInput.addEventListener('change', () => {
            if (!preview) return;
            preview.innerHTML = '';
            Array.from(anexosInput.files).forEach(f => {
                const div = document.createElement('div');
                div.className = 'anexo-preview';
                div.innerText = f.name;
                preview.appendChild(div);
            });
        });
    }

    // Remover anexo via AJAX (no Edit)
    document.querySelectorAll('.btn-remove-anexo').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const id = btn.getAttribute('data-id');
            if (!confirm('Remover anexo?')) return;
            const res = await fetch('/DiarioCampo/RemoverAnexo', {
                method: 'POST',
                headers: {'Content-Type': 'application/x-www-form-urlencoded'},
                body: `id=${id}`
            });
            const j = await res.json();
            if (j.success) {
                btn.closest('.anexo-item').remove();
            } else {
                alert('Erro ao remover.');
            }
        });
    });
});

//tags

document.addEventListener('DOMContentLoaded', function() {
    const textarea = document.getElementById('Descricao');
    if (!textarea) return;

    let mentionBox = document.createElement('div');
    mentionBox.className = 'mention-box';
    mentionBox.style.position = 'absolute';
    mentionBox.style.zIndex = 9999;
    mentionBox.style.display = 'none';
    document.body.appendChild(mentionBox);

    let currentType = 'ator'; // default

    textarea.addEventListener('keyup', async (e) => {
        const pos = textarea.selectionStart;
        const text = textarea.value.slice(0, pos);
        const atIndex = text.lastIndexOf('@');
        if (atIndex === -1) {
            mentionBox.style.display = 'none';
            return;
        }
        const term = text.slice(atIndex + 1);
        if (term.length === 0) {
            // show suggestions base (atores)
            // keep hidden until user types something meaningful
            return;
        }
        // guess type by prefix: @a:ator @c:comunidade @t:atividade (optional)
        if (term.startsWith('c:')) { currentType = 'comunidade'; }
        else if (term.startsWith('t:')) { currentType = 'atividade'; }
        else currentType = 'ator';

        const cleaned = term.replace(/^c:|^t:/, '');

        const res = await fetch(`/DiarioCampo/Autocomplete?q=${encodeURIComponent(cleaned)}&type=${currentType}`);
        const items = await res.json();
        if (!items || items.length === 0) { mentionBox.style.display = 'none'; return; }

        mentionBox.innerHTML = '';
        items.forEach(it => {
            const div = document.createElement('div');
            div.className = 'mention-item';
            div.innerText = it.label;
            div.dataset.id = it.id;
            div.addEventListener('click', () => {
                // insert mention as HTML-like tag
                const before = textarea.value.slice(0, atIndex);
                const after = textarea.value.slice(pos);
                const mentionText = `@${it.label}`;
                textarea.value = before + mentionText + ' ' + after;
                mentionBox.style.display = 'none';
                textarea.focus();
            });
            mentionBox.appendChild(div);
        });

        // position the box near the textarea caret (simple placement)
        const rect = textarea.getBoundingClientRect();
        mentionBox.style.left = (rect.left + 10) + 'px';
        mentionBox.style.top = (rect.top + 30) + 'px';
        mentionBox.style.display = 'block';
    });

    document.addEventListener('click', (e) => {
        if (!mentionBox.contains(e.target)) mentionBox.style.display = 'none';
    });
});
