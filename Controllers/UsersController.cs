using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.AspNetCore.Identity;


namespace InsEmpodera.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario>? _userManager;

    public UsersController(
        ILogger<UsersController> logger,
        ApplicationDbContext context,
        UserManager<Usuario>? userManager = null)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
{
    if (HttpContext.Session.GetString("Email") == null)
    {
        return RedirectToAction("Index", "Account");
    }

    var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Usuarios")).FirstOrDefault();
    if (!PodePerfis.CanList("Usuarios"))
    {
        return StatusCode(StatusCodes.Status403Forbidden);
    }

    ViewBag.CanViewUsers = PodePerfis.CanViewDetails("Usuarios");
    ViewBag.CanCreateUsers = PodePerfis.CanCreate("Usuarios");
    ViewBag.CanUpdateUsers = PodePerfis.CanUpdate("Usuarios");
    ViewBag.CanDeleteUsers = PodePerfis.CanDelete("Usuarios");
    ViewBag.CanManageIdentity = CanManageIdentity(PodePerfis);
    ViewBag.LoggedUserId = PodePerfis!.IdUsuario;

    var users = await _context.Usuarios
        .AsNoTracking()
        .Include(user => user.Perfil)
        .OrderByDescending(user => user.Ativo == "S")
        .ThenBy(user => user.Nome)
        .ToListAsync();
    
    ViewData["DisableMainScroll"] = "true"; 
    
    return View(users); 
}

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");
        if (id == null)
            return NotFound();

        var loggedUser = await GetLoggedUserWithPermissionsAsync();
        if (!loggedUser.CanViewDetails("Usuarios"))
            return StatusCode(StatusCodes.Status403Forbidden);

        var user = await _context.Usuarios
            .AsNoTracking()
            .Include(item => item.Perfil)
            .FirstOrDefaultAsync(item => item.IdUsuario == id.Value);
        if (user == null)
            return NotFound();

        ViewBag.CanUpdateUsers = loggedUser.CanUpdate("Usuarios");
        ViewBag.CanDeleteUsers = loggedUser.CanDelete("Usuarios");
        ViewBag.CanManageIdentity = CanManageIdentity(loggedUser);
        ViewBag.IsCurrentUser = loggedUser!.IdUsuario == user.IdUsuario;
        ViewBag.IsLastActiveAdmin = IsAdminProfile(user.Perfil)
            && user.Ativo == "S"
            && await ActiveAdminCountAsync() <= 1;
        return View(user);
    }

    // GET: /Actor/Create
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Usuarios")).FirstOrDefault();
        if (!PodePerfis.CanCreate("Usuarios"))
            return RedirectToAction("Index", "Users");
        
        ViewBag.PerfilLista = new SelectList(
            await _context.Perfis.OrderBy(a => a.Nome).ToListAsync(),
            "IdPerfil",
            "Nome"
        );
        return View(new Usuario
        {
            Ativo = "S",
            FkIdPerfil = PodePerfis!.FkIdPerfil,
            DtNascimento = DateTime.Today.AddYears(-18)
        });
    }

    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Usuarios")).FirstOrDefault();
        if (!PodePerfis.CanCreate("Usuarios"))
            return RedirectToAction("Index", "Users");

        if (!CanManageIdentity(PodePerfis) && usuario.FkIdPerfil != PodePerfis!.FkIdPerfil)
            return StatusCode(StatusCodes.Status403Forbidden);

        NormalizeEditableFields(usuario);
        var validationError = await ValidateEditableFieldsAsync(usuario, requirePassword: true);
        if (validationError != null)
        {
            ViewBag.ErrorMessage = validationError;
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        var password = usuario.Senha;
        var email = usuario.Email;
        if (await EmailExistsAsync(email))
        {
            ViewBag.ErrorMessage = "Email já cadastrado.";
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        usuario.Email = email;
        usuario.UserName = email;
        usuario.EmailConfirmed = true;

        usuario.DtCriacao = DateTime.Now;
        usuario.DtAtualizacao = DateTime.Now;
        usuario.Ativo = "S";
        usuario.IdiomaPreferido = IdiomaPreferido.Default;
        usuario.LockoutEnabled = true;

        IdentityResult result;
        if (_userManager is not null)
        {
            result = await _userManager.CreateAsync(usuario, password);
        }
        else
        {
            // Caminho exclusivo dos testes unitários que instanciam o controller
            // diretamente, sem o contêiner HTTP/Identity.
            var normalized = email.ToUpperInvariant();
            usuario.NormalizedEmail = normalized;
            usuario.NormalizedUserName = normalized;
            usuario.Senha = new PasswordHasher<Usuario>().HashPassword(usuario, password);
            _context.Add(usuario);
            await _context.SaveChangesAsync();
            result = IdentityResult.Success;
        }

        if (!result.Succeeded)
        {
            ViewBag.ErrorMessage = string.Join(" ", result.Errors.Select(error => error.Description));
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        return RedirectToAction(nameof(Index));
    }


    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
            return NotFound();

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Usuarios")).FirstOrDefault();
        if (!PodePerfis.CanUpdate("Usuarios"))
            return RedirectToAction("Index", "Users");

        ViewBag.PerfilLista = new SelectList(
            await _context.Perfis.OrderBy(a => a.Nome).ToListAsync(),
            "IdPerfil",
            "Nome"
        );
        ViewBag.CanManageIdentity = CanManageIdentity(PodePerfis);
        ViewBag.CanDeleteUsers = PodePerfis.CanDelete("Usuarios");
        ViewBag.IsCurrentUser = PodePerfis!.IdUsuario == id.Value;

        var usuario = await _context.Usuarios.FindAsync(id);


        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Senha = "";

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Usuario usuario)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Usuarios")).FirstOrDefault();
        if (!PodePerfis.CanUpdate("Usuarios"))
            return RedirectToAction("Index", "Users");

        if (!CanManageIdentity(PodePerfis) && usuario.FkIdPerfil != PodePerfis!.FkIdPerfil)
            return StatusCode(StatusCodes.Status403Forbidden);

        var loggedUserId = PodePerfis!.IdUsuario;
        ViewBag.CanManageIdentity = CanManageIdentity(PodePerfis);
        ViewBag.CanDeleteUsers = PodePerfis.CanDelete("Usuarios");
        ViewBag.IsCurrentUser = loggedUserId == id;
        var usuariobd = await _context.Usuarios
            .Include(item => item.Perfil)
            .FirstOrDefaultAsync(item => item.IdUsuario == id);
        if (usuariobd == null) return NotFound();

        if (usuario.IdUsuario != 0 && usuario.IdUsuario != id)
            return NotFound();

        NormalizeEditableFields(usuario);
        var validationError = await ValidateEditableFieldsAsync(usuario, requirePassword: false);
        if (validationError != null)
        {
            ViewBag.ErrorMessage = validationError;
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        if (!CanManageIdentity(PodePerfis) &&
            (usuario.FkIdPerfil != usuariobd.FkIdPerfil ||
             usuario.Ativo != usuariobd.Ativo ||
             !string.IsNullOrWhiteSpace(usuario.Senha)))
            return StatusCode(StatusCodes.Status403Forbidden);

        if (id == loggedUserId &&
            (usuario.FkIdPerfil != usuariobd.FkIdPerfil || usuario.Ativo != "S"))
            return BadRequest("O administrador conectado não pode alterar o próprio perfil nem desativar a própria conta.");

        if (IsAdminProfile(usuariobd.Perfil) &&
            (usuario.FkIdPerfil != usuariobd.FkIdPerfil || usuario.Ativo != "S") &&
            await ActiveAdminCountAsync() <= 1)
            return BadRequest("O último administrador ativo não pode ser rebaixado ou desativado.");

        var password = usuario.Senha;
        var email = usuario.Email;
        if (await EmailExistsAsync(email, id))
        {
            ViewBag.ErrorMessage = "Email já cadastrado.";
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        usuariobd.Nome = usuario.Nome;
        usuariobd.Email = email;
        usuariobd.UserName = email;
        usuariobd.FkIdPerfil = usuario.FkIdPerfil;
        usuariobd.Ativo = usuario.Ativo;
        usuariobd.Ocupacao = usuario.Ocupacao;
        usuariobd.Genero = usuario.Genero;
        usuariobd.DtNascimento = usuario.DtNascimento;
        usuariobd.DtAtualizacao = DateTime.Now;

        IdentityResult result;
        if (_userManager is not null)
        {
            result = await _userManager.UpdateAsync(usuariobd);
            if (result.Succeeded && !string.IsNullOrWhiteSpace(password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(usuariobd);
                result = await _userManager.ResetPasswordAsync(usuariobd, resetToken, password);
            }
            else if (result.Succeeded)
            {
                result = await _userManager.UpdateSecurityStampAsync(usuariobd);
            }
        }
        else
        {
            var normalized = email.ToUpperInvariant();
            usuariobd.NormalizedEmail = normalized;
            usuariobd.NormalizedUserName = normalized;
            if (!string.IsNullOrWhiteSpace(password))
                usuariobd.Senha = new PasswordHasher<Usuario>().HashPassword(usuariobd, password);
            usuariobd.SecurityStamp = Guid.NewGuid().ToString("N");
            await _context.SaveChangesAsync();
            result = IdentityResult.Success;
        }

        if (!result.Succeeded)
        {
            ViewBag.ErrorMessage = string.Join(" ", result.Errors.Select(error => error.Description));
            await LoadProfilesAsync(usuario.FkIdPerfil);
            return View(usuario);
        }

        return RedirectToAction("index", "Users");
    }


    [HttpGet, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmation(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");
        if (id == null)
            return NotFound();

        var loggedUser = await GetLoggedUserWithPermissionsAsync();
        if (!loggedUser.CanDelete("Usuarios"))
            return RedirectToAction(nameof(Index));

        var user = await _context.Usuarios
            .AsNoTracking()
            .Include(item => item.Perfil)
            .FirstOrDefaultAsync(item => item.IdUsuario == id.Value);
        if (user == null)
            return NotFound();

        ViewBag.IsCurrentUser = user.IdUsuario == loggedUser!.IdUsuario;
        ViewBag.IsLastActiveAdmin = IsAdminProfile(user.Perfil)
            && user.Ativo == "S"
            && await ActiveAdminCountAsync() <= 1;
        return View(user);
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
        if (!loggedUser.CanDelete("Usuarios"))
            return RedirectToAction("Index", "Users");

        if (id.Value == loggedUserId)
            return BadRequest("O usuário conectado não pode desativar a própria conta.");

        var user = await _context.Usuarios
            .Include(item => item.Perfil)
            .FirstOrDefaultAsync(item => item.IdUsuario == id.Value);
        if (user == null)
            return NotFound();

        if (IsAdminProfile(user.Perfil) && user.Ativo == "S" && await ActiveAdminCountAsync() <= 1)
            return BadRequest("O último administrador ativo não pode ser desativado.");

        user.Ativo = "N";
        user.DtAtualizacao = DateTime.Now;
        if (_userManager is not null)
        {
            var result = await _userManager.UpdateSecurityStampAsync(user);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status409Conflict, string.Join(" ", result.Errors.Select(error => error.Description)));
        }
        else
        {
            user.SecurityStamp = Guid.NewGuid().ToString("N");
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var loggedUser = await GetLoggedUserWithPermissionsAsync();
        if (!loggedUser.CanUpdate("Usuarios") || !CanManageIdentity(loggedUser))
            return StatusCode(StatusCodes.Status403Forbidden);

        var user = await _context.Usuarios.FindAsync(id);
        if (user == null)
            return NotFound();

        if (user.Ativo != "S")
        {
            user.Ativo = "S";
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.DtAtualizacao = DateTime.Now;

            if (_userManager is not null)
            {
                var result = await _userManager.UpdateSecurityStampAsync(user);
                if (!result.Succeeded)
                    return StatusCode(
                        StatusCodes.Status409Conflict,
                        string.Join(" ", result.Errors.Select(error => error.Description)));
            }
            else
            {
                user.SecurityStamp = Guid.NewGuid().ToString("N");
                await _context.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = $"Usuário {user.Nome} reativado com sucesso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Usuario?> GetLoggedUserWithPermissionsAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("ID"), out var loggedUserId))
            return null;

        return await _context.Usuarios
            .Include(user => user.Perfil)
            .ThenInclude(profile => profile.Permissoes)
            .FirstOrDefaultAsync(user => user.IdUsuario == loggedUserId);
    }

    private async Task<string?> ValidateEditableFieldsAsync(Usuario user, bool requirePassword)
    {
        if (string.IsNullOrWhiteSpace(user.Nome))
            return "Informe o nome do usuário.";
        if (string.IsNullOrWhiteSpace(user.Email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(user.Email))
            return "Informe um e-mail válido.";
        if (string.IsNullOrWhiteSpace(user.Ocupacao))
            return "Informe a ocupação do usuário.";
        if (user.DtNascimento.Date < new DateTime(1900, 1, 1) || user.DtNascimento.Date > DateTime.Today)
            return "Informe uma data de nascimento válida.";
        if (requirePassword && string.IsNullOrWhiteSpace(user.Senha))
            return "Informe uma senha.";
        if (!await _context.Perfis.AnyAsync(profile => profile.IdPerfil == user.FkIdPerfil))
            return "Selecione um perfil de acesso válido.";
        return null;
    }

    private static void NormalizeEditableFields(Usuario user)
    {
        user.Nome = user.Nome?.Trim() ?? string.Empty;
        user.Email = user.Email?.Trim() ?? string.Empty;
        user.Ocupacao = user.Ocupacao?.Trim() ?? string.Empty;
        user.Senha ??= string.Empty;
        user.Ativo = user.Ativo == "N" ? "N" : "S";
    }

    private async Task LoadProfilesAsync(int? selectedProfile = null)
    {
        ViewBag.PerfilLista = new SelectList(
            await _context.Perfis.OrderBy(profile => profile.Nome).ToListAsync(),
            "IdPerfil",
            "Nome",
            selectedProfile);
    }

    private Task<int> ActiveAdminCountAsync() =>
        _context.Usuarios.CountAsync(user => user.Ativo == "S" && user.Perfil.Nome == "Admin");

    private Task<bool> EmailExistsAsync(string email, int? exceptUserId = null)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _context.Usuarios.AnyAsync(user =>
            user.NormalizedEmail == normalized &&
            (!exceptUserId.HasValue || user.IdUsuario != exceptUserId.Value));
    }

    private static bool CanManageIdentity(Usuario? user) =>
        user?.Ativo == "S" && IsAdminProfile(user.Perfil);

    private static bool IsAdminProfile(Perfil? profile) =>
        string.Equals(profile?.Nome, "Admin", StringComparison.OrdinalIgnoreCase);

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
