using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Empodera.Services;
using System.Linq;

namespace InsEmpodera.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly RelatorioExcelService _relatorioService;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, RelatorioExcelService relatorioService)
    {
        _logger = logger;
        _context = context;
        _relatorioService = relatorioService;
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

    [HttpGet]
    public IActionResult ComunidadesCriar()
    {
        var comunidade = new Empodera.Models.Comunidade
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };
        return View("~/Views/Home/Sidebar/ComunidadesCriar.cshtml", comunidade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ComunidadesCriar(Empodera.Models.Comunidade comunidade)
    {
        if (ModelState.IsValid)
        {
            comunidade.DtCriacao = DateTime.Now;
            comunidade.DtModificacao = DateTime.Now;

            _context.Comunidades.Add(comunidade);
            _context.SaveChanges();

            return RedirectToAction("Comunidades");
        }

        return View("~/Views/Home/Sidebar/ComunidadesCriar.cshtml", comunidade);
    }


    [HttpGet]
    public IActionResult ExportarExcel()
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var atores = _context.Atores.ToList();
        var atoresComunidade = _context.AtorComunidades.ToList();
        var comunidades = _context.Comunidades.ToList();

        var relatorios = atores.Select(a =>
        {
            var nomesComunidades = atoresComunidade
                .Where(ac => ac.AtorId == a.IdAtores)
                .Join(comunidades,
                    ac => ac.ComunidadeId,
                    c => c.IdComunidade,
                    (ac, c) => c.Nome)
                .ToList();

            return new AtorRelatorioDto
            {
                IdAtor = a.IdAtores,
                Nome = a.Nome,
                Genero = a.Genero,
                Idade = a.Idade,
                PapelSocial1 = a.PapelSocial1,
                PapelSocial2 = a.PapelSocial2,
                Telefone = a.Telefone.ToString(),
                Extra = a.Extra,
                DtCriacao = a.DtCriacao,
                DtModificacao = a.DtModificacao,
                Comunidades = nomesComunidades.Any() ? string.Join(", ", nomesComunidades) : "-"
            };
        }).ToList();

        var arquivo = _relatorioService.GerarRelatorioAtores(relatorios);

        return File(arquivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Relatorio_Atores.xlsx");
    }

    [HttpGet]
    public IActionResult ComunidadesDetalhes(int id)
    {        
        var comunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == id);
        
        if (comunidade == null)
        {
            Console.WriteLine($"Comunidade com ID {id} não encontrada no GET");
            return RedirectToAction("Comunidades");
        }
        
        Console.WriteLine($"Comunidade encontrada: {comunidade.Nome}");
        
        return View("~/Views/Home/Sidebar/ComunidadesDetalhes.cshtml", comunidade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ComunidadesDetalhes(Empodera.Models.Comunidade comunidade)
    {    
        var existingComunidade = _context.Comunidades
            .FirstOrDefault(c => c.IdComunidade == comunidade.IdComunidade);
        
        if (existingComunidade == null)
        {
            return RedirectToAction("Comunidades");
        }
        
        existingComunidade.Nome = comunidade.Nome;
        existingComunidade.Local = comunidade.Local;
        existingComunidade.Status = comunidade.Status;
        existingComunidade.Complemento = comunidade.Complemento;
        existingComunidade.Descricao = comunidade.Descricao;
        existingComunidade.DescricaoAcessibilidade = comunidade.DescricaoAcessibilidade;
        existingComunidade.DtModificacao = DateTime.Now;

        try
        {
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Comunidades");
        }
        
        return RedirectToAction("ComunidadesDetalhes", new { id = comunidade.IdComunidade });
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

    public IActionResult Privacy()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
