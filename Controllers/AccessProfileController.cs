using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace InsEmpodera.Controllers;

public class AccessProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccessProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /AccessProfile/
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        ViewData["DisableMainScroll"] = "true"; 
        
        // Agora busca os perfis reais
        var perfis = await _context.PerfisAcesso.OrderBy(p => p.Nome).ToListAsync();
        
        return View(perfis); // Envia a lista para a View
    }

    // GET: /AccessProfile/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        // [CORREÇÃO] Envia um novo Model (vazio) para a View
        // para que @Model.DtCriacao não dê erro
        var novoPerfil = new PerfilAcesso
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };
        
        return View(novoPerfil);
    }

    // GET: /AccessProfile/Edit/5
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

        // [CORREÇÃO] Busca o perfil no banco
        var perfil = await _context.PerfisAcesso.FindAsync(id);
        if (perfil == null)
        {
            return NotFound();
        }
        
        return View(perfil); // Envia o perfil encontrado para a View
    }

    // TODO: Adicionar [HttpPost] Create e Edit para salvar os dados
}