document.addEventListener('DOMContentLoaded', function () {

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

    // 2. Gráfico Radar (Avaliações Pessoais)
    const radarChartCtx = document.getElementById('assessmentRadarChart');
    if (radarChartCtx) {
        new Chart(radarChartCtx, {
            type: 'radar',
            data: {
                labels: [
                    'Rede primária', 'Seguridade Social', 'Substâncias', 'Moradia',
                    'Prevenção', 'Assistência Básica', 'Educação', 'Saúde', 'Ocupação', 'Lazer'
                ],
                datasets: [{
                    label: '02/09/25',
                    data: [3, 4, 2, 3, 5, 4, 3, 2, 4, 5], // Dados fictícios (1-5)
                    fill: true,
                    backgroundColor: 'rgba(122, 49, 136, 0.2)',
                    borderColor: 'rgb(122, 49, 136)',
                    pointBackgroundColor: 'rgb(122, 49, 136)',
                }]
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

    // ==========================================================
    // [CÓDIGO CORRIGIDO/ADICIONADO]
    // 3. Gráfico Donut (Rede Primária)
    // ==========================================================
    const donutChartCtx = document.getElementById('networkDonutChart');
    if (donutChartCtx) {
        new Chart(donutChartCtx, {
            type: 'doughnut',
            data: {
                labels: ['Conhecido', 'Amigo', 'Familiar', 'Vizinho', 'Colega de trabalho'],
                datasets: [{
                    data: [1, 0, 0, 0, 0], // Dados fictícios (1 Conhecido)
                    backgroundColor: ['#4C51BF', '#D61F7A', '#38A169', '#3182CE', '#805AD5'],
                }]
            },
            options: {
                responsive: true,
                cutout: '70%', // O "buraco" do donut
                plugins: {
                    legend: {
                        position: 'bottom',
                    }
                }
            }
        });
    }

    // ==========================================================
    // [CÓDIGO CORRIGIDO/ADICIONADO]
    // 4. Gráfico de Barras Horizontais (Rede Primária)
    // ==========================================================
    const barChartCtx = document.getElementById('networkBarChart');
    if (barChartCtx) {
        new Chart(barChartCtx, {
            type: 'bar',
            data: {
                labels: [
                    'Número de nós', 'Rede primária', 'Seguridade Social', 'Substâncias',
                    'Moradia', 'Prevenção', 'Assistência Básica', 'Educação', 'Saúde', 'Ocupação', 'Lazer'
                ],
                datasets: [
                    {
                        label: 'Recursos',
                        data: [0.9, 0.9, 0.9, 0.1, 0, 0, 0, 0, 0, 0, 0], // Dados fictícios (Azul)
                        backgroundColor: '#026AA2', 
                        barPercentage: 0.6,
                    },
                    {
                        label: 'Vulnerabilidades',
                        data: [0.1, 0.1, 0.1, 0, 0.9, 0, 0, 0, 0, 0, 0], // Dados fictícios (Rosa)
                        backgroundColor: '#D61F7A', 
                        barPercentage: 0.6,
                    }
                ]
            },
            options: {
                indexAxis: 'y', // <-- Isso torna o gráfico horizontal
                responsive: true,
                scales: {
                    x: {
                        stacked: true, // Empilha as barras
                        min: 0,
                        max: 1.0,
                        ticks: {
                             stepSize: 0.1
                        }
                    },
                    y: {
                        stacked: true // Empilha as barras
                    }
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                    }
                }
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