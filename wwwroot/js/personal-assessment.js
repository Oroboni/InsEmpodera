// Espera o DOM carregar completamente
document.addEventListener("DOMContentLoaded", function() {

    // --- 1. Lógica dos Sliders de Métrica ---
    const sliders = document.querySelectorAll('.metric-slider');

    // Função para atualizar a cor e o data-level de um slider
    function updateSlider(slider) {
        const value = slider.value;
        // Adiciona um atributo 'data-level' ao slider
        // O CSS [data-level="1"] vai cuidar da cor
        slider.dataset.level = value;

        // Atualiza o fundo (preenchimento)
        const min = slider.min || 1;
        const max = slider.max || 5;
        const percentage = ((value - min) / (max - min)) * 100;
        
        // Aplica o preenchimento da cor
        // (O CSS cuida da cor em si, o JS cuida da porcentagem)
        slider.style.backgroundSize = percentage + '% 100%';
    }

    // Aplica a lógica a todos os sliders da página
    sliders.forEach(slider => {
        // Atualiza a cor no carregamento da página (para 'Edit')
        updateSlider(slider);
        
        // Adiciona um "ouvinte" para atualizar a cor sempre que o valor mudar
        slider.addEventListener('input', function() {
            updateSlider(this);
        });
    });

    // --- 2. Lógica do Rodapé Colapsável (Metadata) ---
    const footerSummary = document.querySelector('.form-footer summary');
    if (footerSummary) {
        // Evita que o clique no sumário submeta o formulário (se estiver dentro de um)
        footerSummary.addEventListener('click', function(e) {
            e.preventDefault();
            const details = this.closest('details');
            
            // Alterna o atributo 'open'
            if (details.hasAttribute('open')) {
                details.removeAttribute('open');
            } else {
                details.setAttribute('open', '');
            }
        });
    }

});