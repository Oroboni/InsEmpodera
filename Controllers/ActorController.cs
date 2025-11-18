using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Empodera.Controllers;

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
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        ViewData["DisableMainScroll"] = "true"; 
        
        return View();
    }

    // GET: /Actor/Create
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        // Carrega o dropdown de comunidades
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "IdComunidade", 
            "Nome"
        );
        
        // [CORREÇÃO] Cria um novo Ator (vazio) com as datas
        // para que o rodapé "Criado em" funcione.
        var novoAtor = new Ator
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now 
            // Você pode pré-definir outros valores aqui se quiser
        };
        
        return View(novoAtor); // Passa o novo Ator (o Model) para a View
    }

    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        // [CORREÇÃO] Busca o Ator no banco de dados
        var ator = await _context.Atores.FindAsync(id);

        if (ator == null)
        {
            return NotFound(); // Ator não encontrado
        }

        // Carrega o dropdown de comunidades
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "IdComunidade", 
            "Nome"
            //, ator.ComunidadeId // Descomente se 'Ator' tiver ComunidadeId
        );

        // [CORREÇÃO] Passa o Ator (o Model) que encontramos para a View
        return View(ator);
    }
    
    // TODO: Você precisará adicionar os métodos [HttpPost] para Create e Edit
    // para salvar as mudanças no banco.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}