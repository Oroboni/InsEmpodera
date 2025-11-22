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

    [HttpGet]
    public IActionResult ComunidadesDetalhes(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        Comunidade comunidade;

        if (id > 0)
        {
            // Modo Edição: Busca a comunidade existente
            comunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == id);
            
            // Se não encontrar, retorna um modelo vazio para o modo de criação/ou erro, 
            // dependendo da sua regra de negócio. Para simplificar, trataremos como novo.
            if (comunidade == null)
            {
                comunidade = new Comunidade();
            }
        }
        else
        {
            // Modo Criação: Retorna um novo objeto vazio (IdComunidade = 0)
            comunidade = new Comunidade();
            // Garante que o ID é 0 (padrão para int, mas é bom ser explícito)
            comunidade.IdComunidade = 0; 
        }

        return View("~/Views/Home/Sidebar/ComunidadesDetalhes.cshtml", comunidade);

        // Antigo
        // var comunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == id);

        // return View("~/Views/Home/Sidebar/ComunidadesDetalhes.cshtml", comunidade);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ComunidadesDetalhes(Empodera.Models.Comunidade comunidade)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        // 1. Lógica de CRIAÇÃO (IdComunidade == 0)
        if (comunidade.IdComunidade == 0)
        {
            comunidade.DtCriacao = DateTime.Now;
            comunidade.DtModificacao = DateTime.Now;
            
            _context.Comunidades.Add(comunidade);
            _context.SaveChanges();
            
            return RedirectToAction("Comunidades");
        }
        
        // 2. Lógica de EDIÇÃO (IdComunidade > 0)
        var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == comunidade.IdComunidade);
        if (existingComunidade != null)
        {
            existingComunidade.Nome = comunidade.Nome;
            existingComunidade.Local = comunidade.Local;
            existingComunidade.Status = comunidade.Status;
            existingComunidade.Complemento = comunidade.Complemento;
            existingComunidade.Descricao = comunidade.Descricao;
            existingComunidade.DescricaoAcessibilidade = comunidade.DescricaoAcessibilidade;
            existingComunidade.DtModificacao = DateTime.Now;

            _context.SaveChanges();
        }

        return RedirectToAction("Comunidades");
        // A
            //     var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == comunidade.IdComunidade);
            //     if (existingComunidade != null)
            //     {
            //         existingComunidade.Nome = comunidade.Nome;
            //         existingComunidade.Local = comunidade.Local;
            //         existingComunidade.Status = comunidade.Status;
            //         existingComunidade.Complemento = comunidade.Complemento;
            //         existingComunidade.Descricao = comunidade.Descricao;
            //         existingComunidade.DescricaoAcessibilidade = comunidade.DescricaoAcessibilidade;
            //         existingComunidade.DtModificacao = DateTime.Now;

            //         _context.SaveChanges();
            //     }

            // return RedirectToAction("Comunidades");
    }

    public IActionResult Comunidades()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        var comunidades = _context.Comunidades
            .Select(c => new Empodera.Models.ComunidadeDto
            {
                Id = c.IdComunidade,
                Nome = c.Nome,
                Status = c.Status
            })
            .ToList();

        return View("~/Views/Home/Sidebar/Comunidades.cshtml", comunidades);
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
        return View("~/Views/Users/index.cshtml");
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
