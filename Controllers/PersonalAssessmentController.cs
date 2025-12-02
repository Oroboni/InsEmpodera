using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SQLitePCL;

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

        ViewBag.AtoresList = new SelectList(
            await _context.Atores.Where(a => a.Ativo == "S").OrderBy(a => a.IdAtores).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId
        );

        // 2. Guardar o ID selecionado
        ViewBag.SelectedAtorId = atorId;

        // 3. Buscar as avaliações APENAS se um ator foi selecionado
        List<AvaliacaoPessoal> avaliacoes = new List<AvaliacaoPessoal>();
        if (atorId.HasValue)
        {
            avaliacoes = await _context.AvaliacaoPessoal
                .Include(b => b.Usuario)
                .Where(a => a.FKidAtores == atorId.Value)
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

        ViewBag.AtorId = atorId;
        
        // Carrega a lista de atores para o dropdown do formulário
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId
        );

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> create(AvaliacaoPessoal avaliacao)
    {
        if (HttpContext.Session.GetString("ID") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        avaliacao.DtCriacao = DateTime.Now;
        avaliacao.DtModificacao = DateTime.Now;
        avaliacao.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID"));
        _context.AvaliacaoPessoal.Add(avaliacao);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "PersonalAssessment", new {atorId = avaliacao.FKidAtores});
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

        var avaliacao = await _context.AvaliacaoPessoal.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.IdAvaliacao == id);
        if (avaliacao == null)
        {
            return NotFound();
        }
        
        // Carrega a lista de atores para o dropdown (para mostrar o nome)
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            avaliacao.FKidAtores 
        );

        ViewBag.atorId = id;

        return View(avaliacao);
    }

    [HttpPost]
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

        var avaliacaobd = await _context.AvaliacaoPessoal.FirstOrDefaultAsync(a => a.IdAvaliacao == id);
        if (avaliacaobd == null)
        {
            return NotFound();
        } 
        avaliacaobd.AssBasica = avaliacao.AssBasica;
        avaliacaobd.CCrimes = avaliacao.CCrimes;
        avaliacaobd.DtModificacao = DateTime.Now;
        avaliacaobd.Lazer = avaliacao.Lazer;
        avaliacaobd.Moradia = avaliacao.Moradia;
        avaliacaobd.Ocupacao = avaliacao.Ocupacao;
        avaliacaobd.Prevencao = avaliacao.Prevencao;
        avaliacaobd.Saude = avaliacao.Saude;
        avaliacaobd.Substancias = avaliacao.Substancias;

        _context.AvaliacaoPessoal.Update(avaliacaobd);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "PersonalAssessment", new {atorId = avaliacaobd.FkIdUsuario});
    }
}