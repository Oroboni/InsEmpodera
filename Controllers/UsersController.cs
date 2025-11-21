using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace InsEmpodera.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly ApplicationDbContext _context;

    public UsersController(ILogger<UsersController> logger, ApplicationDbContext context)
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
    
    ViewData["DisableMainScroll"] = "true"; 
    
    // NENHUM OBJETO USUARIO É CRIADO OU PASSADO.
    // Apenas retornamos a View. A View Index agora deve ter dados fixos.
    return View(); 
}

    // GET: /Actor/Create
    public async Task<IActionResult> Create()
    {
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
        // para que o rodapé "Criado em" não dê erro.
        var novoAtor = new Ator
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now 
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

        // Não buscamos nada no contexto ou usamos o ID.
        // Apenas retornamos a View. A View Edit agora deve ter dados fixos.
        return View(); 
    }
    
    // TODO: Você precisará adicionar os métodos [HttpPost] para Create e Edit
    // para salvar as mudanças no banco.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}