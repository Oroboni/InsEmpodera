// Mapeamento das páginas para as actions do controller
const pageRoutes = {
    home: '/Home/HomePage',
    Comunidades: '/Home/Comunidades',
    Atores: '/Home/Atores',
    FichaPrimeiroContato: '/Home/FichaPrimeiroContato',
    DiariosDeCampo: '/Home/DiariosDeCampo',
    DiarioProcessoPessoal: '/Home/DiarioProcessoPessoal',
    Relatorios: '/Home/Relatorios',
    Dashboard: '/Home/Dashboard',
    Atividades: '/Home/Atividades',
    Usuarios: '/Home/Usuarios',
    PerfisDeAcesso: '/Home/PerfisDeAcesso',
    Ajuda: '/Home/Ajuda',
    Configuracoes: '/Home/Configuracoes',
    Logout: '/Home/Logout'
};

const content = document.getElementById("content");

function loadPage(page, push = true) {
    // Logout é um caso especial - redireciona para a action real
    if (page === 'Logout') {
        window.location.href = '/Home/Logout';
        return;
    }

    if (pageRoutes[page]) {
        // Faz uma requisição AJAX para buscar apenas o conteúdo da view
        fetch(pageRoutes[page], {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.text())
        .then(html => {
            // Extrai apenas o conteúdo do body (sem layout)
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const bodyContent = doc.querySelector('#content') || doc.body;
            content.innerHTML = bodyContent.innerHTML;

            // Atualiza classes ativas
            updateActiveMenuItem(page);

            if (push) {
                // Para home, usamos a URL raiz
                const url = page === 'home' ? '/' : '/' + page.toLowerCase();
                history.pushState({ page }, "", url);
            }
        })
        .catch(error => {
            console.error('Erro ao carregar página:', error);
            content.innerHTML = "<h2>404</h2><p>Página não encontrada.</p>";
        });
    } else {
        content.innerHTML = "<h2>404</h2><p>Página não encontrada.</p>";
    }
}

function updateActiveMenuItem(page) {
    // Remove classe ativa de todos os itens
    document.querySelectorAll(".section-1 li").forEach(item => {
        item.classList.remove("active");
    });
    
    // Adiciona classe ativa ao item atual
    const activeItem = document.querySelector(`[data-page="${page}"]`);
    if (activeItem) {
        activeItem.classList.add("active");
    }
}

// Eventos de clique na sidebar
document.querySelectorAll(".section-1 li").forEach(item => {
    item.addEventListener("click", () => {
        const page = item.getAttribute("data-page");
        loadPage(page);
    });
});

// Lida com botão Voltar/Avançar do navegador
window.addEventListener("popstate", (event) => {
    const page = event.state?.page || "home";
    loadPage(page, false);
});

// Inicialização
document.addEventListener('DOMContentLoaded', function() {
    const currentPath = window.location.pathname.replace("/", "").toLowerCase() || "";

    // Mapear URL para nome da página
    const urlToPageMap = {
        '': 'home',
        'home': 'home',
        'homepage': 'home',
        'comunidades': 'Comunidades',
        'atores': 'Atores',
        'fichaprimeirocontato': 'FichaPrimeiroContato',
        'diariosdeampo': 'DiariosDeCampo',
        'diarioprocessopessoal': 'DiarioProcessoPessoal',
        'relatorios': 'Relatorios',
        'dashboard': 'Dashboard',
        'atividades': 'Atividades',
        'usuarios': 'Usuarios',
        'perfisdeacesso': 'PerfisDeAcesso',
        'ajuda': 'Ajuda',
        'configuracoes': 'Configuracoes'
    };

    const currentPage = urlToPageMap[currentPath] || 'home';
    
    // Se não há estado no history, adiciona o estado atual
    if (!history.state) {
        const url = currentPage === 'home' ? '/' : '/' + currentPath;
        history.replaceState({ page: currentPage }, "", url);
    }
});