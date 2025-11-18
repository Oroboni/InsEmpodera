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
        
        // CORREÇÃO: Busca todos os objetos 'Usuario' na DbSet 'Usuarios'
        var usuarios = await _context.Usuarios.ToListAsync(); 
        
        // CORREÇÃO: Passa a lista de usuários para a View
        return View(usuarios); 
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

        if (id == null)
        {
            return NotFound();
        }

        // Buscando o USUÁRIO (Usuario) no banco de dados.
        // **Ajuste o nome 'Usuarios' se sua DbSet no ApplicationDbContext for diferente.**
        // Aqui assumimos que o ID é o IdUsuario
        var user = await _context.Usuarios.FindAsync(id); 

        if (user == null)
        {
            return NotFound(); // Usuário não encontrado
        }
        
        // Passa o objeto Usuario encontrado para a View.
        return View(user); 
    }
    
    // TODO: Você precisará adicionar os métodos [HttpPost] para Create e Edit
    // para salvar as mudanças no banco.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}