using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Controllers;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class UsersAndProfilesCrudTests : ControllerTestBase
{
    [Fact]
    public async Task Users_Create_PersistsAndHashesPassword()
    {
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));
        var user = NewUser("novo@test.local");

        var result = await controller.Create(user);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Usuarios.SingleAsync(item => item.Email == user.Email);
        Assert.Equal("S", saved.Ativo);
        Assert.NotEqual("Senha@123", saved.Senha);
        Assert.Equal(PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(saved, saved.Senha, "Senha@123"));
    }

    [Fact]
    public async Task Users_Create_DuplicateEmailDoesNotPersist()
    {
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));
        var before = await Db.Usuarios.CountAsync();

        var result = await controller.Create(NewUser("joao@email.com"));

        Assert.IsType<ViewResult>(result);
        Assert.Equal(before, await Db.Usuarios.CountAsync());
        Assert.Equal("Email já cadastrado.", controller.ViewBag.ErrorMessage);
    }

    [Fact]
    public async Task Users_Edit_UpdatesFieldsAndPreservesPasswordWhenBlank()
    {
        var original = await Db.Usuarios.AsNoTracking().SingleAsync(item => item.IdUsuario == 2);
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));
        var update = NewUser("editado@test.local");
        update.IdUsuario = original.IdUsuario;
        update.Nome = "Nome editado";
        update.Senha = "";
        update.Ativo = "N";

        var result = await controller.Edit(original.IdUsuario, update);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Usuarios.AsNoTracking().SingleAsync(item => item.IdUsuario == original.IdUsuario);
        Assert.Equal("Nome editado", saved.Nome);
        Assert.Equal("editado@test.local", saved.Email);
        Assert.Equal("N", saved.Ativo);
        Assert.Equal(original.Senha, saved.Senha);
    }

    [Fact]
    public async Task Users_Edit_RejectsDuplicateEmail()
    {
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));
        var user = NewUser("joao@email.com");
        user.IdUsuario = 2;

        var result = await controller.Edit(2, user);

        Assert.IsType<ViewResult>(result);
        Assert.Equal("Email já cadastrado.", controller.ViewBag.ErrorMessage);
    }

    [Fact]
    public async Task Users_Edit_MissingRecordReturnsNotFound()
    {
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));
        var result = await controller.Edit(999_999, NewUser("missing@test.local"));
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Users_Delete_PerformsSoftDelete()
    {
        var user = NewUser("delete@test.local");
        user.Senha = new PasswordHasher<Usuario>().HashPassword(user, user.Senha);
        Db.Usuarios.Add(user);
        await Db.SaveChangesAsync();
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db));

        var result = await controller.Delete(user.IdUsuario);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("N", (await Db.Usuarios.FindAsync(user.IdUsuario))!.Ativo);
    }

    [Fact]
    public async Task Profiles_Create_PersistsAllModulesAndCheckboxPermissions()
    {
        var values = new Dictionary<string, StringValues>
        {
            ["Permissoes[Usuarios][Listar]"] = "on",
            ["Permissoes[Usuarios][Criar]"] = "on"
        };
        var controller = Attach(new AccessProfileController(Db), form: new FormCollection(values));

        var result = await controller.Create(new Perfil { Nome = "  Perfil de teste  " });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Perfis.Include(item => item.Permissoes)
            .SingleAsync(item => item.Nome == "Perfil de teste");
        Assert.Equal(12, saved.Permissoes.Count);
        var users = saved.Permissoes.Single(item => item.Modulo == "Usuarios");
        Assert.Equal("S", users.PodeListar);
        Assert.Equal("S", users.PodeCriar);
        Assert.Equal("N", users.PodeDeletar);
    }

    [Fact]
    public async Task Profiles_Create_InvalidModelDoesNotPersist()
    {
        var controller = Attach(new AccessProfileController(Db), form: new FormCollection(new Dictionary<string, StringValues>()));
        controller.ModelState.AddModelError("Nome", "Obrigatório");
        var before = await Db.Perfis.CountAsync();

        var result = await controller.Create(new Perfil { Nome = "" });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(before, await Db.Perfis.CountAsync());
    }

    [Fact]
    public async Task Profiles_Edit_UpdatesNameAndPermissions()
    {
        var profile = await Db.Perfis.Include(item => item.Permissoes).SingleAsync(item => item.IdPerfil == 2);
        var permission = profile.Permissoes.First();
        var controller = Attach(new AccessProfileController(Db));
        var update = new Perfil
        {
            IdPerfil = profile.IdPerfil,
            Nome = "Editor revisado",
            Permissoes = new List<Permissoes>
            {
                new() { IdPermissoes = permission.IdPermissoes, PodeListar = "N", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "S", PodeDeletar = "N", Modulo = permission.Modulo }
            }
        };

        var result = await controller.Edit(profile.IdPerfil, update);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Perfis.Include(item => item.Permissoes).SingleAsync(item => item.IdPerfil == profile.IdPerfil);
        Assert.Equal("Editor revisado", saved.Nome);
        Assert.Equal("N", saved.Permissoes.Single(item => item.IdPermissoes == permission.IdPermissoes).PodeListar);
    }

    [Fact]
    public async Task Profiles_Edit_IdMismatchReturnsNotFound()
    {
        var controller = Attach(new AccessProfileController(Db));
        var result = await controller.Edit(1, new Perfil { IdPerfil = 2, Nome = "Inconsistente" });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CrudEndpoints_AnonymousUserIsRedirectedToLogin()
    {
        var users = Attach(new UsersController(NullLogger<UsersController>.Instance, Db), userId: null);
        var profiles = Attach(new AccessProfileController(Db), userId: null);

        AssertLoginRedirect(await users.Create(NewUser("anonymous@test.local")));
        AssertLoginRedirect(await profiles.Edit(1, new Perfil { IdPerfil = 1, Nome = "Sem sessão" }));
    }

    private static Usuario NewUser(string email) => new()
    {
        Nome = "Usuário de teste",
        Senha = "Senha@123",
        Email = email,
        Ocupacao = "Analista",
        Genero = 1,
        DtNascimento = new DateTime(1990, 1, 1),
        FkIdPerfil = 1,
        Ativo = "S"
    };

    private static void AssertLoginRedirect(IActionResult result)
    {
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }
}
