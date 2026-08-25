using Empodera.Data;
using Empodera.Models;
using Empodera.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

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
            SafeDownloadFileName(community.Nome, community.Id_Comunidade));
    }

    private static string SafeDownloadFileName(string? communityName, int communityId)
    {
        var source = string.IsNullOrWhiteSpace(communityName)
            ? $"comunidade-{communityId}"
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

        if (string.IsNullOrWhiteSpace(safeName))
            safeName = $"comunidade-{communityId}";
        if (safeName.Length > 80)
            safeName = safeName[..80].TrimEnd();

        return $"{safeName}.xlsx";
    }
}