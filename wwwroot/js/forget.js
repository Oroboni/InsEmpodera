// === TRANSIÇÕES E ANIMAÇÕES DA PÁGINA DE RECUPERAÇÃO DE SENHA ===

document.addEventListener('DOMContentLoaded', function() {
    initializeRecoveryPage();
});

// Inicializa a página com animações
function initializeRecoveryPage() {
    // Adiciona listeners para os inputs
    const emailInput = document.getElementById('emailInput');
    if (emailInput) {
        emailInput.addEventListener('focus', handleInputFocus);
        emailInput.addEventListener('blur', handleInputBlur);
    }

    // Adiciona listener para o formulário
    const recoveryForm = document.querySelector('.recovery-form');
    if (recoveryForm) {
        recoveryForm.addEventListener('submit', handleRecoverySubmit);
    }

    // Animação de entrada dos elementos
    animatePageElements();
}

// Anima os elementos da página na entrada
function animatePageElements() {
    const elements = [
        '.recovery-form h2',
        '.recovery-description',
        '.form-input',
        '.recovery-button',
        '.cancel-button'
    ];

    elements.forEach((selector, index) => {
        const element = document.querySelector(selector);
        if (element) {
            element.style.animationDelay = `${0.6 + (index * 0.1)}s`;
        }
    });
}

// Manipula o foco nos inputs
function handleInputFocus(event) {
    const input = event.target;
    input.parentElement.classList.add('focused');
    
    // Animação suave no label se existir
    const label = input.parentElement.querySelector('label');
    if (label) {
        label.style.transform = 'translateY(-25px) scale(0.8)';
        label.style.color = 'var(--cor-destaque)';
    }
}

// Manipula a perda de foco nos inputs
function handleInputBlur(event) {
    const input = event.target;
    
    if (!input.value) {
        input.parentElement.classList.remove('focused');
        
        const label = input.parentElement.querySelector('label');
        if (label) {
            label.style.transform = '';
            label.style.color = '';
        }
    }
}

// Manipula o envio do formulário de recuperação
function handleRecoverySubmit(event) {
    event.preventDefault();
    
    const email = document.getElementById('emailInput').value;
    const button = document.getElementById('recoveryBtn');
    const successMessage = document.getElementById('successMessage');
    
    // Validação básica
    if (!validateEmail(email)) {
        showError('Por favor, digite um e-mail válido.');
        return;
    }

    // Inicia animação de carregamento
    startLoadingAnimation(button);
    
    // Simula o envio do email (substitua pela chamada real da API)
    simulateEmailSending(email, button, successMessage);
}

// Valida formato do e-mail
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Mostra mensagem de erro
function showError(message) {
    // Remove mensagens de erro existentes
    const existingError = document.querySelector('.error-message');
    if (existingError) {
        existingError.remove();
    }

    // Cria nova mensagem de erro
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.innerHTML = `
        <div class="error-icon">⚠</div>
        <div class="error-text">${message}</div>
    `;

    // Insere antes do primeiro form-group
    const firstFormGroup = document.querySelector('.form-group');
    firstFormGroup.parentNode.insertBefore(errorDiv, firstFormGroup);

    // Adiciona estilos da mensagem de erro
    errorDiv.style.cssText = `
        display: block;
        background-color: #FED7D7;
        border: 1px solid #E53E3E;
        border-radius: 15px;
        padding: 15px;
        margin-bottom: 1.5rem;
        text-align: center;
        animation: errorSlideIn 0.4s ease;
    `;

    // Remove após 4 segundos
    setTimeout(() => {
        if (errorDiv.parentNode) {
            errorDiv.style.animation = 'errorSlideOut 0.4s ease forwards';
            setTimeout(() => errorDiv.remove(), 400);
        }
    }, 4000);
}

// Inicia animação de carregamento no botão
function startLoadingAnimation(button) {
    button.textContent = 'ENVIANDO...';
    button.disabled = true;
    button.style.cursor = 'not-allowed';
    
    // Adiciona animação de loading
    button.classList.add('loading');
    
    // CSS da animação será adicionado dinamicamente
    if (!document.querySelector('#loading-styles')) {
        const style = document.createElement('style');
        style.id = 'loading-styles';
        style.textContent = `
            .recovery-button.loading {
                position: relative;
                color: transparent !important;
            }
            .recovery-button.loading::after {
                content: '';
                position: absolute;
                top: 50%;
                left: 50%;
                width: 20px;
                height: 20px;
                margin-top: -10px;
                margin-left: -10px;
                border: 2px solid transparent;
                border-top-color: white;
                border-radius: 50%;
                animation: spin 1s linear infinite;
            }
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
        `;
        document.head.appendChild(style);
    }
}

// Simula o envio do e-mail
function simulateEmailSending(email, button, successMessage) {
    // Simula tempo de processamento
    setTimeout(() => {
        // Remove animação de loading
        button.classList.remove('loading');
        
        // Mostra sucesso
        showSuccessState(button, successMessage);
        
        // Reset após 3 segundos
        setTimeout(() => {
            resetFormState(button, successMessage);
        }, 3000);
        
    }, 2000); // 2 segundos de simulação
}

// Mostra estado de sucesso
function showSuccessState(button, successMessage) {
    // Atualiza botão
    button.textContent = 'ENVIADO ✓';
    button.style.backgroundColor = 'var(--cor-sucesso)';
    button.style.transform = 'scale(1.02)';
    
    // Mostra mensagem de sucesso
    successMessage.classList.add('show');
    
    // Adiciona vibração sutil se suportado
    if (navigator.vibrate) {
        navigator.vibrate([100, 50, 100]);
    }
}

// Reseta o estado do formulário
function resetFormState(button, successMessage) {
    // Reset do botão
    button.textContent = 'ENVIAR LINK';
    button.disabled = false;
    button.style.cursor = 'pointer';
    button.style.backgroundColor = '';
    button.style.transform = '';
    
    // Limpa o input
    document.getElementById('emailInput').value = '';
    
    // Remove mensagem de sucesso
    successMessage.classList.remove('show');
    
    // Remove possíveis mensagens de erro
    const errorMessage = document.querySelector('.error-message');
    if (errorMessage) {
        errorMessage.remove();
    }
}

// Volta para a página de login com transição
function goBackToLogin() {
    // Adiciona classe de transição de saída
    document.body.classList.add('page-exit');
    
    // Aguarda animação e redireciona
    setTimeout(() => {
        window.location.href = 'Index';
    }, 500);
}

// Função alternativa para navegação (ASP.NET)
function goBackToLoginMVC() {
    document.body.classList.add('page-exit');
    
    setTimeout(() => {
        // Use esta função se estiver usando ASP.NET MVC
        window.location.href = '@Url.Action("Index", "Account")';
    }, 500);
}

// Adiciona estilos CSS dinamicamente para animações de erro
function addErrorAnimationStyles() {
    if (!document.querySelector('#error-styles')) {
        const style = document.createElement('style');
        style.id = 'error-styles';
        style.textContent = `
            @keyframes errorSlideIn {
                from {
                    opacity: 0;
                    transform: translateY(-20px) scale(0.9);
                }
                to {
                    opacity: 1;
                    transform: translateY(0) scale(1);
                }
            }
            
            @keyframes errorSlideOut {
                to {
                    opacity: 0;
                    transform: translateY(-20px) scale(0.9);
                }
            }
            
            .error-icon {
                color: #E53E3E;
                font-size: 1.5rem;
                margin-bottom: 0.5rem;
            }
            
            .error-text {
                color: #E53E3E;
                font-weight: 600;
                font-size: 0.9rem;
            }
        `;
        document.head.appendChild(style);
    }
}

// Inicializa estilos de erro
addErrorAnimationStyles();

// Adiciona listener para tecla Enter no campo de e-mail
document.addEventListener('keypress', function(event) {
    if (event.key === 'Enter' && event.target.id === 'emailInput') {
        event.preventDefault();
        handleRecoverySubmit(event);
    }
});

// Função para lidar com erros de rede (para uso futuro com API real)
function handleNetworkError(error) {
    console.error('Erro de rede:', error);
    showError('Erro de conexão. Tente novamente.');
    
    const button = document.getElementById('recoveryBtn');
    if (button) {
        button.textContent = 'ENVIAR LINK';
        button.disabled = false;
        button.style.cursor = 'pointer';
        button.classList.remove('loading');
    }
}

// Exporta funções para uso global (se necessário)
window.goBackToLogin = goBackToLogin;
window.goBackToLoginMVC = goBackToLoginMVC;