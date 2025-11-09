using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
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
        
        // Flag para o _Layout desativar o scroll principal (mesma lógica do Actor)
        ViewData["DisableMainScroll"] = "true"; 
        
        // No futuro, você carregará os perfis do banco:
        // var perfis = await _context.PerfisAcesso.ToListAsync();
        // return View(perfis);

        return View();
    }

    // GET: /AccessProfile/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View();
    }

    // GET: /AccessProfile/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        // No futuro, você buscará o perfil e suas permissões aqui:
        // var perfil = await _context.PerfisAcesso.FindAsync(id);
        // if (perfil == null) return NotFound();
        
        return View(); // Em um caso real: return View(perfil);
    }
}