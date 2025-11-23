const pageRoutes = {
    'home': '/Home/Index',
    'comunidades': '/Comunidades/Index',
    'atores': '/Atores/Index',
    'fichaprimeirocontato': '/FichaPrimeiroContato/Index',
    'diariosdecampo': '/DiariosDeCampo/Index',
    'diarioprocessopessoal': '/DiarioProcessoPessoal/Index',
    'relatorios': '/Relatorios/Index',
    'dashboard': '/Dashboard/Index',
    'atividades': '/Atividades/Index',
    'usuarios': '/Usuarios/Index',
    'perfisdeacesso': '/PerfisDeAcesso/Index',
    'ajuda': '/Ajuda/Index',
    'configuracoes': '/Configuracoes/Index',
    'logout': '/Account/Logout'
};

const content = document.getElementById("content");


function loadPage(pageKey, push = true) {
    const page = pageKey.toLowerCase();

    if (page === 'logout') {
        window.location.href = pageRoutes[page];
        return;
    }

    if (pageRoutes[page]) {
        fetch(pageRoutes[page], {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(res => {
            if (!res.ok) throw new Error('Erro na resposta do servidor');
            return res.text();
        })
        .then(html => processAndRender(html, page, push))
        .catch(err => console.error('Erro ao carregar:', err));
    } else {
        console.warn("Rota não mapeada para AJAX:", page);
    }
}

function processAndRender(html, pageKey, push) {
    document.querySelectorAll('link[data-page-specific]').forEach(link => link.remove());

    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = html;
    
    const overrideCss = tempDiv.querySelector('[data-page-css]');
    const cssFileName = overrideCss ? overrideCss.getAttribute('data-page-css') : pageKey;

    const render = () => {
        content.innerHTML = html;
        executePageScripts(html);
        updateActiveMenuItem(pageKey);
        if (push) updateHistory(pageKey);
    };

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = `/css/pages/${cssFileName}.css`;
    link.setAttribute('data-page-specific', 'true');

    link.onload = render;
    link.onerror = () => {
        render();
    };

    document.head.appendChild(link);
}

function executePageScripts(html) {
    const scripts = html.match(/<script>([\s\S]*?)<\/script>/g);
    if (scripts) {
        scripts.forEach(scriptTag => {
            const content = scriptTag.replace(/<script>|<\/script>/g, '');
            const script = document.createElement('script');
            script.textContent = content;
            document.body.appendChild(script);
        });
    }
}

function updateHistory(page) {
    const url = page === 'home' ? '/' : '/' + page;
    history.pushState({ page }, "", url);
}

function updateActiveMenuItem(pageKey) {
    const page = pageKey.toLowerCase();
    document.querySelectorAll(".section-1 li").forEach(item => {
        item.classList.remove("active");
        // Compara ignorando maiúsculas/minúsculas
        const itemPage = item.getAttribute("data-page")?.toLowerCase();
        if (itemPage === page) item.classList.add("active");
    });
}

document.querySelectorAll(".section-1 li").forEach(item => {
    item.addEventListener("click", (e) => {
        e.preventDefault();
        const page = item.getAttribute("data-page");
        if(page) loadPage(page);
    });
});

window.addEventListener("popstate", (event) => {
    const page = event.state?.page || "home";
    loadPage(page, false);
});

document.addEventListener('DOMContentLoaded', function () {
    let currentPath = window.location.pathname.replace(/^\/|\/$/g, '').toLowerCase();
    if (currentPath === '' || currentPath === 'homepage') currentPath = 'home';

    let isAjaxPage = false;
    let mappedKey = null;

    if (pageRoutes[currentPath]) {
        isAjaxPage = true;
        mappedKey = currentPath;
    } 
    else if (currentPath === 'home') {
        isAjaxPage = true;
        mappedKey = 'home';
    }

    if (isAjaxPage) {
        if (!history.state) {
            const url = mappedKey === 'home' ? '/' : '/' + mappedKey;
            history.replaceState({ page: mappedKey }, "", url);
        }
        updateActiveMenuItem(mappedKey);
    } else {
        // É uma página externa ao fluxo AJAX (ex: Create, Edit, Login).
        // não faz nada. O navegador mostra o HTML que o servidor enviou.
        console.log("Página standalone detectada (Create/Edit). JS de navegação pausado.");
    }
});