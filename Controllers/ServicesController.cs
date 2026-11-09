using Empodera.Data;
using Empodera.Models;
using Empodera.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SpreadsheetExportService _exports;
    private readonly ExcelBackupService _backup;

    public ServicesController(
        ApplicationDbContext context,
        SpreadsheetExportService? exports = null,
        ExcelBackupService? backup = null)
    {
        _context = context;
        _exports = exports ?? new SpreadsheetExportService(context);
        _backup = backup ?? new ExcelBackupService(context);
    }

    [HttpGet]
    public async Task<IActionResult> ExportComunidades()
    {
        var user = await AuthorizedUserAsync("Comunidades");
        if (user.Result != null) return user.Result;
        return Spreadsheet(await _exports.ExportCommunitiesAsync(HttpContext.RequestAborted), "comunidades-empodera.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportComunidadeCompleta(int id)
    {
        var user = await AuthorizedUserAsync("Comunidades");
        if (user.Result != null) return user.Result;
        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted))
            return NotFound();
        var content = await _exports.ExportCommunityAsync(id, HttpContext.RequestAborted);
        var community = await _context.Comunidades.AsNoTracking().FirstOrDefaultAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted);
        return Spreadsheet(content, $"{community?.Nome}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportAtoresComunidade(int id)
    {
        var user = await AuthorizedUserAsync("Atores");
        if (user.Result != null) return user.Result;
        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted))
            return NotFound();
        var community = await _context.Comunidades.AsNoTracking().FirstOrDefaultAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted);
        return Spreadsheet(await _exports.ExportCommunityActorsAsync(id, HttpContext.RequestAborted), $"{community?.Nome}-atores.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportAtividadesComunidade(int id)
    {
        var user = await AuthorizedUserAsync("Atividades");
        if (user.Result != null) return user.Result;
        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted))
            return NotFound();
        var community = await _context.Comunidades.AsNoTracking().FirstOrDefaultAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted);
        return Spreadsheet(await _exports.ExportCommunityActivitiesAsync(id, HttpContext.RequestAborted), $"{community?.Nome}-atividades.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportRecursosComunidade(int id)
    {
        var user = await AuthorizedUserAsync("Recursos");
        if (user.Result != null) return user.Result;
        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted))
            return NotFound();
        var community = await _context.Comunidades.AsNoTracking().FirstOrDefaultAsync(item => item.Id_Comunidade == id, HttpContext.RequestAborted);
        return Spreadsheet(await _exports.ExportCommunityResourcesAsync(id, HttpContext.RequestAborted), $"{community?.Nome}-recursos.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportAtores()
    {
        var user = await AuthorizedUserAsync("Atores");
        if (user.Result != null) return user.Result;
        return Spreadsheet(await _exports.ExportActorsAsync(HttpContext.RequestAborted), "atores-empodera.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDiariosCampo()
    {
        var user = await AuthorizedUserAsync("DiariosCampo");
        if (user.Result != null) return user.Result;
        return Spreadsheet(await _exports.ExportFieldDiariesAsync(HttpContext.RequestAborted), "diarios-de-campo.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportFichasPrimeiroContato()
    {
        var user = await AuthorizedUserAsync("Ficha1Contato");
        if (user.Result != null) return user.Result;
        return Spreadsheet(await _exports.ExportFirstContactsAsync(HttpContext.RequestAborted), "fichas-primeiro-contato.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBackupGeral()
    {
        var user = await AuthorizedUserAsync("SER", adminOnly: true);
        if (user.Result != null) return user.Result;
        var content = await _backup.ExportAsync(HttpContext.RequestAborted);
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"insempodera-backup-geral-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
    }

    private async Task<(Usuario? User, IActionResult? Result)> AuthorizedUserAsync(string module, bool adminOnly = false)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return (null, RedirectToAction("Index", "Account"));
        var id = int.TryParse(HttpContext.Session.GetString("ID"), out var parsed) ? parsed : 0;
        var user = await _context.Usuarios.AsNoTracking().Include(item => item.Perfil)
            .ThenInclude(item => item.Permissoes).FirstOrDefaultAsync(item => item.IdUsuario == id, HttpContext.RequestAborted);
        if (user == null || !user.CanViewDetails(module) || adminOnly && !string.Equals(user.Perfil?.Nome, "Admin", StringComparison.OrdinalIgnoreCase))
            return (user, StatusCode(StatusCodes.Status403Forbidden));
        return (user, null);
    }

    private FileContentResult Spreadsheet(byte[] content, string fileName) => File(
        content,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);

    [HttpGet]
    public async Task<IActionResult> ExportComunidade(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var loggedUserId = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        var loggedUser = await _context.Usuarios
            .AsNoTracking()
            .Include(user => user.Perfil)
            .ThenInclude(profile => profile.Permissoes)
            .FirstOrDefaultAsync(
                user => user.IdUsuario == loggedUserId,
                HttpContext.RequestAborted);
        if (!loggedUser.CanViewDetails("Comunidades"))
            return StatusCode(StatusCodes.Status403Forbidden);

        var community = await _context.Comunidades
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.RedeRecursos)
            .Include(item => item.Atividades)
            .FirstOrDefaultAsync(
                item => item.Id_Comunidade == id && item.Ativo == "S",
                HttpContext.RequestAborted);
        if (community == null)
            return NotFound();

        var actors = await _context.AtorComunidades
            .AsNoTracking()
            .Where(link =>
                link.FkIdComunidade == id &&
                link.Ator.Ativo == "S")
            .Include(link => link.Ator)
                .ThenInclude(actor => actor.Redes)
            .Include(link => link.Ator)
                .ThenInclude(actor => actor.Avaliacoes)
            .Select(link => link.Ator)
            .ToListAsync(HttpContext.RequestAborted);

        var content = new ExportComunidade()
            .GerarRelatorioComunidade(new[] { community }, community, actors);

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            SafeDownloadFileName(community.Nome));
    }

    private static string SafeDownloadFileName(string? communityName)
    {
        var source = string.IsNullOrWhiteSpace(communityName)
            ? $"comunidade-{communityName}"
            : communityName.Trim();
        var invalidCharacters = Path.GetInvalidFileNameChars()
            .Concat(new[] { '<', '>', ':', (char)34, '/', (char)92, '|', '?', '*' })
            .ToHashSet();
        var safeName = new string(source
            .Select(character =>
                char.IsControl(character) || invalidCharacters.Contains(character)
                    ? '_'
                    : character)
            .ToArray())
            .Trim(' ', '.');

        while (safeName.Contains("..", StringComparison.Ordinal))
            safeName = safeName.Replace("..", "_", StringComparison.Ordinal);

        if (safeName.Length > 80)
            safeName = safeName[..80].TrimEnd();

        return $"{safeName}.xlsx";
    }
}
