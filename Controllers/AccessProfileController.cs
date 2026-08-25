using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class AccessProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccessProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /AccessProfile/
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodePerfis = _context.Usuarios
            .Include(c => c.Perfil)
            .ThenInclude(p => p.Permissoes)
            .Where(u =>
                u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") &&
                u.Perfil.Permissoes.Any(p => p.Modulo == "Perfis"))
            .FirstOrDefault();

        if (!PodePerfis.CanList("Perfis"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        ViewBag.CanDeleteProfiles = PodePerfis.CanDelete("Perfis");
        ViewBag.LoggedProfileId = PodePerfis!.FkIdPerfil;
        ViewData["DisableMainScroll"] = "true";

        var perfis = await _context.Perfis
            .Include(p => p.Usuarios) 
            .OrderBy(p => p.Nome)
            .ToListAsync();

        ViewBag.TotalUsuarios = perfis.Sum(p => p.Usuarios.Count);

        ViewBag.PerfisAtivos = perfis.Count(p =>
        p.Usuarios.Any(u => u.Ativo == "S")
    );

        return View(perfis);
    }

    // GET: /AccessProfile/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Perfis")).FirstOrDefault();
        if (!PodePerfis.CanCreate("Perfis"))
        {
            return RedirectToAction("Index", "AccessProfile");
        }

        var novoPerfil = new Perfil
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(novoPerfil);
    }

    // POST: /AccessProfile/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Perfil model)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Perfis")).FirstOrDefault();
        if (!PodePerfis.CanCreate("Perfis"))
        {
            return RedirectToAction("Index", "AccessProfile");
        }

        var novoPerfil = new Perfil
        {
            Nome = model.Nome.Trim(),
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now,
            Permissoes = new List<Permissoes>()
        };

        // Lista de módulos definidos na view
        var modulos = new[]
        {
            "Usuarios","Perfis","Atividades","Comunidades","Vulnerabilidades",
            "Recursos","DiariosCampo","Atores","Ficha1Contato","DiariosProcessoPessoal",
            "AvaliacoesPessoais","SER"
        };

        // Ler dados do form manualmente (os checkboxes nomeados em Permissoes[Modulo][Permissao])
        foreach (var modulo in modulos)
        {
            var permissao = new Permissoes
            {
                Modulo = modulo,
                PodeListar = Request.Form[$"Permissoes[{modulo}][Listar]"].Count > 0 ? "S" : "N",
                PodeDetalhar = Request.Form[$"Permissoes[{modulo}][Detalhar]"].Count > 0 ? "S" : "N",
                PodeCriar = Request.Form[$"Permissoes[{modulo}][Criar]"].Count > 0 ? "S" : "N",
                PodeAtualizar = Request.Form[$"Permissoes[{modulo}][Atualizar]"].Count > 0 ? "S" : "N",
                PodeDeletar = Request.Form[$"Permissoes[{modulo}][Deletar]"].Count > 0 ? "S" : "N"
            };

            novoPerfil.Permissoes.Add(permissao);
        }

        _context.Perfis.Add(novoPerfil);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /AccessProfile/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Perfis")).FirstOrDefault();
        if (!PodePerfis.CanUpdate("Perfis"))
        {
            return RedirectToAction("Index", "AccessProfile");
        }

        if (id == null)
            return NotFound();

        var perfil = await _context.Perfis
            .Include(p => p.Permissoes)
            .FirstOrDefaultAsync(p => p.IdPerfil == id);

        if (perfil == null)
            return NotFound();

        return View(perfil);
    }

    // POST: /AccessProfile/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Perfil perfilAtualizado)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodePerfis = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Perfis")).FirstOrDefault();
        if (!PodePerfis.CanUpdate("Perfis"))
        {
            return RedirectToAction("Index", "AccessProfile");
        }

        if (id != perfilAtualizado.IdPerfil)
            return NotFound();

        var perfilDb = await _context.Perfis
            .Include(p => p.Permissoes)
            .FirstOrDefaultAsync(p => p.IdPerfil == id);

        if (perfilDb == null)
            return NotFound();

        perfilDb.Nome = perfilAtualizado.Nome;
        perfilDb.DtModificacao = DateTime.Now;

        if (perfilAtualizado.Permissoes != null)
        {
            foreach (var permissaoForm in perfilAtualizado.Permissoes)
            {
                var permissaoDb = perfilDb.Permissoes
                    .FirstOrDefault(p => p.IdPermissoes == permissaoForm.IdPermissoes);

                if (permissaoDb != null)
                {
                    permissaoDb.PodeListar = permissaoForm.PodeListar == "S" ? "S" : "N";
                    permissaoDb.PodeDetalhar = permissaoForm.PodeDetalhar == "S" ? "S" : "N";
                    permissaoDb.PodeCriar = permissaoForm.PodeCriar == "S" ? "S" : "N";
                    permissaoDb.PodeAtualizar = permissaoForm.PodeAtualizar == "S" ? "S" : "N";
                    permissaoDb.PodeDeletar = permissaoForm.PodeDeletar == "S" ? "S" : "N";
                }
            }
        }

        try
        {
            _context.Update(perfilDb);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Perfis.Any(p => p.IdPerfil == perfilAtualizado.IdPerfil))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction("Index", "AccessProfile");
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
        if (!loggedUser.CanDelete("Perfis"))
            return RedirectToAction(nameof(Index));

        var profile = await _context.Perfis
            .Include(item => item.Usuarios)
            .Include(item => item.Permissoes)
            .FirstOrDefaultAsync(item => item.IdPerfil == id.Value);
        if (profile == null)
            return NotFound();

        if (profile.IdPerfil == loggedUser!.FkIdPerfil)
        {
            TempData["ErrorMessage"] = "O perfil do usuário conectado não pode ser excluído.";
            return RedirectToAction(nameof(Index));
        }

        if (profile.Usuarios.Count > 0)
        {
            TempData["ErrorMessage"] = "Este perfil não pode ser excluído porque possui usuários associados, inclusive inativos.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            _context.Permissoes.RemoveRange(profile.Permissoes);
            _context.Perfis.Remove(profile);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            await transaction.CommitAsync(HttpContext.RequestAborted);
        }
        catch
        {
            await transaction.RollbackAsync(HttpContext.RequestAborted);
            throw;
        }

        return RedirectToAction(nameof(Index));
    }}
