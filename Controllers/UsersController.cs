using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.AspNetCore.Identity;


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

    var users = await _context.Usuarios.ToListAsync();
    
    ViewData["DisableMainScroll"] = "true"; 
    
    return View(users); 
}

    // GET: /Actor/Create
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        ViewBag.PerfilLista = new SelectList(
            await _context.Perfis.OrderBy(a => a.Nome).ToListAsync(),
            "IdPerfil",
            "Nome"
        );
        
        return View();
    }

    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == usuario.Email);

        if (user != null)
        {
            ViewBag.ErrorMessage = "Email já cadastrado.";
            return View();
        }

        var hasher = new PasswordHasher<Usuario>();
        usuario.Senha = hasher.HashPassword(usuario, usuario.Senha);

        usuario.DtCriacao = DateTime.Now;
        usuario.DtAtualizacao = DateTime.Now;
        usuario.Ativo = "S";

        _context.Add(usuario);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
            return NotFound();

        ViewBag.PerfilLista = new SelectList(
            await _context.Perfis.OrderBy(a => a.Nome).ToListAsync(),
            "IdPerfil",
            "Nome"
        );

        var usuario = await _context.Usuarios.FindAsync(id);


        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Senha = "";

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Usuario usuario)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var existingUser = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == usuario.Email && u.IdUsuario != id);

        if (existingUser != null)
        {
            ViewBag.ErrorMessage = "Email já cadastrado.";
            ViewBag.PerfilLista = new SelectList(
                await _context.Perfis.OrderBy(a => a.Nome).ToListAsync(),
                "IdPerfil",
                "Nome",
                usuario.FkIdPerfil
            );
            return View(usuario);
        }

        var usuariobd = await _context.Usuarios.FindAsync(id);
        if (usuariobd == null) return NotFound();

        usuariobd.Nome = usuario.Nome;
        usuariobd.Email = usuario.Email;
        usuariobd.FkIdPerfil = usuario.FkIdPerfil;
        usuariobd.Ativo = usuario.Ativo;
        usuariobd.Ocupacao = usuario.Ocupacao;
        usuariobd.Genero = usuario.Genero;
        usuariobd.DtAtualizacao = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(usuario.Senha))
        {
            var hasher = new PasswordHasher<Usuario>();
            usuariobd.Senha = hasher.HashPassword(usuario, usuario.Senha);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("index", "Users");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}