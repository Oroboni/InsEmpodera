document.addEventListener('DOMContentLoaded', function () {

    // Cores da marca vêm do tema ativo (wwwroot/css/tema.css); o canvas do Chart.js não entende var(--...).
    const estiloRaiz = getComputedStyle(document.documentElement);
    const corTema = (token, fallback) => estiloRaiz.getPropertyValue(token).trim() || fallback;
    const corPrimaria = corTema('--color-primary', '#7A3188');
    const corPrimariaRgb = corTema('--color-primary-rgb', '122 49 136');

    // 1. Gráfico de Pizza (Fichas de 1º Contato)
    const pieChartCtx = document.getElementById('firstContactPieChart');
    if (pieChartCtx) {
        new Chart(pieChartCtx, {
            type: 'pie',
            data: {
                labels: ['Em progresso', 'Completa', 'Abandonada'],
                datasets: [{
                    label: 'Fichas',
                    data: [2, 1, 0], // Seus dados fictícios
                    backgroundColor: [
                        '#026AA2', // Azul
                        '#D61F7A', // Rosa
                        '#36A2EB'  // Verde (substitua pela sua cor)
                    ],
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                    }
                }
            }
        });
    }

    // 2. Gráfico Radar (Avaliações Pessoais): uma série por data escolhida, sobrepostas e com cores diferentes.
    const radarChartCtx = document.getElementById('assessmentRadarChart');
    if (radarChartCtx) {
        const series = JSON.parse(radarChartCtx.dataset.series || '[]');
        new Chart(radarChartCtx, {
            type: 'radar',
            data: {
                labels: [
                    'Condições de crimes', 'Substâncias', 'Moradia', 'Prevenção',
                    'Assistência Básica', 'Educação', 'Saúde', 'Ocupação', 'Lazer'
                ],
                datasets: series.map(serie => ({
                    label: serie.label,
                    data: serie.pontos,
                    fill: true,
                    backgroundColor: serie.cor + '26',
                    borderColor: serie.cor,
                    pointBackgroundColor: serie.cor,
                }))
            },
            options: {
                responsive: true,
                scales: {
                    r: {
                        angleLines: { display: true },
                        suggestedMin: 0,
                        suggestedMax: 5,
                        pointLabels: { font: { size: 13, weight: '500' } }
                    }
                },
                plugins: { legend: { position: 'bottom' } }
            }
        });
    }

    // 3. Gráfico Donut (Rede Primária): vínculos por tipo de relação.
    const donutChartCtx = document.getElementById('networkDonutChart');
    if (donutChartCtx) {
        new Chart(donutChartCtx, {
            type: 'doughnut',
            data: {
                labels: JSON.parse(donutChartCtx.dataset.labels || '[]'),
                datasets: [{
                    data: JSON.parse(donutChartCtx.dataset.values || '[]'),
                    backgroundColor: ['#4C51BF', '#D61F7A', '#38A169', '#3182CE', '#805AD5', '#E8A33D', '#C05621', '#4A5568'],
                }]
            },
            options: {
                responsive: true,
                cutout: '70%',
                plugins: { legend: { position: 'bottom' } }
            }
        });
    }

    // 4. Gráfico de Barras Horizontais (Rede Primária): proporção de atores da rede com cada recurso/vulnerabilidade.
    const barChartCtx = document.getElementById('networkBarChart');
    if (barChartCtx) {
        new Chart(barChartCtx, {
            type: 'bar',
            data: {
                labels: JSON.parse(barChartCtx.dataset.labels || '[]'),
                datasets: [
                    {
                        label: 'Recursos',
                        data: JSON.parse(barChartCtx.dataset.resources || '[]'),
                        backgroundColor: '#026AA2',
                        barPercentage: 0.6,
                    },
                    {
                        label: 'Vulnerabilidades',
                        data: JSON.parse(barChartCtx.dataset.vulnerabilities || '[]'),
                        backgroundColor: '#D61F7A',
                        barPercentage: 0.6,
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { min: 0, max: 1, ticks: { stepSize: 0.1 } }
                },
                plugins: { legend: { position: 'bottom' } }
            }
        });
    }

    // ==========================================================
    // 5. LÓGICA DO TOGGLE DA PÁGINA RSC
    // ==========================================================
    const btnNumbers = document.getElementById('btn-numbers');
    const btnPercent = document.getElementById('btn-percent');
    const tableNumbers = document.getElementById('table-numbers');
    const tablePercentages = document.getElementById('table-percentages');

    if (btnNumbers && btnPercent && tableNumbers && tablePercentages) {
        
        btnNumbers.addEventListener('click', function() {
            // Mostrar tabela de números
            tableNumbers.style.display = '';
            tablePercentages.style.display = 'none';
            
            // Atualizar botões
            btnNumbers.classList.add('active');
            btnPercent.classList.remove('active');
        });

        btnPercent.addEventListener('click', function() {
            // Mostrar tabela de porcentagem
            tableNumbers.style.display = 'none';
            tablePercentages.style.display = '';
            
            // Atualizar botões
            btnNumbers.classList.remove('active');
            btnPercent.classList.add('active');
        });
    }

});

