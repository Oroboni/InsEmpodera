using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace InsEmpodera.Controllers;

public class ActivityController : Controller
{
    private readonly ApplicationDbContext _context;
    public ActivityController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var atividades = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .ThenInclude(ae => ae.Eixo)
            .ToListAsync();

        return View(atividades);
    }

    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        var model = new Atividades
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atividades atividade, List<int> EixosSelecionados)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        atividade.DtCriacao = DateTime.Now;
        atividade.DtModificacao = DateTime.Now;
        atividade.FkIdComunidade = 1;

        _context.Atividades.Add(atividade);
        await _context.SaveChangesAsync();

        if (EixosSelecionados != null && EixosSelecionados.Count > 0)
        {
            foreach (var eixoId in EixosSelecionados)
            {
                _context.AtividadesEixo.Add(new AtividadesEixo
                {
                    FkIdAtividade = atividade.IdAtividade,
                    FkIdEixo = eixoId
                });
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null) return NotFound();

        var atividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .ThenInclude(ae => ae.Eixo)
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        if (atividade == null) return NotFound();

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        return View(atividade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Atividades atividade, List<int> EixosSelecionados)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id != atividade.IdAtividade) return NotFound();

        var existingAtividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        if (existingAtividade == null) return NotFound();

        existingAtividade.Nome = atividade.Nome;
        existingAtividade.Descricao = atividade.Descricao;
        existingAtividade.DtModificacao = DateTime.Now;

        var existingEixoIds = existingAtividade.AtividadesEixos.Select(ae => ae.FkIdEixo).ToList();

        var eixosToAdd = EixosSelecionados.Except(existingEixoIds).ToList();
        var eixosToRemove = existingEixoIds.Except(EixosSelecionados).ToList();

        foreach (var eixoId in eixosToAdd)
        {
            _context.AtividadesEixo.Add(new AtividadesEixo
            {
                FkIdAtividade = existingAtividade.IdAtividade,
                FkIdEixo = eixoId
            });
        }

        foreach (var eixoId in eixosToRemove)
        {
            var atividadeEixo = await _context.AtividadesEixo
                .FirstOrDefaultAsync(ae => ae.FkIdAtividade == existingAtividade.IdAtividade && ae.FkIdEixo == eixoId);
            if (atividadeEixo != null)
            {
                _context.AtividadesEixo.Remove(atividadeEixo);
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
