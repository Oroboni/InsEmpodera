using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class PersonalProcessController : Controller
{
    private const string Module = "DiariosProcessoPessoal";
    private readonly ApplicationDbContext _context;

    public PersonalProcessController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? atorId, string? searchQuery)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanList(Module)) return StatusCode(StatusCodes.Status403Forbidden);

        await PopulateListsAsync(atorId);
        ViewBag.SelectedAtorId = atorId;
        ViewBag.SearchQuery = searchQuery;

        if (!atorId.HasValue)
            return View(new List<DiarioProcessoPessoal>());

        var query = _context.DiariosProcessoPessoal
            .AsNoTracking()
            .Include(d => d.Eixos)
            .ThenInclude(link => link.Eixo)
            .Where(d => d.FK_id_Atores == atorId.Value);

        if (!string.IsNullOrWhiteSpace(searchQuery))
            query = query.Where(d => d.Descricao.Contains(searchQuery.Trim()));

        return View(await query.OrderByDescending(d => d.Data).ThenByDescending(d => d.DtCriacao).ToListAsync());
    }

    public async Task<IActionResult> Create(int? atorId)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanCreate(Module)) return StatusCode(StatusCodes.Status403Forbidden);

        await PopulateListsAsync(atorId);
        return View(new DiarioProcessoPessoal { FK_id_Atores = atorId ?? 0, Data = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiarioProcessoPessoal diario, int[]? eixosIds)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanCreate(Module)) return StatusCode(StatusCodes.Status403Forbidden);

        RemoveNavigationValidation();
        await ValidateReferencesAsync(diario.FK_id_Atores, eixosIds);
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(diario.FK_id_Atores, eixosIds);
            return View(diario);
        }

        var now = DateTime.Now;
        diario.Descricao = diario.Descricao.Trim();
        diario.DtCriacao = now;
        diario.DtModificacao = now;
        diario.FkIdUsuario = user.IdUsuario;
        diario.FkIdUsuarioM = user.IdUsuario;
        diario.Eixos = DistinctEixos(eixosIds)
            .Select(id => new DiarioProcessoEixo { FkIdEixo = id })
            .ToList();
        _context.DiariosProcessoPessoal.Add(diario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { atorId = diario.FK_id_Atores });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanUpdate(Module)) return StatusCode(StatusCodes.Status403Forbidden);
        if (!id.HasValue) return NotFound();

        var diario = await _context.DiariosProcessoPessoal
            .AsNoTracking()
            .Include(d => d.Eixos)
            .FirstOrDefaultAsync(d => d.IdDiarioProcesso == id.Value);
        if (diario is null) return NotFound();

        await PopulateListsAsync(diario.FK_id_Atores, diario.Eixos.Select(e => e.FkIdEixo));
        ViewBag.CanDelete = user.CanDelete(Module);
        return View(diario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DiarioProcessoPessoal diario, int[]? eixosIds)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanUpdate(Module)) return StatusCode(StatusCodes.Status403Forbidden);
        if (id != diario.IdDiarioProcesso) return NotFound();

        var persisted = await _context.DiariosProcessoPessoal
            .Include(d => d.Eixos)
            .FirstOrDefaultAsync(d => d.IdDiarioProcesso == id);
        if (persisted is null) return NotFound();

        RemoveNavigationValidation();
        await ValidateReferencesAsync(diario.FK_id_Atores, eixosIds);
        if (!ModelState.IsValid)
        {
            diario.DtCriacao = persisted.DtCriacao;
            diario.DtModificacao = persisted.DtModificacao;
            await PopulateListsAsync(diario.FK_id_Atores, eixosIds);
            ViewBag.CanDelete = user.CanDelete(Module);
            return View(diario);
        }

        persisted.FK_id_Atores = diario.FK_id_Atores;
        persisted.Data = diario.Data;
        persisted.Descricao = diario.Descricao.Trim();
        persisted.DtModificacao = DateTime.Now;
        persisted.FkIdUsuarioM = user.IdUsuario;
        _context.DiariosProcessoEixos.RemoveRange(persisted.Eixos);
        persisted.Eixos = DistinctEixos(eixosIds)
            .Select(eixoId => new DiarioProcessoEixo { FkIdDiarioProcesso = id, FkIdEixo = eixoId })
            .ToList();
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { atorId = persisted.FK_id_Atores });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await GetLoggedUserAsync();
        if (user is null) return RedirectToAction("Index", "Account");
        if (!HasCanonicalPermission(user) || !user.CanDelete(Module)) return StatusCode(StatusCodes.Status403Forbidden);

        var diario = await _context.DiariosProcessoPessoal.FindAsync(id);
        if (diario is null) return NotFound();
        var actorId = diario.FK_id_Atores;
        _context.DiariosProcessoPessoal.Remove(diario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { atorId = actorId });
    }

    private async Task<Usuario?> GetLoggedUserAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("ID"), out var userId)) return null;
        return await _context.Usuarios.Include(u => u.Perfil).ThenInclude(p => p.Permissoes)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId && u.Ativo == "S");
    }

    private async Task PopulateListsAsync(int? actorId, IEnumerable<int>? selectedEixos = null)
    {
        ViewBag.AtorList = new SelectList(await _context.Atores.AsNoTracking().Where(a => a.Ativo == "S")
            .OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", actorId);
        ViewBag.EixosList = await _context.Eixos.AsNoTracking().OrderBy(e => e.Nome).ToListAsync();
        ViewBag.SelectedEixos = selectedEixos?.ToHashSet() ?? new HashSet<int>();
    }

    private async Task ValidateReferencesAsync(int actorId, int[]? eixosIds)
    {
        if (!await _context.Atores.AnyAsync(a => a.IdAtores == actorId && a.Ativo == "S"))
            ModelState.AddModelError(nameof(DiarioProcessoPessoal.FK_id_Atores), "Selecione um ator ativo.");
        var ids = DistinctEixos(eixosIds).ToArray();
        if (ids.Length > 0 && await _context.Eixos.CountAsync(e => ids.Contains(e.IdEixo)) != ids.Length)
            ModelState.AddModelError("eixosIds", "Um ou mais eixos selecionados não existem.");
    }

    private void RemoveNavigationValidation()
    {
        ModelState.Remove(nameof(DiarioProcessoPessoal.Ator));
        ModelState.Remove(nameof(DiarioProcessoPessoal.Usuario));
        ModelState.Remove(nameof(DiarioProcessoPessoal.UsuarioModificacao));
        ModelState.Remove(nameof(DiarioProcessoPessoal.Eixos));
    }

    private static IEnumerable<int> DistinctEixos(int[]? values) =>
        (values ?? Array.Empty<int>()).Where(id => id > 0).Distinct();

    private static bool HasCanonicalPermission(Usuario? user) =>
        user?.Perfil?.Permissoes?.Any(p => p.Modulo == "DiariosProcessoPessoal") == true;
}
