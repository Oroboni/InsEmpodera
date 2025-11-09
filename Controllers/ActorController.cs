using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // <-- ADICIONADO

namespace InsEmpodera.Controllers;

public class ActorController : Controller
{
    private readonly ILogger<ActorController> _logger;
    private readonly ApplicationDbContext _context;

    public ActorController(ILogger<ActorController> logger, ApplicationDbContext context)
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
        
        // Flag para o _Layout desativar o scroll principal
        ViewData["DisableMainScroll"] = "true"; 
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id) // <-- AJUSTADO
    {
        // Aqui você buscaria o ator pelo ID, por exemplo:
        // var ator = await _context.Atores.FindAsync(id);
        // if (ator == null) return NotFound();

        // [NOVO] Carrega as comunidades para o dropdown
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "IdComunidade", 
            "Nome"
            //, ator.ComunidadeId // Descomente e ajuste para pré-selecionar
        );

        return View(); // Em um caso real: return View(ator);
    }

    // GET: /Actor/Create
    public async Task<IActionResult> Create() // <-- AJUSTADO
    {
        // [NOVO] Carrega as comunidades para o dropdown
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "IdComunidade", 
            "Nome"
        );
        
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}