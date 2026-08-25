using System.Net;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class HttpAuthorizationAndValidationFlowTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public HttpAuthorizationAndValidationFlowTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task ReadOnlyUser_CanListModulesButCannotCreateThroughHttpPipeline()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 3);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        foreach (var path in new[]
                 {
                     "/Users", "/AccessProfile", "/Comunidade", "/Atores", "/Atividades",
                     "/DiarioCampo", "/FichaPrimeiroContato", "/PersonalAssessment"
                 })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var before = await CountsAsync();
        var attempts = new[]
        {
            ("/Users/Create", UserFields($"blocked-{Guid.NewGuid():N}@test.local")),
            ("/AccessProfile/Create", new Dictionary<string, string> { ["Nome"] = "Perfil bloqueado" }),
            ("/Comunidade/ComunidadesDetalhes?id=0", new Dictionary<string, string>
            {
                ["Id_Comunidade"] = "0", ["Nome"] = "Comunidade bloqueada", ["Local"] = "Rua bloqueada",
                ["Status"] = "Em processo", ["Ativo"] = "S"
            }),
            ("/Atores/Create", new Dictionary<string, string>
            {
                ["Nome"] = "Ator bloqueado", ["Idade"] = "30", ["ComunidadeId"] = "1", ["Ativo"] = "S"
            }),
            ("/Atividades/Create", new Dictionary<string, string>
            {
                ["Nome"] = "Atividade bloqueada", ["Descricao"] = "Sem permissão", ["ComunidadeId"] = "1"
            }),
            ("/DiarioCampo/Create", new Dictionary<string, string>
            {
                ["FkIdComunidade"] = "1", ["Data"] = "2026-08-24", ["Descricao"] = "Diário bloqueado",
                ["Localizacao"] = "Local", ["Foto"] = "foto.png"
            }),
            ("/FichaPrimeiroContato/Create", new Dictionary<string, string>
            {
                ["FK_id_Atores"] = "1", ["DtContato"] = "2026-08-24", ["HoraContato"] = "10:00"
            }),
            ("/PersonalAssessment/Create", AssessmentFields(actorId: 1, score: 3))
        };

        foreach (var (path, fields) in attempts)
        {
            using var response = await HttpFlowTestSupport.PostFormAsync(client, path, fields);
            HttpFlowTestSupport.AssertRedirect(response);
        }

        Assert.Equal(before, await CountsAsync());
    }

    [Fact]
    public async Task Editor_CannotDeleteCoreRecordsThroughHttpPipeline()
    {
        var editor = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 2);
        var targets = await SeedDeletionTargetsAsync();
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, editor.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        var paths = new[]
        {
            $"/Users/Delete/{targets.UserId}",
            $"/Comunidade/Delete/{targets.CommunityId}",
            $"/Atores/Delete/{targets.ActorId}",
            $"/Atividades/Delete/{targets.ActivityId}",
            $"/DiarioCampo/Delete/{targets.DiaryId}",
            $"/PersonalAssessment/Delete/{targets.AssessmentId}",
            $"/FichaPrimeiroContato/Delete/{targets.FormId}"
        };

        foreach (var path in paths)
        {
            using var response = await HttpFlowTestSupport.PostFormAsync(client, path);
            HttpFlowTestSupport.AssertRedirect(response);
        }

        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            Assert.Equal("S", (await db.Usuarios.FindAsync(targets.UserId))!.Ativo);
            Assert.Equal("S", (await db.Comunidades.FindAsync(targets.CommunityId))!.Ativo);
            Assert.Equal("S", (await db.Atores.FindAsync(targets.ActorId))!.Ativo);
            Assert.NotNull(await db.Atividades.FindAsync(targets.ActivityId));
            Assert.NotNull(await db.DiariosCampo.FindAsync(targets.DiaryId));
            Assert.NotNull(await db.AvaliacaoPessoal.FindAsync(targets.AssessmentId));
            Assert.NotNull(await db.FichasPrimeiroContato.FindAsync(targets.FormId));
        });
    }

    [Fact]
    public async Task DuplicateUserSubmission_ReturnsFormAndPreservesExistingAccount()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");
        var originalHash = admin.Senha;
        var before = await HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.Usuarios.CountAsync());

        using var response = await HttpFlowTestSupport.PostFormAsync(client, "/Users/Create", UserFields(admin.Email));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(before, await HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.Usuarios.CountAsync()));
        Assert.Equal(originalHash, await HttpFlowTestSupport.InDatabaseAsync(
            _factory, db => db.Usuarios.Where(item => item.IdUsuario == admin.IdUsuario).Select(item => item.Senha).SingleAsync()));
    }

    [Fact]
    public async Task ActorWithUnknownCommunity_ReturnsValidationPageAndLeavesNoOrphan()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");
        var marker = $"Ator inválido {Guid.NewGuid():N}";

        using var response = await HttpFlowTestSupport.PostFormAsync(client, "/Atores/Create", new Dictionary<string, string>
        {
            ["Nome"] = marker,
            ["Idade"] = "31",
            ["Genero"] = "1",
            ["ComunidadeId"] = "999999",
            ["Ativo"] = "S"
        }, tokenPage: "/Atores/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.False(await HttpFlowTestSupport.InDatabaseAsync(
            _factory, db => db.Atores.AnyAsync(item => item.Nome == marker)));
    }

    [Fact]
    public async Task InvalidDiaryBinding_ReturnsFormAndDoesNotPersist()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync();
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");
        var before = await HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.DiariosCampo.CountAsync());

        using var response = await HttpFlowTestSupport.PostFormAsync(client, "/DiarioCampo/Create", new Dictionary<string, string>
        {
            ["FkIdComunidade"] = communityId.ToString(),
            ["Data"] = "data-inválida",
            ["Descricao"] = "",
            ["Localizacao"] = "",
            ["Foto"] = ""
        }, tokenPage: "/DiarioCampo/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.DiariosCampo.CountAsync()));
    }

    [Fact]
    public async Task TamperedActivityId_ReturnsNotFoundAndPreservesStoredEntity()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var activity = await SeedActivityAsync();
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/Atividades/Edit/{activity.IdAtividade}",
            new Dictionary<string, string>
            {
                ["IdAtividade"] = (activity.IdAtividade + 1).ToString(),
                ["Nome"] = "Nome adulterado",
                ["Descricao"] = "Não deve persistir",
                ["ComunidadeId"] = activity.FkIdComunidade.ToString()
            },
            tokenPage: $"/Atividades/Edit/{activity.IdAtividade}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(activity.Nome, await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Atividades.Where(item => item.IdAtividade == activity.IdAtividade).Select(item => item.Nome).SingleAsync()));
    }

    private async Task<CrudCounts> CountsAsync() => await HttpFlowTestSupport.InDatabaseAsync(_factory, async db => new CrudCounts(
        await db.Usuarios.CountAsync(),
        await db.Perfis.CountAsync(),
        await db.Comunidades.CountAsync(),
        await db.Atores.CountAsync(),
        await db.Atividades.CountAsync(),
        await db.DiariosCampo.CountAsync(),
        await db.FichasPrimeiroContato.CountAsync(),
        await db.AvaliacaoPessoal.CountAsync()));

    private async Task<int> SeedCommunityAsync() => await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
    {
        var community = NewCommunity();
        db.Comunidades.Add(community);
        await db.SaveChangesAsync();
        return community.Id_Comunidade;
    });

    private async Task<Atividades> SeedActivityAsync() => await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
    {
        var community = NewCommunity();
        db.Comunidades.Add(community);
        await db.SaveChangesAsync();
        var activity = new Atividades
        {
            Nome = $"Atividade protegida {Guid.NewGuid():N}",
            Descricao = "Original",
            FkIdComunidade = community.Id_Comunidade,
            FkIdUsuario = 1,
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow
        };
        db.Atividades.Add(activity);
        await db.SaveChangesAsync();
        return activity;
    });

    private async Task<DeletionTargets> SeedDeletionTargetsAsync() => await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
    {
        var community = NewCommunity();
        db.Comunidades.Add(community);
        await db.SaveChangesAsync();

        var actor = new Atores
        {
            Nome = $"Ator protegido {Guid.NewGuid():N}", Ativo = "S", Idade = 30,
            FkIdUsuario = 1, DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        db.Atores.Add(actor);
        await db.SaveChangesAsync();
        db.AtorComunidades.Add(new AtorComunidade
        {
            FK_id_Atores = actor.IdAtores, FkIdComunidade = community.Id_Comunidade
        });

        var activity = new Atividades
        {
            Nome = $"Atividade protegida {Guid.NewGuid():N}", Descricao = "Protegida",
            FkIdComunidade = community.Id_Comunidade, FkIdUsuario = 1,
            DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var diary = new DiarioCampo
        {
            FkIdComunidade = community.Id_Comunidade, FkIdUsuario = 1, Data = DateTime.UtcNow,
            Descricao = "Diário protegido", Localizacao = "Local", Foto = "foto.png",
            DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var assessment = new AvaliacaoPessoal
        {
            FK_id_Atores = actor.IdAtores, FkIdUsuario = 1,
            DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var form = new FichaPrimeiroContato
        {
            FK_id_Atores = actor.IdAtores, FkIdComunidade = community.Id_Comunidade, FkIdUsuario = 1,
            DtContato = DateTime.UtcNow, HoraContato = TimeSpan.FromHours(10),
            DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var targetUser = new Usuario
        {
            Nome = "Usuário protegido", Email = $"protected-{Guid.NewGuid():N}@test.local", Senha = "Temporária@123",
            Ocupacao = "Teste", DtNascimento = new DateTime(1990, 1, 1), DtCriacao = DateTime.UtcNow,
            DtAtualizacao = DateTime.UtcNow, FkIdPerfil = 1, Ativo = "S"
        };
        targetUser.Senha = new PasswordHasher<Usuario>().HashPassword(targetUser, targetUser.Senha);
        db.AddRange(activity, diary, assessment, form, targetUser);
        await db.SaveChangesAsync();

        return new DeletionTargets(targetUser.IdUsuario, community.Id_Comunidade, actor.IdAtores,
            activity.IdAtividade, diary.IdDCampo, assessment.IdAvaliacao, form.IdFicha);
    });

    private static Comunidade NewCommunity() => new()
    {
        Nome = $"Comunidade de fluxo {Guid.NewGuid():N}",
        Local = "Rua de teste, 10, São Paulo",
        LocalMapa = "Rua de teste, 10, São Paulo",
        Status = "Em processo",
        Ativo = "S",
        FK_Id_Usuario = 1,
        Dt_Criacao = DateTime.UtcNow,
        Dt_Modificacao = DateTime.UtcNow
    };

    private static Dictionary<string, string> UserFields(string email) => new()
    {
        ["Nome"] = "Usuário submetido",
        ["Senha"] = "Senha@123",
        ["Email"] = email,
        ["Ocupacao"] = "QA",
        ["Genero"] = "1",
        ["DtNascimento"] = "1990-01-01",
        ["FkIdPerfil"] = "1",
        ["Ativo"] = "S"
    };

    private static Dictionary<string, string> AssessmentFields(int actorId, int score) => new()
    {
        ["FK_id_Atores"] = actorId.ToString(), ["CCrimes"] = score.ToString(),
        ["Substancias"] = score.ToString(), ["Moradia"] = score.ToString(),
        ["Prevencao"] = score.ToString(), ["AssBasica"] = score.ToString(),
        ["Educacao"] = score.ToString(), ["Saude"] = score.ToString(),
        ["Ocupacao"] = score.ToString(), ["Lazer"] = score.ToString()
    };

    private sealed record CrudCounts(int Users, int Profiles, int Communities, int Actors, int Activities,
        int Diaries, int Forms, int Assessments);

    private sealed record DeletionTargets(int UserId, int CommunityId, int ActorId, int ActivityId,
        int DiaryId, int AssessmentId, int FormId);
}
