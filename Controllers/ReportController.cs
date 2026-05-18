using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Empodera.Controllers;

public class ReporteController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReporteController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Report/
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View();
    }


    // GET: /Report/Rsc
    public async Task<IActionResult> Rsc(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) 
        { 
            return RedirectToAction("Index", "Account"); 
        }
        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        return View();
    }

    // GET: /Report/FirstContact
    public async Task<IActionResult> FirstContact(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        return View();
    }

    // GET: /Report/PersonalAssessment
    public async Task<IActionResult> PersonalAssessment(int? atorId)
    {
         if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        ViewBag.SelectedAtorId = atorId; 
        return View();
    }

    // GET: /Report/Actions
    public async Task<IActionResult> Actions(int? comunidadeId, int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        return View();
    }

    // GET: /Report/PrimaryNetwork
    public async Task<IActionResult> PrimaryNetwork(int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        
        // Carrega a lista de atores e mantem o selecionado
        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        
        // Envia o ID para a View saber se deve mostrar o gráfico
        ViewBag.SelectedAtorId = atorId; 
        
        return View();
    }
}