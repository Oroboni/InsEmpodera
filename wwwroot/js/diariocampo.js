/* ---------------------- POPUP CONTROLLERS ------------------------ */

function abrirModal(id) {
  document.getElementById(id).classList.remove("hidden");
}

function fecharModal(id) {
  document.getElementById(id).classList.add("hidden");
}

/* ---------------------- ATIVIDADE DA EQUIPE ------------------------ */

let atividadeIndex = 0;

function addAtividade() {
  abrirModal("modalAtividade");
}

function confirmarAtividade() {
  // Agora sempre será chamado depois que a DOM existir
  const container = document.getElementById("atividadesContainer");

  if (!container) {
    console.error("ERRO: atividadesContainer não encontrado na página.");
    return;
  }

  const atorId = document.getElementById("modalAtorAtividade").value;
  const atorNome =
    document.getElementById("modalAtorAtividade").selectedOptions[0].text;
  const qtd = document.getElementById("modalQtdAtividade").value;

  if (!qtd || qtd < 1) {
    alert("Informe a quantidade.");
    return;
  }

  const item = document.createElement("div");
  item.classList.add("atividade-item");
  item.innerHTML = `
        <p><strong>${atorNome}</strong> — ${qtd} pessoa(s)</p>

        <input type="hidden" name="Atividades[${atividadeIndex}].IdAtor" value="${atorId}">
        <input type="hidden" name="Atividades[${atividadeIndex}].Quantidade" value="${qtd}">

        <button type="button" class="remove-x" onclick="this.parentNode.remove()">×</button>
    `;

  container.appendChild(item);

  atividadeIndex++;
  fecharModal("modalAtividade");
}

/* ---------------------- AÇÃO DA EQUIPE ------------------------ */

let acaoIndex = 0;

function addAcao() {
  abrirModal("modalAcao");
}

function confirmarAcao() {
  const container = document.getElementById("acoesContainer");

  if (!container) {
    console.error("ERRO: acoesContainer não encontrado na página.");
    return;
  }

  const nome = document.getElementById("modalNomeAcao").value;
  const atorId = document.getElementById("modalAtorAcao").value;
  const atorNome =
    document.getElementById("modalAtorAcao").selectedOptions[0].text;
  const qtd = document.getElementById("modalQtdAcao").value;
  const apoiador = document.getElementById("modalApoiadorAcao").value;

  if (!nome.trim()) {
    alert("Informe o nome da atividade.");
    return;
  }
  if (!qtd || qtd < 1) {
    alert("Informe a quantidade.");
    return;
  }

  const item = document.createElement("div");
  item.classList.add("acao-item");
  item.innerHTML = `
        <p><strong>${nome}</strong> — ${atorNome} (${qtd}) 
        ${apoiador ? "— Apoio: " + apoiador : ""}</p>

        <input type="hidden" name="Acoes[${acaoIndex}].NomeAtividade" value="${nome}">
        <input type="hidden" name="Acoes[${acaoIndex}].IdAtor" value="${atorId}">
        <input type="hidden" name="Acoes[${acaoIndex}].Quantidade" value="${qtd}">
        <input type="hidden" name="Acoes[${acaoIndex}].ApoiadorExterno" value="${apoiador}">

        <button type="button" class="remove-x" onclick="this.parentNode.remove()">×</button>
    `;

  container.appendChild(item);

  acaoIndex++;
  fecharModal("modalAcao");
}

/* ---------------------- UTIL: SELECT DINÂMICO ------------------------ */
function createSelect(name, list) {
  const select = document.createElement("select");
  select.name = name;

  list.forEach((item) => {
    const opt = document.createElement("option");
    opt.value =
      item.id ??
      item.Id ??
      item.IdAtividade ??
      item.IdEixo ??
      item.IdAcao ??
      item.IdAtor;
    opt.textContent = item.nome ?? item.Nome;
    select.appendChild(opt);
  });

  return select;
}

/* ---------------------- BUSCA CEP ------------------------ */
function buscarCEP() {
  const cep = document.getElementById("cep").value.replace(/\D/g, "");
  if (cep.length !== 8) return;

  fetch(`https://viacep.com.br/ws/${cep}/json/`)
    .then((r) => r.json())
    .then((data) => {
      document.getElementById("rua").value = data.logradouro || "";
      document.getElementById("bairro").value = data.bairro || "";
      document.getElementById("cidade").value = data.localidade || "";
      document.getElementById("estado").value = data.uf || "";

      atualizarMapa(data.logradouro, data.localidade, data.uf);
    });
}

/* ---------------------- MAPA FICTÍCIO ------------------------ */
function atualizarMapa(rua, cidade, estado) {
  const mapaFrame = document.getElementById("mapaFrame");

  if (!mapaFrame) {
    console.error("Mapa não encontrado no DOM.");
    return;
  }

  const endereco = `${rua}, ${cidade} - ${estado}`;
  const enderecoEncoded = encodeURIComponent(endereco);

  mapaFrame.src = `https://www.google.com/maps?q=${enderecoEncoded}&output=embed`;
}

/* ============================================================
   MENÇÕES AVANÇADAS COM AUTOCOMPLETE
   ============================================================ */

const mentionArea = document.getElementById("Descricao");
const mentionBox2 = document.getElementById("mentionBox");

// DADOS FICTÍCIOS (como se viessem do banco)
const DB = {
    comunidade: ["Empodera", "Comunidade Alegria", "Comunidade Sul", "Vila Esperança"],
    atividade: ["Oficina Criativa", "Roda de Conversa", "Atividade Física", "Aula de Teatro"],
    ator: ["Educador João", "Educadora Ana", "Coordenador Paulo", "Apoio Maria"]
};

// Estado atual de menção
let mentionMode = null; 
let mentionStart = 0;

/* ----------------------- CAPTURA DO TEXTO ---------------------- */
mentionArea.addEventListener("keyup", function (e) {
    
    const cursor = mentionArea.selectionStart;
    const valor = mentionArea.value.substring(0, cursor);

    // Detectou o @
    const match = valor.match(/@([a-zA-Z]*)$/);

    if (match) {
        const texto = match[1].toLowerCase();
        mentionStart = cursor - (texto.length + 1);

        // O usuário começou a digitar após o @ → filtra categorias
        if (!mentionMode) {
            filtrarCategoria(texto);
        } else {
            filtrarItens(texto);
        }

        positionMentionBox2();
        mentionBox2.style.display = "block";
    } 
    else {
        mentionBox2.style.display = "none";
        mentionMode = null;
    }
});

/* ------------------ FILTRAR CATEGORIA INICIAL ------------------ */
function filtrarCategoria(texto) {

    const categorias = [
        { key: "atividade", label: "@atividade" },
        { key: "comunidade", label: "@comunidade" },
        { key: "ator", label: "@ator" }
    ];

    const filtradas = categorias.filter(c => c.label.includes("@" + texto));

    mentionBox2.innerHTML = filtradas.map(f => `
        <div class="mention-option" onclick="selecionarCategoria('${f.key}')">${f.label}</div>
    `).join("");
}

function selecionarCategoria(cat) {
    mentionMode = cat;

    // Substitui o @ por @categoria
    inserirTexto(`@${cat} `);
    
    mentionBox2.style.display = "none";
}

/* ------------------- FILTRAR ITENS DA CATEGORIA ------------------- */
function filtrarItens(texto) {
    const lista = DB[mentionMode] || [];
    const filt = lista.filter(item => item.toLowerCase().includes(texto.toLowerCase()));

    mentionBox2.innerHTML = filt.map(i => `
        <div class="mention-option" onclick="selecionarItem('${i}')">${i}</div>
    `).join("");
}

function selecionarItem(nome) {
    inserirTexto(nome + " ");
    mentionMode = null;
    mentionBox2.style.display = "none";

    // aqui você poderá abrir detalhes futuramente:
    // window.open(`/Comunidade/Detalhes/${nome}`, "_blank");
}

/* ----------------------- INSERIR TEXTO -------------------------- */
function inserirTexto(texto) {
    const start = mentionArea.selectionStart;
    const end = mentionArea.selectionEnd;
    const valor = mentionArea.value;

    mentionArea.value = valor.substring(0, start) + texto + valor.substring(end);
    mentionArea.selectionStart = mentionArea.selectionEnd = start + texto.length;
}

/* ----------------------- POSIÇÃO DO BOX -------------------------- */
function positionMentionBox2() {
    const rect = mentionArea.getBoundingClientRect();

    mentionBox2.style.position = "absolute";
    mentionBox2.style.left = rect.left + "px";
    mentionBox2.style.top = rect.bottom + "px";
    mentionBox2.style.width = rect.width + "px";
    mentionBox2.style.zIndex = 999;
}

/* Excluir Diario de campo pop-up */

   let dadosDiario = {};

function carregarPopup(dados) {
    dadosDiario = dados;

    // preenche o popup
    document.getElementById("pComunidade").innerText = dados.comunidade;
    document.getElementById("pEixos").innerText = dados.eixos;
    document.getElementById("pAcoesInst").innerText = dados.inst;
    document.getElementById("pAcoesEquipe").innerText = dados.equipe;
    document.getElementById("pLocalizacao").innerText = dados.local;
    document.getElementById("pAnexos").innerText = dados.anexos;

    // abre automaticamente ao carregar a página
    document.getElementById("popupExcluir").style.display = "flex";
}

function fecharPopup() {
    document.getElementById("popupExcluir").style.display = "none";
}

function confirmarExclusao() {
    alert("Diário excluído (simulação sem banco)");
    window.location.href = "/DiarioCampo"; // volta ao Index
}