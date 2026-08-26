using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Empodera.Controllers;

public class AtoresController : Controller
{
    private readonly ILogger<AtoresController> _logger;
    private readonly ApplicationDbContext _context;

    public AtoresController(ILogger<AtoresController> logger, ApplicationDbContext context)
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

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanList("Atores"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        ViewBag.qgenero = _context.Atores.Select(a => a.Genero).Distinct().ToList();

        var Atores = _context.Atores.Where(a => a.Ativo != "N").ToList();
        return View(Atores);
    }

    // GET: /Actor/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.ToListAsync(),
            "Id_Comunidade",
            "Nome"
        );

        var novoAtor = new Atores
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(novoAtor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atores ator, int ComunidadeId, List<string>? recursos = null, List<string>? vulnerabilidades = null)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        var comunidadeExiste = await _context.Comunidades
            .AnyAsync(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N");
        ModelState.Remove(nameof(Atores.Usuario));
        if (!ModelState.IsValid || !comunidadeExiste)
        {
            if (!comunidadeExiste)
                ModelState.AddModelError(nameof(ComunidadeId), "A comunidade selecionada não existe ou está inativa.");
            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.Where(item => item.Ativo != "N").OrderBy(item => item.Nome).ToListAsync(),
                "Id_Comunidade", "Nome", ComunidadeId);
            return View(ator);
        }
        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

        ator.ConfigureCreationAggregate(ComunidadeId, recursos, vulnerabilidades);
        _context.Atores.Add(ator);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Atores");
    }


    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        var ator = await _context.Atores.FindAsync(id);
        if (ator == null)
            return NotFound();

        ViewBag.UsuarioOriginal = _context.Usuarios
            .Where(z => z.IdUsuario == ator.FkIdUsuario)
            .FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios
            .Where(z => z.IdUsuario == ator.FkIdUsuarioM)
            .FirstOrDefault();

        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.FK_id_Atores == id);

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "Id_Comunidade",
            "Nome",
            atorCom?.FkIdComunidade 
        );

        return View(ator);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Atores atorFormulario, int id, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("ID") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N"))
            return NotFound();
        var atorDb = await _context.Atores.FindAsync(id);
        if (atorDb == null)
        {
            return NotFound();
        }

        atorDb.Nome = atorFormulario.Nome;
        atorDb.Genero = atorFormulario.Genero;
        atorDb.Idade = atorFormulario.Idade;
        atorDb.PapelSocial1 = atorFormulario.PapelSocial1;
        atorDb.PapelSocial2 = atorFormulario.PapelSocial2;
        atorDb.Telefone = atorFormulario.Telefone;
        atorDb.DaEquipe = atorFormulario.DaEquipe;
        atorDb.Lopiniao = atorFormulario.Lopiniao;
        atorDb.Mcomunidade = atorFormulario.Mcomunidade;
        atorDb.Rope = atorFormulario.Rope;
        atorDb.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        atorDb.DtModificacao = DateTime.Now;

        var atorCom = await _context.AtorComunidades.FirstOrDefaultAsync(ac => ac.FK_id_Atores == id);

        if (atorCom == null)
        {
            atorCom = new AtorComunidade
            {
                FK_id_Atores = id,
                FkIdComunidade = ComunidadeId
            };
            _context.AtorComunidades.Add(atorCom);
        }
        else
        {
            atorCom.FkIdComunidade = ComunidadeId;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    // POST: /Actor/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanDelete("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        if (id == null)
        {
            return NotFound();
        }

        var ator = await _context.Atores.FindAsync(id);
        if (ator != null)
        {
            ator.Ativo = "N";
            _context.Atores.Update(ator);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Atores");
        }
        return RedirectToAction("Index", "Atores");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
