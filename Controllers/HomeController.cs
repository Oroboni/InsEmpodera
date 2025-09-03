using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;

namespace InsEmpodera.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    //pessoal, o plano é q a página de login fique aqui
    //aí quando iniciar o site ele ja inicia no login

    public IActionResult Index()
    {
        // Aqui você mostra a tela de login
        return View("~/Views/Home/Login.cshtml");
    }
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == "user" && password == "123")
        {
            return RedirectToAction("HomePage");
        }

        ViewBag.Error = "Usuário ou senha inválidos";
        return View("Login");
    }


    public IActionResult HomePage()
    {
        return View("~/Views/Home/Sidebar/HomePage.cshtml");
    }
    
     public IActionResult Comunidades()
    {
        return View("~/Views/Home/Sidebar/Comunidades.cshtml");
    }

    public IActionResult Atores()
    {
        return View("~/Views/Home/Sidebar/Atores.cshtml");
    }

    public IActionResult FichaPrimeiroContato()
    {
        return View("~/Views/Home/Sidebar/FichaPrimeiroContato.cshtml");
    }

    public IActionResult DiariosDeCampo()
    {
        return View("~/Views/Home/Sidebar/DiariosDeCampo.cshtml");
    }

    public IActionResult DiarioProcessoPessoal()
    {
        return View("~/Views/Home/Sidebar/DiarioProcessoPessoal.cshtml");
    }

    public IActionResult Relatorios()
    {
        return View("~/Views/Home/Sidebar/Relatorios.cshtml");
    }

    public IActionResult Dashboard()
    {
        return View("~/Views/Home/Sidebar/Dashboard.cshtml");
    }

    public IActionResult Atividades()
    {
        return View("~/Views/Home/Sidebar/Atividades.cshtml");
    }

    public IActionResult Usuarios()
    {
        return View("~/Views/Home/Sidebar/Usuarios.cshtml");
    }

    public IActionResult PerfisDeAcesso()
    {
        return View("~/Views/Home/Sidebar/PerfisDeAcesso.cshtml");
    }

    public IActionResult Ajuda()
    {
        return View("~/Views/Home/Sidebar/Ajuda.cshtml");
    }

    public IActionResult Configuracoes()
    {
        return View("~/Views/Home/Sidebar/Configuracoes.cshtml");
    }

    public IActionResult Logout()
    {
        // espaçozinho para talvez futura lógica de logout
        return RedirectToAction("Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
