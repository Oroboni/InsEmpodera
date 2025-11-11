using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsEmpodera.Controllers;

public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Report/
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        return View();
    }

    // GET: /Report/Rsc
    public async Task<IActionResult> Rsc(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        ViewBag.SelectedComunidadeId = comunidadeId;
        return View();
    }

    // GET: /Report/FirstContact
    public async Task<IActionResult> FirstContact(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) { return RedirectToAction("Index", "Account"); }
        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        ViewBag.SelectedComunidadeId = comunidadeId; 
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
        
        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        ViewBag.SelectedAtorId = atorId; 
        
        return View();
    }
}