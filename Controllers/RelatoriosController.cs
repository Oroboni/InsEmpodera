using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class RelatoriosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RelatoriosController(ILogger<AtoresController> logger, ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanList("SER"));
        return denied ?? View();
    }

    public async Task<IActionResult> Rsc()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    public async Task<IActionResult> PrimaryNetwork()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    public async Task<IActionResult> PersonalAssessment()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        return denied ?? View();
    }

    public async Task<IActionResult> FirstContact()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    public async Task<IActionResult> Actions()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    private async Task<IActionResult?> RequireSerPermissionAsync(Func<Usuario?, bool> permission)
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

        return permission(loggedUser)
            ? null
            : RedirectToAction("Index", "Home");
    }

    private async Task PopulateActiveCommunitiesAsync()
    {
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades
                .AsNoTracking()
                .Where(community => community.Ativo != "N")
                .OrderBy(community => community.Nome)
                .ToListAsync(HttpContext.RequestAborted),
            "Id_Comunidade",
            "Nome");
    }
}