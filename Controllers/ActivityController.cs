using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsEmpodera.Controllers;

public class ActivityController : Controller
{
    private readonly ApplicationDbContext _context;

    public ActivityController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Activity/
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        ViewData["DisableMainScroll"] = "true"; 
        
        // No futuro, você vai carregar as atividades e seus eixos
        // var atividades = await _context.Atividades.Include(a => a.AtividadesEixo).ThenInclude(ae => ae.Eixo).ToListAsync();
        // return View(atividades);

        return View();
    }

    // GET: /Activity/Create
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        // Carrega a lista de Eixos para o dropdown
        ViewBag.EixosList = new SelectList(
            await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(),
            "IdEixo",
            "Nome"
        );
        
        // Envia um model vazio para o rodapé padronizado
        var model = new Atividade
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(model);
    }

    // GET: /Activity/Edit/5
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

        // Busca a Atividade e seus Eixos
        var atividade = await _context.Atividades.FindAsync(id); // (Você precisará incluir os Eixos)
            
        if (atividade == null)
        {
            return NotFound();
        }

        // Carrega a lista de Eixos para o dropdown (se necessário)
        ViewBag.EixosList = new SelectList(
            await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(),
            "IdEixo",
            "Nome"
        );

        return View(atividade);
    }
}