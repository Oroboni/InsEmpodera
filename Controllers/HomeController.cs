using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.AspNetCore.Authorization;

namespace Empodera.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View();
    }

    public IActionResult HomePage()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/HomePage.cshtml");
    }

    public IActionResult Atores()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Atores.cshtml");
    }

    public IActionResult FichaPrimeiroContato()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/FichaPrimeiroContato.cshtml");
    }

    public IActionResult DiariosDeCampo()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/DiariosDeCampo.cshtml");
    }

    public IActionResult DiarioProcessoPessoal()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/DiarioProcessoPessoal.cshtml");
    }

    public IActionResult Relatorios()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Relatorios.cshtml");
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Dashboard.cshtml");
    }

    public IActionResult Atividades()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Atividades.cshtml");
    }

    public IActionResult Usuarios()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Usuarios.cshtml");
    }

    public IActionResult PerfisDeAcesso()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/PerfisDeAcesso.cshtml");
    }

    public IActionResult Ajuda()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Ajuda.cshtml");
    }

    public IActionResult Configuracoes()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Configuracoes.cshtml");
    }

    public IActionResult Logout()
    {
        // espaçozinho para talvez futura lógica de logout
        return RedirectToAction("Home");
    }
}
