// Rede Primária e Rede Subjetiva da tela de Atores (Views/Atores/_RedesModais.cshtml).
// Fluxo: botão da rede -> lista (Ator | Tipo de Relação) -> Adicionar -> formulário -> Salvar -> lista.
// Os vínculos ficam em memória e são enviados junto com o formulário principal como VinculosRede[i].*.
(function () {
    "use strict";

    const NOMES_REDE = { Primaria: "Rede Primária", Subjetiva: "Rede Subjetiva" };
    const estado = { editavel: true, aplicar: null };

    // Permite que a tela de edição habilite/desabilite as alterações junto com o botão "Editar".
    window.AtoresRedes = {
        setEditavel(valor) {
            estado.editavel = !!valor;
            if (estado.aplicar) estado.aplicar();
        }
    };

    function init() {
        const dadosEl = document.getElementById("redes-dados");
        const campos = document.querySelector("[data-redes-campos]");
        if (!dadosEl || !campos) return;

        let dados = {};
        try { dados = JSON.parse(dadosEl.textContent || "{}") || {}; } catch { dados = {}; }
        const atores = dados.atores || [];
        const comunidades = dados.comunidades || [];
        const redes = (dados.redes || []).filter(r => NOMES_REDE[r.tipoRede]);
        if (campos.dataset.editavel === "false") estado.editavel = false;

        const modalLista = document.getElementById("modalRedeLista");
        const modalForm = document.getElementById("modalRedeForm");
        const titulo = document.getElementById("modalRedeListaTitulo");
        const tituloForm = document.getElementById("modalRedeFormTitulo");
        const corpo = document.getElementById("redesTabelaCorpo");
        const tabela = document.getElementById("redesTabela");
        const vazio = document.getElementById("redesVazio");
        const aviso = document.getElementById("redesAvisoEdicao");
        const btnAdicionar = document.getElementById("redesBtnAdicionar");
        const form = document.getElementById("redesForm");
        const selComunidade = document.getElementById("redesComunidade");
        const selAtor = document.getElementById("redesAtor");
        const btnNovoAtor = document.getElementById("redesNovoAtor");
        const dicaNome = document.getElementById("redesNomeDica");
        const radioOutra = document.getElementById("redesTipoOutra");
        const inpOutra = document.getElementById("redesOutraTexto");
        const erro = document.getElementById("redesErro");

        let tipoAtual = "Primaria";
        let ultimaComunidade = "";
        let emEdicao = null; // vínculo sendo editado no formulário (null = adicionando)
        let gatilho = null;

        comunidades.forEach(c => selComunidade.add(new Option(c.nome, c.id)));

        // ---- Campos ocultos enviados com o formulário principal ----
        function campoOculto(nome, valor) {
            const input = document.createElement("input");
            input.type = "hidden";
            input.name = nome;
            input.value = valor == null ? "" : valor;
            campos.appendChild(input);
        }

        function renderCampos() {
            campos.replaceChildren();
            campoOculto("redesEnviadas", "true");
            redes.forEach((r, i) => {
                campoOculto(`VinculosRede[${i}].TipoRede`, r.tipoRede);
                if (r.atorId) campoOculto(`VinculosRede[${i}].AtorId`, r.atorId);
                campoOculto(`VinculosRede[${i}].Nome`, r.nome);
                if (r.comunidadeId) campoOculto(`VinculosRede[${i}].ComunidadeId`, r.comunidadeId);
                campoOculto(`VinculosRede[${i}].TipoRelacao`, r.tipoRelacao);
            });
            document.querySelectorAll("[data-rede-contador]").forEach(el => {
                const total = redes.filter(r => r.tipoRede === el.dataset.redeContador).length;
                el.textContent = total ? String(total) : "";
                el.hidden = total === 0;
            });
        }

        // ---- Lista ----
        function renderLista() {
            titulo.textContent = NOMES_REDE[tipoAtual];
            corpo.replaceChildren();
            const itens = redes.filter(r => r.tipoRede === tipoAtual);
            itens.forEach(r => {
                const tr = document.createElement("tr");
                const tdNome = document.createElement("td");
                tdNome.textContent = r.nome;
                if (!r.atorId) {
                    const novo = document.createElement("span");
                    novo.className = "redes-tag-novo";
                    novo.textContent = "novo";
                    novo.title = "Será cadastrado como ator ao salvar";
                    tdNome.append(" ", novo);
                }
                const tdTipo = document.createElement("td");
                tdTipo.textContent = r.tipoRelacao;
                const tdAcao = document.createElement("td");
                tdAcao.className = "redes-col-acao";
                if (estado.editavel) {
                    const btnEditar = document.createElement("button");
                    btnEditar.type = "button";
                    btnEditar.className = "redes-acao redes-editar";
                    btnEditar.title = "Editar";
                    btnEditar.setAttribute("aria-label", `Editar ${r.nome} na ${NOMES_REDE[tipoAtual]}`);
                    btnEditar.innerHTML = '<i class="fa-solid fa-pen"></i>';
                    btnEditar.addEventListener("click", () => abrirFormulario(r));

                    const btnRemover = document.createElement("button");
                    btnRemover.type = "button";
                    btnRemover.className = "redes-acao redes-remover";
                    btnRemover.title = "Excluir";
                    btnRemover.setAttribute("aria-label", `Remover ${r.nome} da ${NOMES_REDE[tipoAtual]}`);
                    btnRemover.innerHTML = '<i class="fa-solid fa-trash"></i>';
                    btnRemover.addEventListener("click", () => {
                        redes.splice(redes.indexOf(r), 1);
                        renderCampos();
                        renderLista();
                    });
                    tdAcao.append(btnEditar, btnRemover);
                }
                tr.append(tdNome, tdTipo, tdAcao);
                corpo.appendChild(tr);
            });
            tabela.hidden = itens.length === 0;
            vazio.hidden = itens.length > 0;
            btnAdicionar.hidden = !estado.editavel;
            aviso.hidden = estado.editavel;
        }

        // ---- Modais ----
        function abrir(modal, foco) {
            modal.classList.add("active");
            (foco || modal.querySelector("button, select, input"))?.focus();
        }

        function fechar(modal) {
            modal.classList.remove("active");
        }

        function abrirLista(tipo, botao) {
            tipoAtual = tipo;
            gatilho = botao || gatilho;
            renderLista();
            abrir(modalLista);
        }

        function fecharTudo() {
            fechar(modalForm);
            fechar(modalLista);
            gatilho?.focus();
        }

        function voltarParaLista() {
            emEdicao = null;
            fechar(modalForm);
            abrirLista(tipoAtual);
        }

        // ---- Formulário de adição ----
        // Lista os atores da comunidade escolhida que ainda não estão nesta rede.
        function atualizarAtores() {
            const jaNaRede = new Set(redes.filter(r => r.tipoRede === tipoAtual && r.atorId && r !== emEdicao).map(r => r.atorId));
            selAtor.replaceChildren(new Option(selComunidade.value ? "Selecione o ator" : "Selecione primeiro a comunidade", ""));
            selAtor.disabled = !selComunidade.value;
            if (selComunidade.value) {
                atores
                    .filter(a => a.comunidadeId === selComunidade.value && !jaNaRede.has(a.id))
                    .forEach(a => selAtor.add(new Option(a.nome, a.id)));
            }
            atualizarDica();
        }

        function atualizarDica() {
            dicaNome.textContent = !selComunidade.value ? "Selecione primeiro a comunidade."
                : selAtor.options.length <= 1 ? "Nenhum ator disponível nesta comunidade. Use \u201cCadastrar novo ator\u201d."
                : "";
        }

        // ---- Cadastro de novo ator em outra janela ----
        // Abre Atores/Create?retorno=janela; ao salvar, a página AtorCriado devolve o ator por
        // postMessage/BroadcastChannel e ele entra selecionado aqui.
        let janelaCadastro = null;
        function abrirCadastroAtor() {
            const params = new URLSearchParams({ retorno: "janela" });
            if (selComunidade.value) params.set("comunidadeId", selComunidade.value);
            const url = `${window.location.origin}/Atores/Create?${params}`;
            if (janelaCadastro && !janelaCadastro.closed) {
                janelaCadastro.location.href = url;
            } else {
                janelaCadastro = window.open(url, "empoderaCadastroAtor", "width=1100,height=860");
            }
            if (janelaCadastro) {
                janelaCadastro.focus();
                dicaNome.textContent = "Cadastre o ator na janela aberta; ao salvar, ele aparece selecionado aqui.";
            } else {
                // Bloqueador de pop-up: abre na mesma aba não é possível sem perder o que foi preenchido.
                erro.textContent = "O navegador bloqueou a janela de cadastro. Permita pop-ups para este site e tente de novo.";
            }
        }

        function receberAtorCriado(dados) {
            if (!dados || dados.tipo !== "empodera:ator-criado" || !dados.id) return;
            if (!atores.some(a => a.id === dados.id && a.comunidadeId === dados.comunidadeId)) {
                atores.push({ id: dados.id, nome: dados.nome, comunidadeId: dados.comunidadeId });
                atores.sort((a, b) => a.nome.localeCompare(b.nome, "pt-BR"));
            }
            if (!modalForm.classList.contains("active")) return;
            if (dados.comunidadeId && comunidades.some(c => c.id === dados.comunidadeId)) {
                selComunidade.value = dados.comunidadeId;
            }
            atualizarAtores();
            selAtor.value = dados.id;
            atualizarDica();
            dicaNome.textContent = `Ator \u201c${dados.nome}\u201d cadastrado e selecionado.`;
            window.focus();
            selAtor.focus();
        }

        window.addEventListener("message", e => {
            if (e.origin === window.location.origin) receberAtorCriado(e.data);
        });
        try {
            const canal = new BroadcastChannel("empodera-atores");
            canal.addEventListener("message", e => receberAtorCriado(e.data));
        } catch { /* navegador sem BroadcastChannel: postMessage cobre o caso */ }

        // Sem argumento: adiciona. Com um vínculo: abre o formulário preenchido para editá-lo.
        function abrirFormulario(vinculo) {
            emEdicao = vinculo instanceof Object && redes.includes(vinculo) ? vinculo : null;
            form.reset();
            erro.textContent = "";
            tituloForm.textContent = (emEdicao ? "Editar vínculo na " : "Adicionar à ") + NOMES_REDE[tipoAtual];

            // Comunidade: a do vínculo editado; senão a última usada no modal; na primeira vez, a do próprio ator.
            const comunidadeVinculo = emEdicao
                ? (emEdicao.comunidadeId || atores.find(a => a.id === emEdicao.atorId)?.comunidadeId)
                : null;
            const sugerida = comunidadeVinculo || ultimaComunidade || document.getElementById("ComunidadeId")?.value;
            if (sugerida && comunidades.some(c => c.id === sugerida)) {
                selComunidade.value = sugerida;
            }
            atualizarAtores();

            if (emEdicao) {
                if (emEdicao.atorId && [...selAtor.options].some(o => o.value === emEdicao.atorId)) {
                    selAtor.value = emEdicao.atorId;
                }
                const opcao = [...form.querySelectorAll('input[name="redesTipoRelacao"]')]
                    .find(o => o.value === emEdicao.tipoRelacao);
                if (opcao) {
                    opcao.checked = true;
                } else {
                    radioOutra.checked = true;
                    inpOutra.value = emEdicao.tipoRelacao;
                }
            }

            fechar(modalLista);
            abrir(modalForm, selComunidade.value ? selAtor : selComunidade);
        }

        function tipoRelacaoEscolhido() {
            const marcado = form.querySelector('input[name="redesTipoRelacao"]:checked');
            if (!marcado) return "";
            return marcado.value === "__outra" ? inpOutra.value.trim() : marcado.value;
        }

        form.addEventListener("submit", function (e) {
            e.preventDefault();
            const comunidadeId = selComunidade.value;
            const tipoRelacao = tipoRelacaoEscolhido();

            let mensagem = "";
            if (!comunidadeId) mensagem = "Selecione a comunidade.";
            else if (!selAtor.value) mensagem = "Selecione o ator.";
            else if (!tipoRelacao) mensagem = radioOutra.checked ? "Escreva o tipo de relação." : "Selecione o tipo de relação.";

            const existente = atores.find(a => a.id === selAtor.value && a.comunidadeId === comunidadeId);
            const nome = existente ? existente.nome : "";
            if (!mensagem && !existente) mensagem = "Selecione o ator.";
            const repetido = !!existente && redes.some(r => r !== emEdicao && r.tipoRede === tipoAtual && r.atorId === existente.id);
            if (!mensagem && repetido) mensagem = "Este ator já está na " + NOMES_REDE[tipoAtual] + ".";

            if (mensagem) {
                erro.textContent = mensagem;
                return;
            }

            ultimaComunidade = comunidadeId;
            const vinculo = {
                tipoRede: tipoAtual,
                atorId: existente ? existente.id : null,
                nome: existente ? existente.nome : nome,
                comunidadeId: comunidadeId,
                tipoRelacao: tipoRelacao
            };
            if (emEdicao) redes.splice(redes.indexOf(emEdicao), 1, vinculo);
            else redes.push(vinculo);
            emEdicao = null;
            renderCampos();
            voltarParaLista();
        });

        form.addEventListener("input", () => { erro.textContent = ""; });
        form.addEventListener("change", () => { erro.textContent = ""; });
        selComunidade.addEventListener("change", atualizarAtores);
        selAtor.addEventListener("change", atualizarDica);
        btnNovoAtor.addEventListener("click", abrirCadastroAtor);
        inpOutra.addEventListener("input", () => { if (inpOutra.value) radioOutra.checked = true; });
        inpOutra.addEventListener("focus", () => { radioOutra.checked = true; });
        radioOutra.addEventListener("change", () => inpOutra.focus());

        // ---- Eventos gerais ----
        document.querySelectorAll("[data-rede-abrir]").forEach(botao => {
            botao.addEventListener("click", () => abrirLista(botao.dataset.redeAbrir, botao));
        });
        btnAdicionar.addEventListener("click", () => abrirFormulario());
        modalLista.querySelectorAll("[data-redes-fechar]").forEach(b => b.addEventListener("click", fecharTudo));
        modalForm.querySelectorAll("[data-redes-voltar]").forEach(b => b.addEventListener("click", voltarParaLista));
        modalLista.addEventListener("click", e => { if (e.target === modalLista) fecharTudo(); });
        modalForm.addEventListener("click", e => { if (e.target === modalForm) voltarParaLista(); });
        document.addEventListener("keydown", e => {
            if (e.key !== "Escape") return;
            if (modalForm.classList.contains("active")) voltarParaLista();
            else if (modalLista.classList.contains("active")) fecharTudo();
        });

        estado.aplicar = function () {
            if (modalLista.classList.contains("active")) renderLista();
        };

        renderCampos();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
