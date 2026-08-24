using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Empodera.Controllers;

public class AtividadesController : Controller
{
    private readonly ApplicationDbContext _context;
    public AtividadesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
        {
            return RedirectToAction("Index", "Atividades");
        }

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

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("Index", "Atividades");
        }

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), 
            "Id_Comunidade", 
            "Nome"
        );

        var model = new Atividades
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atividades atividade, List<int> EixosSelecionados, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("Index", "Atividades");
        }

        atividade.DtCriacao = DateTime.Now;
        atividade.DtModificacao = DateTime.Now;
        atividade.FkIdComunidade = ComunidadeId;
        atividade.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

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

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "Atividades");
        }

        if (id == null) return NotFound();

        var atividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .ThenInclude(ae => ae.Eixo)
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "Id_Comunidade",
            "Nome",
            atividade?.FkIdComunidade
        );

        if (atividade == null) return NotFound();

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();
        ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == atividade.FkIdUsuario).FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == atividade.FkIdUsuarioM).FirstOrDefault();

        return View(atividade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Atividades atividade, List<int> EixosSelecionados, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "Atividades");
        }
        
        if (id != atividade.IdAtividade) return NotFound();

        var existingAtividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        if (existingAtividade == null) return NotFound();

        existingAtividade.Nome = atividade.Nome;
        existingAtividade.Descricao = atividade.Descricao;
        existingAtividade.FkIdComunidade = ComunidadeId;
        existingAtividade.DtModificacao = DateTime.Now;
        existingAtividade.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var loggedUserId = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        var loggedUser = await _context.Usuarios
            .Include(user => user.Perfil)
            .ThenInclude(profile => profile.Permissoes)
            .FirstOrDefaultAsync(user => user.IdUsuario == loggedUserId);
        var permission = loggedUser?.Perfil.Permissoes.FirstOrDefault(item => item.Modulo == "Atividades");
        if (permission?.PodeDeletar != "S")
            return RedirectToAction(nameof(Index));

        var activity = await _context.Atividades.FindAsync(id.Value);
        if (activity == null)
            return NotFound();

        _context.Atividades.Remove(activity);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
