using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PersonalAssessmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public PersonalAssessmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /PersonalAssessment/
    // Opcionalmente, pode receber o ID do ator
    public async Task<IActionResult> Index(int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeAvaliacao = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (!PodeAvaliacao.CanList("AvaliacoesPessoais"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        ViewBag.AtoresList = new SelectList(
            await _context.Atores.Where(a => a.Ativo == "S").OrderBy(a => a.IdAtores).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId
        );

        // 2. Guardar o ID selecionado
        ViewBag.SelectedAtorId = atorId;

        List<AvaliacaoPessoal> avaliacoes = new List<AvaliacaoPessoal>();
        if (atorId.HasValue)
        {
            avaliacoes = await _context.AvaliacaoPessoal
                .Include(b => b.Usuario)
                .Where(a => a.FK_id_Atores == atorId.Value)
                .OrderByDescending(a => a.DtCriacao)
                .ToListAsync();
        }

        // Flag para o layout não ter scroll
        ViewData["DisableMainScroll"] = "true"; 

        return View(avaliacoes); // Envia a lista (vazia ou não) para a View
    }

    // GET: /PersonalAssessment/Create
    public async Task<IActionResult> Create(int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeAvaliacao = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (!PodeAvaliacao.CanCreate("AvaliacoesPessoais"))
        {
            return RedirectToAction("Index", "PersonalAssessment");
        }

        ViewBag.AtorId = atorId;
        
        // Carrega a lista de atores para o dropdown do formulário
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.Where(a => a.Ativo == "S").OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> create(AvaliacaoPessoal avaliacao)
    {
        if (HttpContext.Session.GetString("ID") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeAvaliacao = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (!PodeAvaliacao.CanCreate("AvaliacoesPessoais"))
        {
            return RedirectToAction("Index", "PersonalAssessment");
        }

        ModelState.Remove(nameof(AvaliacaoPessoal.Usuario));
        ModelState.Remove(nameof(AvaliacaoPessoal.Ator));
        var actorIsActive = await _context.Atores
            .AnyAsync(actor => actor.IdAtores == avaliacao.FK_id_Atores && actor.Ativo == "S");
        if (!actorIsActive)
            ModelState.AddModelError(nameof(AvaliacaoPessoal.FK_id_Atores), "O ator selecionado não existe ou está inativo.");

        if (!ModelState.IsValid)
        {
            await PopulateActorsListAsync(avaliacao.FK_id_Atores);
            ViewBag.AtorId = avaliacao.FK_id_Atores;
            return View("Create", avaliacao);
        }

        avaliacao.DtCriacao = DateTime.Now;
        avaliacao.DtModificacao = DateTime.Now;
        avaliacao.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        _context.AvaliacaoPessoal.Add(avaliacao);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "PersonalAssessment", new {atorId = avaliacao.FK_id_Atores});
    }

    // GET: /PersonalAssessment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        var PodeAvaliacao = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (!PodeAvaliacao.CanUpdate("AvaliacoesPessoais"))
        {
            return RedirectToAction("Index", "PersonalAssessment");
        }

        var avaliacao = await _context.AvaliacaoPessoal.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.IdAvaliacao == id);
        if (avaliacao == null)
        {
            return NotFound();
        }
        
        // Carrega a lista de atores para o dropdown (para mostrar o nome)
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.Where(a => a.Ativo == "S").OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            avaliacao.FK_id_Atores 
        );

        ViewBag.atorId = id;

        return View(avaliacao);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> edit(AvaliacaoPessoal avaliacao, int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        var PodeAvaliacao = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (!PodeAvaliacao.CanUpdate("AvaliacoesPessoais"))
        {
            return RedirectToAction("Index", "PersonalAssessment");
        }

        var avaliacaobd = await _context.AvaliacaoPessoal.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.IdAvaliacao == id);
        if (avaliacaobd == null)
        {
            return NotFound();
        } 
        ModelState.Remove(nameof(AvaliacaoPessoal.Usuario));
        ModelState.Remove(nameof(AvaliacaoPessoal.Ator));
        var actorIsActive = await _context.Atores
            .AnyAsync(actor => actor.IdAtores == avaliacaobd.FK_id_Atores && actor.Ativo == "S");
        if (!actorIsActive)
            ModelState.AddModelError(nameof(AvaliacaoPessoal.FK_id_Atores), "O ator da avaliação não existe ou está inativo.");

        if (!ModelState.IsValid)
        {
            avaliacao.IdAvaliacao = avaliacaobd.IdAvaliacao;
            avaliacao.FK_id_Atores = avaliacaobd.FK_id_Atores;
            avaliacao.DtCriacao = avaliacaobd.DtCriacao;
            avaliacao.DtModificacao = avaliacaobd.DtModificacao;
            avaliacao.FkIdUsuario = avaliacaobd.FkIdUsuario;
            avaliacao.Usuario = avaliacaobd.Usuario;
            await PopulateActorsListAsync(avaliacaobd.FK_id_Atores);
            ViewBag.atorId = avaliacaobd.FK_id_Atores;
            return View("Edit", avaliacao);
        }

        avaliacaobd.AssBasica = avaliacao.AssBasica;
        avaliacaobd.CCrimes = avaliacao.CCrimes;
        avaliacaobd.DtModificacao = DateTime.Now;
        avaliacaobd.Lazer = avaliacao.Lazer;
        avaliacaobd.Educacao = avaliacao.Educacao;
        avaliacaobd.Moradia = avaliacao.Moradia;
        avaliacaobd.Ocupacao = avaliacao.Ocupacao;
        avaliacaobd.Prevencao = avaliacao.Prevencao;
        avaliacaobd.Saude = avaliacao.Saude;
        avaliacaobd.Substancias = avaliacao.Substancias;

        _context.AvaliacaoPessoal.Update(avaliacaobd);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "PersonalAssessment", new {atorId = avaliacaobd.FK_id_Atores});
    }
    private async Task PopulateActorsListAsync(int? selectedActorId)
    {
        ViewBag.AtoresList = new SelectList(
            await _context.Atores
                .Where(actor => actor.Ativo == "S")
                .OrderBy(actor => actor.Nome)
                .ToListAsync(),
            "IdAtores",
            "Nome",
            selectedActorId);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var loggedUserId = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        var loggedUser = await _context.Usuarios
            .Include(user => user.Perfil)
            .ThenInclude(profile => profile.Permissoes)
            .FirstOrDefaultAsync(user => user.IdUsuario == loggedUserId);
        if (!loggedUser.CanDelete("AvaliacoesPessoais"))
            return RedirectToAction(nameof(Index));

        var assessment = await _context.AvaliacaoPessoal.FindAsync(id.Value);
        if (assessment == null)
            return NotFound();

        var actorId = assessment.FK_id_Atores;
        _context.AvaliacaoPessoal.Remove(assessment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { atorId = actorId });
    }
}
