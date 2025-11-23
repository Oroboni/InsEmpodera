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
        
        var perfis = await _context.Perfis.OrderBy(p => p.Nome).ToListAsync();
        
        return View(perfis);
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
        var novoPerfil = new Perfil
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

        var perfil = await _context.Perfis.FindAsync(id);
        if (perfil == null)
        {
            return NotFound();
        }
        
        return View(perfil);
    }

}