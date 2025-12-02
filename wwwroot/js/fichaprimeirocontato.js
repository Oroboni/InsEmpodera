document.addEventListener('DOMContentLoaded', function () {
    
    // ==========================================
    // 1. MÁSCARA DE TELEFONE
    // ==========================================
    const campoTelefone = document.getElementById('inputTelefone');

    const aplicarMascaraTelefone = (valor) => {
        if (!valor) return "";
        
        // Remove tudo o que não é número
        valor = valor.replace(/\D/g, "");
        // Limita a 11 números
        valor = valor.substring(0, 11);
        
        // Aplica a formatação
        valor = valor.replace(/^(\d{2})(\d)/g, "($1) $2");
        valor = valor.replace(/(\d)(\d{4})$/, "$1-$2");
        
        return valor;
    };

    if (campoTelefone) {
        campoTelefone.addEventListener('input', (e) => {
            e.target.value = aplicarMascaraTelefone(e.target.value);
        });
        // Formata valor inicial se houver
        if (campoTelefone.value) {
            campoTelefone.value = aplicarMascaraTelefone(campoTelefone.value);
        }
    }

    // ==========================================
    // 2. INICIALIZAÇÃO DO MAPA
    // ==========================================
    if (typeof initMapSelector === 'function') {
        initMapSelector('mapa-principal', 'input-endereco');
    }

    // Inicializa o estado dos botões do Wizard
    updateButtons();
});

// ==========================================
// 3. LÓGICA DO WIZARD (PASSO A PASSO)
// ==========================================
// Estas variáveis e funções ficam fora do EventListener para serem
// acessíveis pelo "onclick" do HTML
let currentStep = 1;
const totalSteps = 3;

function changeStep(direction) {
    const nextStep = currentStep + direction;
    if (nextStep < 1 || nextStep > totalSteps) return;

    // Remove classe ativa do passo atual
    const stepAtual = document.getElementById(`step-${currentStep}`);
    if(stepAtual) stepAtual.classList.remove('active');

    // Atualiza índice
    currentStep = nextStep;

    // Adiciona classe ativa no novo passo
    const stepNovo = document.getElementById(`step-${currentStep}`);
    if(stepNovo) stepNovo.classList.add('active');

    // Atualiza Header (Bolinhas)
    for (let i = 1; i <= totalSteps; i++) {
        const stepEl = document.getElementById(`header-step-${i}`);
        if(stepEl) {
            if (i <= currentStep) {
                stepEl.classList.add('active');
            } else {
                stepEl.classList.remove('active');
            }
        }
    }
    
    // Atualiza Linhas
    for (let i = 1; i < totalSteps; i++) {
        const lineEl = document.getElementById(`line-${i}`);
        if(lineEl) {
            if (currentStep > i) {
                lineEl.classList.add('active');
            } else {
                lineEl.classList.remove('active');
            }
        }
    }

    updateButtons();
    
    // Rola suavemente para o topo do form ao mudar de passo
    const stepperContainer = document.querySelector('.stepper-container');
    if(stepperContainer) stepperContainer.scrollIntoView({ behavior: 'smooth' });
}

function updateButtons() {
    const btnPrev = document.getElementById('btn-prev');
    const btnNext = document.getElementById('btn-next');
    const btnSave = document.getElementById('btn-save');

    // Verificação de segurança caso o JS carregue antes do HTML (raro, mas possível)
    if (!btnPrev || !btnNext || !btnSave) return;

    if (currentStep === 1) {
        btnPrev.style.display = 'none';
    } else {
        btnPrev.style.display = 'inline-flex'; 
    }

    if (currentStep === totalSteps) {
        btnNext.style.display = 'none';
        btnSave.style.display = 'inline-flex';
    } else {
        btnNext.style.display = 'inline-flex';
        btnSave.style.display = 'none';
    }
}