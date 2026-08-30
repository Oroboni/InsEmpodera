using System.Net;
using System.Text.RegularExpressions;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class EndToEndCrudTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private const string Password = "Crud@123";
    private readonly EmpoderaWebApplicationFactory _factory;

    public EndToEndCrudTests(EmpoderaWebApplicationFactory factory) => _factory = factory;
    [Theory]
    [InlineData("/Users/Create")]
    [InlineData("/Comunidade/ComunidadesDetalhes")]
    [InlineData("/Atores/Create")]
    [InlineData("/Atividades/Create")]
    [InlineData("/DiarioCampo/Create")]
    [InlineData("/FichaPrimeiroContato/Create")]
    [InlineData("/PersonalAssessment/Create")]
    public async Task CrudPosts_WithoutAntiforgeryTokenAreRejected(string path)
    {
        using var client = await CreateAuthenticatedClientAsync();
        using var response = await client.PostAsync(path, new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Users_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var email = $"crud-{Guid.NewGuid():N}@test.local";

        var create = await PostFormAsync(client, "/Users/Create", "/Users/Create", new()
        {
            ["Nome"] = "Usuário HTTP",
            ["Senha"] = "Senha@123",
            ["Email"] = email,
            ["Ocupacao"] = "Tester",
            ["Genero"] = "1",
            ["DtNascimento"] = "1990-01-01",
            ["FkIdPerfil"] = "1",
            ["Ativo"] = "S"
        });
        AssertRedirect(create);

        var id = await QueryAsync(db => db.Usuarios.Where(item => item.Email == email).Select(item => item.IdUsuario).SingleAsync());
        var edit = await PostFormAsync(client, $"/Users/Edit/{id}", $"/Users/Edit/{id}", new()
        {
            ["IdUsuario"] = id.ToString(), ["Nome"] = "Usuário HTTP editado", ["Senha"] = "",
            ["Email"] = email, ["Ocupacao"] = "QA", ["Genero"] = "2",
            ["DtNascimento"] = "1990-01-01", ["FkIdPerfil"] = "1", ["Ativo"] = "S"
        });
        AssertRedirect(edit);
        Assert.Equal("Usuário HTTP editado", await QueryAsync(db => db.Usuarios.Where(item => item.IdUsuario == id).Select(item => item.Nome).SingleAsync()));

        var delete = await PostFormAsync(client, $"/Users/Edit/{id}", $"/Users/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.Equal("N", await QueryAsync(db => db.Usuarios.Where(item => item.IdUsuario == id).Select(item => item.Ativo).SingleAsync()));
    }

    [Fact]
    public async Task Communities_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var name = $"Comunidade HTTP {Guid.NewGuid():N}";

        var create = await PostFormAsync(client, "/Comunidade/ComunidadesDetalhes/0", "/Comunidade/ComunidadesDetalhes?id=0", new()
        {
            ["Id_Comunidade"] = "0", ["Nome"] = name,
            ["Local"] = "Rua HTTP, 10, São Paulo",
            ["LocalSecundario"] = "Rua HTTP Dois, 20, São Paulo",
            ["Status"] = "Em Processo", ["Ativo"] = "S"
        });
        AssertRedirect(create);
        var id = await QueryAsync(db => db.Comunidades.Where(item => item.Nome == name).Select(item => item.Id_Comunidade).SingleAsync());
        Assert.Equal("Rua HTTP Dois, 20, São Paulo", await QueryAsync(db => db.Comunidades.Where(item => item.Id_Comunidade == id).Select(item => item.LocalSecundario).SingleAsync()));

        var edit = await PostFormAsync(client, $"/Comunidade/ComunidadesDetalhes/{id}", $"/Comunidade/ComunidadesDetalhes?id={id}", new()
        {
            ["Id_Comunidade"] = id.ToString(), ["Nome"] = name + " editada",
            ["Local"] = "Avenida HTTP, 20, Recife", ["LocalSecundario"] = string.Empty,
            ["Status"] = "Em Diagnóstico", ["Ativo"] = "S"
        });
        AssertRedirect(edit);
        Assert.Equal("Em diagnóstico", await QueryAsync(db => db.Comunidades.Where(item => item.Id_Comunidade == id).Select(item => item.Status).SingleAsync()));
        Assert.Null(await QueryAsync(db => db.Comunidades.Where(item => item.Id_Comunidade == id).Select(item => item.LocalSecundario).SingleAsync()));

        var delete = await PostFormAsync(client, $"/Comunidade/ComunidadesDetalhes/{id}", $"/Comunidade/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.Equal("N", await QueryAsync(db => db.Comunidades.Where(item => item.Id_Comunidade == id).Select(item => item.Ativo).SingleAsync()));
    }

    [Fact]
    public async Task Actors_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var communityId = await SeedCommunityAsync();
        var name = $"Ator HTTP {Guid.NewGuid():N}";

        var create = await PostFormAsync(client, "/Atores/Create", "/Atores/Create", new()
        {
            ["Nome"] = name, ["Idade"] = "35", ["Genero"] = "1",
            ["Telefone"] = "11999990000", ["ComunidadeId"] = communityId.ToString(), ["Ativo"] = "S"
        });
        AssertRedirect(create);
        Assert.True(await QueryAsync(db => db.Atores.AnyAsync(item => item.Nome == name)), "Actor was not persisted. Redirect: " + create.Headers.Location);
        var id = await QueryAsync(db => db.Atores.Where(item => item.Nome == name).Select(item => item.IdAtores).SingleAsync());

        var edit = await PostFormAsync(client, $"/Atores/Edit/{id}", $"/Atores/Edit/{id}", new()
        {
            ["Nome"] = name + " editado", ["Idade"] = "36", ["Genero"] = "2",
            ["Telefone"] = "11888880000", ["ComunidadeId"] = communityId.ToString()
        });
        AssertRedirect(edit);
        Assert.Equal(name + " editado", await QueryAsync(db => db.Atores.Where(item => item.IdAtores == id).Select(item => item.Nome).SingleAsync()));

        var delete = await PostFormAsync(client, $"/Atores/Edit/{id}", $"/Atores/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.Equal("N", await QueryAsync(db => db.Atores.Where(item => item.IdAtores == id).Select(item => item.Ativo).SingleAsync()));
    }

    [Fact]
    public async Task Activities_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var communityId = await SeedCommunityAsync();
        var axisId = await SeedAxisAsync();
        var name = $"Atividade HTTP {Guid.NewGuid():N}";

        var create = await PostFormAsync(client, "/Atividades/Create", "/Atividades/Create", new()
        {
            ["Nome"] = name, ["Descricao"] = "Criada pelo pipeline HTTP",
            ["ComunidadeId"] = communityId.ToString(), ["EixosSelecionados"] = axisId.ToString()
        });
        AssertRedirect(create);
        Assert.True(await QueryAsync(db => db.Atividades.AnyAsync(item => item.Nome == name)), "Activity was not persisted. Redirect: " + create.Headers.Location);
        var id = await QueryAsync(db => db.Atividades.Where(item => item.Nome == name).Select(item => item.IdAtividade).SingleAsync());
        Assert.True(await QueryAsync(db => db.AtividadesEixo.AnyAsync(item => item.FkIdAtividade == id && item.FkIdEixo == axisId)));

        var edit = await PostFormAsync(client, $"/Atividades/Edit/{id}", $"/Atividades/Edit/{id}", new()
        {
            ["IdAtividade"] = id.ToString(), ["Nome"] = name + " editada", ["Descricao"] = "Editada",
            ["ComunidadeId"] = communityId.ToString(), ["EixosSelecionados"] = axisId.ToString()
        });
        AssertRedirect(edit);
        Assert.Equal(name + " editada", await QueryAsync(db => db.Atividades.Where(item => item.IdAtividade == id).Select(item => item.Nome).SingleAsync()));

        var delete = await PostFormAsync(client, $"/Atividades/Edit/{id}", $"/Atividades/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.False(await QueryAsync(db => db.Atividades.AnyAsync(item => item.IdAtividade == id)));
    }

    [Fact]
    public async Task FieldDiaries_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var communityId = await SeedCommunityAsync();
        var axisId = await SeedAxisAsync();
        var description = $"Diário HTTP {Guid.NewGuid():N}";
        var create = await PostFormAsync(client, "/DiarioCampo/Create", "/DiarioCampo/Create", new()
        {
            ["FkIdComunidade"] = communityId.ToString(), ["Data"] = "2026-08-23", ["Descricao"] = description,
            ["Localizacao"] = "Local HTTP", ["Foto"] = "sem-foto.png", ["EixosSelecionados"] = axisId.ToString()
        });
        AssertRedirect(create);
        var id = await QueryAsync(db => db.DiariosCampo.Where(item => item.Descricao == description).Select(item => item.IdDCampo).SingleAsync());
        var edit = await PostFormAsync(client, $"/DiarioCampo/Edit/{id}", $"/DiarioCampo/Edit/{id}", new()
        {
            ["IdDCampo"] = id.ToString(), ["FkIdComunidade"] = communityId.ToString(), ["Data"] = "2026-08-24",
            ["Descricao"] = description + " editado", ["Localizacao"] = "Local editado", ["Foto"] = "sem-foto.png",
            ["EixosSelecionados"] = axisId.ToString()
        });
        AssertRedirect(edit);
        Assert.Equal(description + " editado", await QueryAsync(db => db.DiariosCampo.Where(item => item.IdDCampo == id).Select(item => item.Descricao).SingleAsync()));
        var delete = await PostFormAsync(client, $"/DiarioCampo/Delete/{id}", $"/DiarioCampo/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.False(await QueryAsync(db => db.DiariosCampo.AnyAsync(item => item.IdDCampo == id)));
    }

    [Fact]
    public async Task PersonalAssessments_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var actorId = await SeedActorAsync();
        var create = await PostFormAsync(client, $"/PersonalAssessment/Create?atorId={actorId}", "/PersonalAssessment/Create", AssessmentFields(actorId, 2));
        AssertRedirect(create);
        var id = await QueryAsync(db => db.AvaliacaoPessoal.Where(item => item.FK_id_Atores == actorId).OrderByDescending(item => item.IdAvaliacao).Select(item => item.IdAvaliacao).FirstAsync());
        var edit = await PostFormAsync(client, $"/PersonalAssessment/Edit/{id}", $"/PersonalAssessment/Edit/{id}", AssessmentFields(actorId, 5));
        AssertRedirect(edit);
        Assert.Equal(5, await QueryAsync(db => db.AvaliacaoPessoal.Where(item => item.IdAvaliacao == id).Select(item => item.Saude).SingleAsync()));
        var delete = await PostFormAsync(client, $"/PersonalAssessment/Edit/{id}", $"/PersonalAssessment/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.False(await QueryAsync(db => db.AvaliacaoPessoal.AnyAsync(item => item.IdAvaliacao == id)));
    }

    [Fact]
    public async Task FirstContactForms_FullHttpCrudWorkflow_WorksThroughRealPipeline()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var communityId = await SeedCommunityAsync();
        var actorId = await SeedActorAsync(communityId);
        var marker = $"Contato HTTP {Guid.NewGuid():N}";
        var fields = FirstContactFields(actorId, communityId, marker);
        var create = await PostFormAsync(client, "/FichaPrimeiroContato/Create", "/FichaPrimeiroContato/Create", fields);
        AssertRedirect(create);
        var id = await QueryAsync(db => db.FichasPrimeiroContato.Where(item => item.Coment == marker).Select(item => item.IdFicha).SingleAsync());
        fields["IdFicha"] = id.ToString();
        fields["Coment"] = marker + " editado";
        var edit = await PostFormAsync(client, $"/FichaPrimeiroContato/Edit/{id}", $"/FichaPrimeiroContato/Edit/{id}", fields);
        AssertRedirect(edit);
        Assert.Equal(marker + " editado", await QueryAsync(db => db.FichasPrimeiroContato.Where(item => item.IdFicha == id).Select(item => item.Coment).SingleAsync()));
        var delete = await PostFormAsync(client, $"/FichaPrimeiroContato/Delete/{id}", $"/FichaPrimeiroContato/Delete/{id}", new());
        AssertRedirect(delete);
        Assert.False(await QueryAsync(db => db.FichasPrimeiroContato.AnyAsync(item => item.IdFicha == id)));
    }
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
        await QueryAsync(async db =>
        {
            var user = await db.Usuarios.SingleAsync(item => item.IdUsuario == 1);
            user.Senha = new PasswordHasher<Usuario>().HashPassword(user, Password);
            user.Ativo = "S";
            await db.SaveChangesAsync();
            return true;
        });

        using var login = await AuthenticationTestHelper.LoginAsync(client, "joao@email.com", Password);
        AssertRedirect(login);
        return client;
    }

    private static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string tokenPage, string postPath, Dictionary<string, string> fields)
    {
        using var page = await client.GetAsync(tokenPage);
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        Assert.True(match.Success, $"Antiforgery token was not rendered by {tokenPage}.");

        var values = fields.Select(item => new KeyValuePair<string, string>(item.Key, item.Value)).ToList();
        values.Add(new KeyValuePair<string, string>("__RequestVerificationToken", WebUtility.HtmlDecode(match.Groups[1].Value)));
        return await client.PostAsync(postPath, new FormUrlEncodedContent(values));
    }

    private async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> operation)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await operation(db);
    }

    private Task<int> SeedCommunityAsync() => QueryAsync(async db =>
    {
        var community = new Comunidade
        {
            Nome = $"Pré-requisito {Guid.NewGuid():N}", Local = "Rua de Teste, 1, São Paulo",
            LocalMapa = "Rua de Teste, 1, São Paulo", Status = "Em processo", Ativo = "S",
            FK_Id_Usuario = 1, Dt_Criacao = DateTime.UtcNow, Dt_Modificacao = DateTime.UtcNow
        };
        db.Comunidades.Add(community);
        await db.SaveChangesAsync();
        return community.Id_Comunidade;
    });

    private Task<int> SeedAxisAsync() => QueryAsync(async db =>
    {
        var axis = new Eixo { Nome = $"Eixo HTTP {Guid.NewGuid():N}" };
        db.Eixos.Add(axis);
        await db.SaveChangesAsync();
        return axis.IdEixo;
    });
    private async Task<int> SeedActorAsync(int? communityId = null)
    {
        var actualCommunityId = communityId ?? await SeedCommunityAsync();
        return await QueryAsync(async db =>
        {
            var actor = new Atores
            {
                Nome = $"Ator pré-requisito {Guid.NewGuid():N}", Ativo = "S", Idade = 30,
                FkIdUsuario = 1, DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
            };
            db.Atores.Add(actor);
            await db.SaveChangesAsync();
            db.AtorComunidades.Add(new AtorComunidade { FK_id_Atores = actor.IdAtores, FkIdComunidade = actualCommunityId });
            await db.SaveChangesAsync();
            return actor.IdAtores;
        });
    }

    private static Dictionary<string, string> AssessmentFields(int actorId, int score) => new()
    {
        ["FK_id_Atores"] = actorId.ToString(), ["CCrimes"] = score.ToString(), ["Substancias"] = score.ToString(),
        ["Moradia"] = score.ToString(), ["Prevencao"] = score.ToString(), ["AssBasica"] = score.ToString(),
        ["Educacao"] = score.ToString(), ["Saude"] = score.ToString(), ["Ocupacao"] = score.ToString(), ["Lazer"] = score.ToString()
    };

    private static Dictionary<string, string> FirstContactFields(int actorId, int communityId, string comment) => new()
    {
        ["FK_id_Atores"] = actorId.ToString(), ["FkIdComunidade"] = communityId.ToString(), ["Endereco"] = "Rua HTTP, 100",
        ["Telefone"] = "11999990000", ["DtContato"] = "2026-08-23", ["HoraContato"] = "10:30",
        ["CEstabeleceu"] = "Visita", ["NovoParceiro"] = "Sim", ["FornecidoParceiro"] = "Sim",
        ["Status"] = "EmProgresso", ["Coment"] = comment, ["SelectedCondicoes"] = "Moradia",
        ["SelectedResultados"] = "Acompanhamento"
    };
    private static void AssertRedirect(HttpResponseMessage response) =>
        Assert.True((int)response.StatusCode is >= 300 and < 400,
            $"Expected redirect, received {(int)response.StatusCode}: {response.ReasonPhrase}");
}
