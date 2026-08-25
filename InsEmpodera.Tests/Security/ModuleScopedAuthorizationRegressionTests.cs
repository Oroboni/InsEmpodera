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

namespace InsEmpodera.Tests.Security;

public sealed class ModuleScopedAuthorizationRegressionTests : ControllerTestBase
{
    [Fact]
    public async Task PermissionGrantedForTargetModule_IsNotCancelledByUnrelatedModuleDenial()
    {
        var userId = await CreateUserWithPermissionsAsync(
            Allow("Usuarios"), Allow("Perfis"), Allow("Comunidades"), Allow("Atores"),
            Allow("Atividades"), Allow("DiariosCampo"), Allow("Ficha1Contato"),
            Allow("AvaliacoesPessoais"), Deny("SER"));

        Assert.IsType<ViewResult>(await Attach(
            new UsersController(NullLogger<UsersController>.Instance, Db), userId).Create());
        Assert.IsType<ViewResult>(Attach(new AccessProfileController(Db), userId).Create());
        Assert.IsType<ViewResult>(Attach(
            new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db), userId).ComunidadesDetalhes(0));
        Assert.IsType<ViewResult>(await Attach(
            new AtoresController(NullLogger<AtoresController>.Instance, Db), userId).Create());
        Assert.IsType<ViewResult>(await Attach(new AtividadesController(Db), userId).Create());
        Assert.IsType<ViewResult>(await Attach(new DiarioCampoController(Db), userId).Create());
        Assert.IsType<ViewResult>(Attach(new FichaPrimeiroContatoController(Db), userId).Create());
        Assert.IsType<ViewResult>(await Attach(new PersonalAssessmentController(Db), userId).Create(null));
    }

    [Fact]
    public async Task TargetModuleGrant_AllowsReadUpdateAndDeleteDespiteUnrelatedDenial()
    {
        var userId = await CreateUserWithPermissionsAsync(Allow("Atores"), Deny("SER"));
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db), userId);

        Assert.IsType<ViewResult>(controller.Index());
        Assert.IsType<ViewResult>(await controller.Edit(actor.IdAtores));
        Assert.IsType<RedirectToActionResult>(await controller.Delete(actor.IdAtores));
        Assert.Equal("N", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
    }

    [Fact]
    public async Task MissingTargetModule_DeniesReadCreateAndDeleteWithoutMutation()
    {
        var userId = await CreateUserWithPermissionsAsync(Allow("SER"));
        var actor = await CreateActorAsync();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db), userId);

        AssertForbidden(controller.Index());
        Assert.IsType<RedirectToActionResult>(await controller.Create());
        Assert.IsType<RedirectToActionResult>(await controller.Delete(actor.IdAtores));
        Assert.Equal("S", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
    }

    [Fact]
    public async Task ConflictingDuplicateModulePermissions_AreDeniedConservatively()
    {
        var userId = await CreateUserWithPermissionsAsync(Allow("Atores"), Deny("Atores"));
        var actor = await CreateActorAsync();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db), userId);

        AssertForbidden(controller.Index());
        Assert.IsType<RedirectToActionResult>(await controller.Create());
        Assert.IsType<RedirectToActionResult>(await controller.Delete(actor.IdAtores));
        Assert.Equal("S", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
    }

    [Fact]
    public async Task UsersAndProfiles_AreAuthorizedByTheirOwnIndependentModules()
    {
        var usersOnlyId = await CreateUserWithPermissionsAsync(Allow("Usuarios"), Deny("Perfis"));
        var profilesOnlyId = await CreateUserWithPermissionsAsync(Deny("Usuarios"), Allow("Perfis"));

        Assert.IsType<ViewResult>(await Attach(
            new UsersController(NullLogger<UsersController>.Instance, Db), usersOnlyId).Create());
        Assert.IsType<RedirectToActionResult>(Attach(new AccessProfileController(Db), usersOnlyId).Create());

        Assert.IsType<RedirectToActionResult>(await Attach(
            new UsersController(NullLogger<UsersController>.Instance, Db), profilesOnlyId).Create());
        Assert.IsType<ViewResult>(Attach(new AccessProfileController(Db), profilesOnlyId).Create());
    }

    [Fact]
    public async Task PersonalProcessAndAssessment_AreAuthorizedByTheirOwnIndependentModules()
    {
        var processOnlyId = await CreateUserWithPermissionsAsync(
            Allow("DiariosProcessoPessoal"), Deny("AvaliacoesPessoais"));
        var assessmentOnlyId = await CreateUserWithPermissionsAsync(
            Deny("DiariosProcessoPessoal"), Allow("AvaliacoesPessoais"));

        Assert.IsType<ViewResult>(await Attach(new PersonalProcessController(Db), processOnlyId).Index(null, ""));
        AssertForbidden(await Attach(new PersonalAssessmentController(Db), processOnlyId).Index(null));

        AssertForbidden(await Attach(new PersonalProcessController(Db), assessmentOnlyId).Index(null, ""));
        Assert.IsType<ViewResult>(await Attach(new PersonalAssessmentController(Db), assessmentOnlyId).Index(null));
    }

    [Fact]
    public async Task CommunityDetails_UsesCreateForZeroAndUpdateForExistingId()
    {
        var community = await CreateCommunityAsync();
        var creatorId = await CreateUserWithPermissionsAsync(Permission(
            "Comunidades", list: "S", details: "S", create: "S", update: "N", delete: "N"));
        var editorId = await CreateUserWithPermissionsAsync(Permission(
            "Comunidades", list: "S", details: "S", create: "N", update: "S", delete: "N"));

        var creator = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db), creatorId);
        Assert.IsType<ViewResult>(creator.ComunidadesDetalhes(0));
        Assert.IsType<RedirectToActionResult>(creator.ComunidadesDetalhes(community.Id_Comunidade));

        var editor = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db), editorId);
        Assert.IsType<RedirectToActionResult>(editor.ComunidadesDetalhes(0));
        Assert.IsType<ViewResult>(editor.ComunidadesDetalhes(community.Id_Comunidade));
    }

    [Fact]
    public async Task ProfileCreation_PersistsCanonicalPermissionModuleNames()
    {
        var formValues = new Dictionary<string, StringValues>
        {
            ["Permissoes[Ficha1Contato][Listar]"] = "on",
            ["Permissoes[DiariosProcessoPessoal][Criar]"] = "on",
            ["Permissoes[AvaliacoesPessoais][Atualizar]"] = "on"
        };
        var controller = Attach(
            new AccessProfileController(Db),
            form: new FormCollection(formValues));

        var result = await controller.Create(new Perfil { Nome = $"Canônico {Guid.NewGuid():N}" });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Perfis.Include(profile => profile.Permissoes)
            .SingleAsync(profile => profile.Nome.StartsWith("Canônico"));
        var modules = saved.Permissoes.Select(permission => permission.Modulo).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("Ficha1Contato", modules);
        Assert.Contains("DiariosProcessoPessoal", modules);
        Assert.Contains("AvaliacoesPessoais", modules);
        Assert.DoesNotContain("FichaContato", modules);
        Assert.DoesNotContain("DiarioPessoal", modules);
        Assert.DoesNotContain("Avaliacoes", modules);
    }
    [Fact]
    public void NullInvalidOrDuplicatePermissionValues_NeverGrantAccess()
    {
        var user = new Usuario
        {
            Perfil = new Perfil
            {
                Permissoes = new List<Permissoes>
                {
                    Permission("Atores", list: null!, details: "?", create: "", update: "N", delete: "n")
                }
            }
        };

        Assert.False(user.CanList("Atores"));
        Assert.False(user.CanViewDetails("Atores"));
        Assert.False(user.CanCreate("Atores"));
        Assert.False(user.CanUpdate("Atores"));
        Assert.False(user.CanDelete("Atores"));
        Assert.False(user.CanList("ModuloAusente"));
    }

    private async Task<int> CreateUserWithPermissionsAsync(params Permissoes[] permissions)
    {
        var profile = new Perfil
        {
            Nome = $"Perfil misto {Guid.NewGuid():N}",
            FkIdUsuario = 1,
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow,
            Permissoes = permissions.ToList()
        };
        Db.Perfis.Add(profile);
        await Db.SaveChangesAsync();

        var user = new Usuario
        {
            Nome = "Usuário com permissões específicas",
            Email = $"permission-{Guid.NewGuid():N}@test.local",
            Senha = "Permissao@123",
            Ocupacao = "Teste",
            DtNascimento = new DateTime(1990, 1, 1),
            DtCriacao = DateTime.UtcNow,
            DtAtualizacao = DateTime.UtcNow,
            FkIdPerfil = profile.IdPerfil,
            Ativo = "S"
        };
        user.Senha = new PasswordHasher<Usuario>().HashPassword(user, user.Senha);
        Db.Usuarios.Add(user);
        await Db.SaveChangesAsync();
        return user.IdUsuario;
    }

    private static Permissoes Allow(string module) => Permission(module, "S", "S", "S", "S", "S");
    private static Permissoes Deny(string module) => Permission(module, "N", "N", "N", "N", "N");

    private static void AssertForbidden(IActionResult result)
    {
        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, status.StatusCode);
    }

    private static Permissoes Permission(
        string module,
        string list,
        string details,
        string create,
        string update,
        string delete) => new()
    {
        Modulo = module,
        PodeListar = list,
        PodeDetalhar = details,
        PodeCriar = create,
        PodeAtualizar = update,
        PodeDeletar = delete
    };
}
