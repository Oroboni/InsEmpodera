using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Empodera.Controllers;

public class ComunidadeController : Controller
{
    private readonly ILogger<ComunidadeController> _logger;
    private readonly ApplicationDbContext _context;

    public ComunidadeController(ILogger<ComunidadeController> logger, ApplicationDbContext context)
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
        var comunidades = _context.Comunidades
            .Select(c => new Empodera.Models.ComunidadeDto
            {
                Id = c.IdComunidade,
                Nome = c.Nome,
                Status = c.Status
            })
            .ToList();

        return View(comunidades);
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

        ViewBag.qAtores = _context.AtorComunidades
            .Where(a => a.IdAtorComunidade == id)
            .ToList();

        return View(comunidade);
    }

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
            
            return RedirectToAction("Index");
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

        return RedirectToAction("Index");
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var comunidade = await _context.Comunidades.FindAsync(id);
        if (comunidade == null)
            return RedirectToAction("Index", "Comunidade");

        var relacoes = await _context.AtorComunidades
            .Where(ac => ac.IdAtorComunidade == id)
            .ToListAsync();
        var atoresIds = relacoes.Select(r => r.IdAtorComunidade).ToList();

        _context.AtorComunidades.RemoveRange(relacoes);
        await _context.SaveChangesAsync();

        var atores = await _context.Atores
            .Where(a => atoresIds.Contains(a.IdAtores))
            .ToListAsync();

        _context.Atores.RemoveRange(atores);
        await _context.SaveChangesAsync();

        _context.Comunidades.Remove(comunidade);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Comunidade");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}