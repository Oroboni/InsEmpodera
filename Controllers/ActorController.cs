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

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        var Atores = _context.Atores.ToList();
        return View(Atores);
    }

    // GET: /Actor/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "IdComunidade", 
            "Nome"
        );
        
        var novoAtor = new Atores
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now 
        };
        
        return View(novoAtor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Atores ator, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (ModelState.IsValid)
        {
            ator.DtCriacao = DateTime.Now;
            ator.DtModificacao = DateTime.Now;

            _context.Atores.Add(ator);
            await _context.SaveChangesAsync();

            var relacao = new AtorComunidade
            {
                FkIdComunidade = ComunidadeId,
                FKidAtores = ator.IdAtores
            };

            _context.AtorComunidades.Add(relacao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Actor");
        }

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "IdComunidade",
            "Nome"
        );

        return View(ator);
    }


    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var ator = await _context.Atores.FindAsync(id);
        if (ator == null)
            return NotFound();

        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.IdAtorComunidade == id);

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "IdComunidade",
            "Nome",
            atorCom?.IdAtorComunidade
        );

        return View(ator);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(int id, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var ator = await _context.Atores.FindAsync(id);
        if (ator == null)
            return NotFound();

        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.IdAtorComunidade == id);

        if (atorCom == null)
        {
            atorCom = new AtorComunidade
            {
                FKidAtores = id,
                FkIdComunidade = ComunidadeId
            };
            _context.AtorComunidades.Add(atorCom);
        }
        else
        {
            atorCom.FkIdComunidade = ComunidadeId;
            _context.AtorComunidades.Update(atorCom);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    // GET: /Actor/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }
        
        var ator = await _context.Atores.FindAsync(id);
        if (ator != null)
        {
            var relacoes = _context.AtorComunidades.Where(ac => ac.IdAtorComunidade == id);
            _context.AtorComunidades.RemoveRange(relacoes);
            await _context.SaveChangesAsync();
            _context.Atores.Remove(ator);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", "Actor");
    }

    // TODO: Você precisará adicionar os métodos [HttpPost] para Create e Edit
    // para salvar as mudanças no banco.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}