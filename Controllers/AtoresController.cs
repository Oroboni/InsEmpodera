using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Empodera.Controllers;

public class AtoresController : Controller
{
    private readonly ILogger<AtoresController> _logger;
    private readonly ApplicationDbContext _context;

    public AtoresController(ILogger<AtoresController> logger, ApplicationDbContext context)
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
        
        var Atores = _context.Atores.Where(a => a.Ativo != "N").ToList();
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
            await _context.Comunidades.ToListAsync(), 
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atores ator, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID"));

        _context.Atores.Add(ator);
        await _context.SaveChangesAsync();

        var relacao = new AtorComunidade
        {
            FkIdComunidade = ComunidadeId,
            FKidAtores = ator.IdAtores
        };

        _context.AtorComunidades.Add(relacao);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Atores");
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

        ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuario).FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuarioM).FirstOrDefault();

        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.FKidAtores == id);


        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "IdComunidade",
            "Nome",
            atorCom?.IdAtorComunidade
        );

        return View(ator);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(Atores atores,int id, int ComunidadeId)
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
            var atorDb = await _context.Atores.FindAsync(atores.IdAtores);
            if (atorDb == null)
                return NotFound();

            atorDb.Nome = ator.Nome;
            atorDb.Genero = ator.Genero;
            atorDb.DtNascimento = ator.DtNascimento;
            atorDb.PapelSocial1 = ator.PapelSocial1;
            atorDb.PapelSocial2 = ator.PapelSocial2;
            atorDb.Telefone = ator.Telefone;
            atorDb.DaEquipe = ator.DaEquipe;
            atorDb.Lopiniao = ator.Lopiniao;
            atorDb.Mcomunidade = ator.Mcomunidade;
            atorDb.Rope = ator.Rope;
            atorDb.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID"));
            atorDb.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();
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
            ator.Ativo = "N";
            _context.Atores.Update(ator);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", "Atores");
    }

    // TODO: Você precisará adicionar os métodos [HttpPost] para Create e Edit
    // para salvar as mudanças no banco.

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}