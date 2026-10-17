using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var desde = DateTime.Today.AddDays(-30);
        var painel = new PainelInicial
        {
            Comunidades = await _context.Comunidades.CountAsync(c => c.Ativo != "N"),
            ComunidadesEmProcesso = await _context.Comunidades.CountAsync(c => c.Ativo != "N" && c.Status == "Em processo"),
            Atores = await _context.Atores.CountAsync(a => a.Ativo != "N"),
            AtoresUltimos30Dias = await _context.Atores.CountAsync(a => a.Ativo != "N" && a.DtCriacao >= desde),
            Fichas = await _context.FichasPrimeiroContato.CountAsync(),
            FichasEmProgresso = await _context.FichasPrimeiroContato.CountAsync(f => f.Status == "EmProgresso"),
            DiariosCampo = await _context.DiariosCampo.CountAsync(),
            DiariosUltimos30Dias = await _context.DiariosCampo.CountAsync(d => d.Data >= desde),
            UsuariosAtivos = await _context.Usuarios.CountAsync(u => u.Ativo == "S"),
            AtoresRecentes = await _context.Atores.AsNoTracking()
                .Where(a => a.Ativo != "N")
                .OrderByDescending(a => a.DtCriacao)
                .Take(5)
                .Select(a => new AtorRecente(a.IdAtores, a.Nome,
                    a.Comunidades.Select(c => c.Comunidade.Nome).FirstOrDefault(), a.DtCriacao))
                .ToListAsync()
        };
        return View(painel);
    }
    public IActionResult Logout()
    {
        // espaçozinho para talvez futura lógica de logout
        return RedirectToAction("Home");
    }
}
