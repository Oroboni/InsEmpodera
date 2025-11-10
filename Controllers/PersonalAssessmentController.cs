using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsEmpodera.Controllers;

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

        // 1. Carregar a lista de todos os atores para o dropdown principal
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId // O ID do ator selecionado (se houver)
        );

        // 2. Guardar o ID selecionado
        ViewBag.SelectedAtorId = atorId;

        // 3. Buscar as avaliações APENAS se um ator foi selecionado
        List<AvaliacaoPessoal> avaliacoes = new List<AvaliacaoPessoal>();
        if (atorId.HasValue)
        {
            avaliacoes = await _context.AvaliacoesPessoais
                .Where(a => a.AtorId == atorId.Value)
                .OrderByDescending(a => a.DtCriacao) // Mostrar mais nova primeiro
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
        
        // Carrega a lista de atores para o dropdown do formulário
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId // Pré-seleciona o ator vindo do Index
        );

        return View();
    }

    // GET: /PersonalAssessment/Edit/5
    public async Task<IActionResult> Edit(int? id) // id da Avaliação
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        var avaliacao = await _context.AvaliacoesPessoais.FindAsync(id);
        if (avaliacao == null)
        {
            return NotFound();
        }
        
        // Carrega a lista de atores para o dropdown (para mostrar o nome)
        ViewBag.AtoresList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            avaliacao.AtorId // Pré-seleciona o ator da avaliação
        );

        return View(avaliacao);
    }
}