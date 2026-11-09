using System.Net;
using System.Text;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

// Pedidos de melhoria da equipe: redes do ator, recursos por tipo e relatórios (filtros, RSC, exportações).
public sealed class CommunityNetworksAndReportsHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public CommunityNetworksAndReportsHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "Ator — as sete redes são gravadas e recurso relacional entra uma vez nos recursos da comunidade")]
    public async Task Actor_NetworkFlags_PersistAndRelationalResourceIsCreatedOnce()
    {
        using var client = await LoggedClientAsync();
        var communityId = await SeedCommunityAsync();
        var name = $"Ator redes {Guid.NewGuid():N}";

        using (var create = await HttpFlowTestSupport.PostFormAsync(client, "/Comunidade/Create_Atores",
                   new Dictionary<string, string>
                   {
                       ["ComunidadeId"] = $"{communityId}",
                       ["Nome"] = name,
                       ["Parceiro"] = "true",
                       ["OperadorPar"] = "true",
                       ["RecursoRelacional"] = "true"
                   }, $"/Comunidade/Create_Atores/{communityId}"))
            Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);

        var actor = await Db(db => db.Atores.AsNoTracking().SingleAsync(a => a.Nome == name));
        Assert.True(actor.Parceiro && actor.OperadorPar && actor.RecursoRelacional);
        Assert.False(actor.DaEquipe || actor.Rope || actor.Lopiniao || actor.Mcomunidade);

        var editPage = $"/Comunidade/Edit_Atores/{actor.IdAtores}?comunidadeId={communityId}";
        using (var edit = await HttpFlowTestSupport.PostFormAsync(client, "/Comunidade/Edit_Atores",
                   new Dictionary<string, string>
                   {
                       ["IdAtores"] = $"{actor.IdAtores}",
                       ["ComunidadeId"] = $"{communityId}",
                       ["Nome"] = name,
                       ["Lopiniao"] = "true",
                       ["RecursoRelacional"] = "true"
                   }, editPage))
            Assert.Equal(HttpStatusCode.Redirect, edit.StatusCode);

        var edited = await Db(db => db.Atores.AsNoTracking().SingleAsync(a => a.IdAtores == actor.IdAtores));
        Assert.True(edited.Lopiniao && edited.RecursoRelacional);
        Assert.False(edited.Parceiro || edited.OperadorPar);

        var relational = await Db(db => db.RedeRecursos.AsNoTracking()
            .Where(r => r.FkIdComunidade == communityId && r.FK_id_Atores == actor.IdAtores).ToListAsync());
        var resource = Assert.Single(relational);
        Assert.Equal(TiposRecurso.Relacional, resource.Tipo);
        Assert.Equal(name, resource.Nome);
    }

    [Fact(DisplayName = "Recursos — tipo em lista; relacional usa um ator da comunidade; estrutural aceita nome livre sem ator")]
    public async Task Resources_TypeListAndNameRules()
    {
        using var client = await LoggedClientAsync();
        var communityId = await SeedCommunityAsync();
        var actorId = await SeedActorAsync(communityId, "Ator recurso");
        var otherCommunityActor = await SeedActorAsync(await SeedCommunityAsync(), "Ator de fora");
        var page = $"/Comunidade/Create_Recursos?comunidadeId={communityId}";

        async Task<HttpStatusCode> Post(Dictionary<string, string> fields)
        {
            fields["comunidadeId"] = $"{communityId}";
            using var response = await HttpFlowTestSupport.PostFormAsync(client, "/Comunidade/Create_Recursos", fields, page);
            return response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.BadRequest, await Post(new() { ["Tipo"] = "Internet", ["Nome"] = "Wi-Fi" }));
        Assert.Equal(HttpStatusCode.BadRequest, await Post(new() { ["Tipo"] = "Relacional" }));
        Assert.Equal(HttpStatusCode.BadRequest, await Post(new() { ["Tipo"] = "Relacional", ["FK_id_Atores"] = $"{otherCommunityActor}" }));
        Assert.Equal(HttpStatusCode.BadRequest, await Post(new() { ["Tipo"] = "Estrutural", ["Nome"] = " " }));

        Assert.Equal(HttpStatusCode.Redirect, await Post(new() { ["Tipo"] = "Estrutural", ["Nome"] = "Horta comunitária" }));
        Assert.Equal(HttpStatusCode.Redirect, await Post(new() { ["Tipo"] = "Simbólico", ["Nome"] = "Cultura carnavalesca" }));
        Assert.Equal(HttpStatusCode.Redirect, await Post(new() { ["Tipo"] = "Relacional", ["FK_id_Atores"] = $"{actorId}", ["Nome"] = "ignorado" }));

        var saved = await Db(db => db.RedeRecursos.AsNoTracking().Where(r => r.FkIdComunidade == communityId)
            .OrderBy(r => r.Id_Rede).ToListAsync());
        Assert.Collection(saved,
            r => { Assert.Equal(("Estrutural", "Horta comunitária"), (r.Tipo, r.Nome)); Assert.Null(r.FK_id_Atores); },
            r => Assert.Equal(("Simbólico", "Cultura carnavalesca"), (r.Tipo, r.Nome)),
            r => { Assert.Equal(("Relacional", "Ator recurso"), (r.Tipo, r.Nome)); Assert.Equal(actorId, r.FK_id_Atores); });

        // Recursos sem ator (estrutural/simbólico) também aparecem na lista da comunidade.
        var list = await client.GetStringAsync($"/Comunidade/ComunidadeRecursos?comunidadeId={communityId}");
        Assert.Contains("Horta comunitária", WebUtility.HtmlDecode(list));
        Assert.Contains("Cultura carnavalesca", WebUtility.HtmlDecode(list));
    }

    [Fact(DisplayName = "RSC — uma linha por vínculo da rede subjetiva, com as redes do Ator 2 e os totais")]
    public async Task Rsc_ListsSubjectiveLinksWithTotalsAndExportsExcel()
    {
        using var client = await LoggedClientAsync();
        var communityId = await SeedCommunityAsync();
        var maria = await SeedActorAsync(communityId, "Maria RSC", a => { a.Mcomunidade = true; a.RecursoRelacional = true; });
        var pepe = await SeedActorAsync(communityId, "Pepe RSC", a => { a.DaEquipe = true; a.OperadorPar = true; });
        var john = await SeedActorAsync(communityId, "John RSC", a => { a.Parceiro = true; a.Lopiniao = true; });
        await SeedLinkAsync(maria, pepe, "Amigo", TiposRede.Subjetiva);
        await SeedLinkAsync(maria, john, "Vizinho", TiposRede.Subjetiva);
        await SeedLinkAsync(john, maria, "Vizinho", TiposRede.Subjetiva);
        await SeedLinkAsync(john, pepe, "Familiar", TiposRede.Primaria);

        var html = WebUtility.HtmlDecode(await client.GetStringAsync($"/Relatorios/Rsc?comunidadeId={communityId}"));
        Assert.Matches(@"card-title"">3</span>\s*<span class=""card-subtitle"">Nós", html);
        Assert.Matches(@"card-title"">3</span>\s*<span class=""card-subtitle"">Vínculos", html);
        // Totais por rede: EQ 1, RO 1 (operador par), RLO 1, PA 1, CO 1, RRC 1.
        foreach (var rede in new[] { "EQ", "RO", "RLO", "PA", "CO", "RRC" })
            Assert.Contains($"data-rede=\"{rede}\">1<", html);

        using var excel = await client.GetAsync($"/Relatorios/Rsc?comunidadeId={communityId}&exportar=excel");
        await AssertFileAsync(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PK");
    }

    [Fact(DisplayName = "Relatórios — filtros por comunidade e eixo, várias datas da avaliação e exportações em PDF/Excel")]
    public async Task Reports_FiltersAndExports()
    {
        using var client = await LoggedClientAsync();
        var communityId = await SeedCommunityAsync();
        var otherCommunityId = await SeedCommunityAsync();
        var actorId = await SeedActorAsync(communityId, "Ator avaliado");
        await SeedActorAsync(otherCommunityId, "Ator de outra comunidade");
        var assessments = await Db(async db =>
        {
            var lista = new[] { new DateTime(2026, 8, 10), new DateTime(2026, 10, 30) }.Select(data => new AvaliacaoPessoal
            {
                FK_id_Atores = actorId, CCrimes = 2, Substancias = 3, Moradia = 2, Prevencao = 3, AssBasica = 2,
                Educacao = 3, Saude = 2, Ocupacao = 2, Lazer = 4, DtCriacao = data, FkIdUsuario = TestIds.Seed(1)
            }).ToList();
            db.AvaliacaoPessoal.AddRange(lista);
            await db.SaveChangesAsync();
            return lista.Select(a => a.IdAvaliacao).ToList();
        });

        var filtered = await client.GetStringAsync($"/Relatorios/PersonalAssessment?comunidadeId={communityId}");
        Assert.Contains("Ator avaliado", filtered);
        Assert.DoesNotContain("Ator de outra comunidade", filtered);

        var twoDates = $"/Relatorios/PersonalAssessment?comunidadeId={communityId}&atorId={actorId}"
            + $"&avaliacaoIds={assessments[0]}&avaliacaoIds={assessments[1]}";
        var chart = WebUtility.HtmlDecode(await client.GetStringAsync(twoDates));
        Assert.Contains("10/08/2026", chart);
        Assert.Contains("30/10/2026", chart);
        using (var pdf = await client.GetAsync(twoDates + "&exportar=pdf"))
            await AssertFileAsync(pdf, "application/pdf", "%PDF");
        using (var xlsx = await client.GetAsync(twoDates + "&exportar=excel"))
            await AssertFileAsync(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PK");

        var (saude, lazer) = await Db(async db => (
            await db.Eixos.Where(e => e.Nome.StartsWith("SAÚDE")).Select(e => e.IdEixo).FirstAsync(),
            await db.Eixos.Where(e => e.Nome.StartsWith("LAZER")).Select(e => e.IdEixo).FirstAsync()));
        await SeedActionAsync(communityId, "Roda de saúde", 5, saude, actorId);
        await SeedActionAsync(communityId, "Oficina de teatro", 3, lazer, actorId);

        var onlyHealth = WebUtility.HtmlDecode(await client.GetStringAsync($"/Relatorios/Actions?comunidadeId={communityId}&eixoId={saude}"));
        Assert.Contains("Roda de saúde", onlyHealth);
        Assert.DoesNotContain("Oficina de teatro", onlyHealth);
        using (var pdf = await client.GetAsync($"/Relatorios/Actions?comunidadeId={communityId}&exportar=pdf"))
            await AssertFileAsync(pdf, "application/pdf", "%PDF");
        using (var xlsx = await client.GetAsync($"/Relatorios/Actions?comunidadeId={communityId}&exportar=excel"))
            await AssertFileAsync(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PK");

        var summary = WebUtility.HtmlDecode(await client.GetStringAsync($"/Relatorios/Community?comunidadeId={communityId}"));
        Assert.Contains("data-metric=\"atendimentos\">8<", summary);
        using (var pdf = await client.GetAsync($"/Relatorios/Community?comunidadeId={communityId}&exportar=pdf"))
            await AssertFileAsync(pdf, "application/pdf", "%PDF");
    }

    [Fact(DisplayName = "Rede Primária — amplitude, densidade e centralidade calculadas pelos vínculos")]
    public async Task PrimaryNetwork_Metrics()
    {
        using var client = await LoggedClientAsync();
        var communityId = await SeedCommunityAsync();
        var maria = await SeedActorAsync(communityId, "Maria primária");
        var a = await SeedActorAsync(communityId, "Primária A");
        var b = await SeedActorAsync(communityId, "Primária B");
        var c = await SeedActorAsync(communityId, "Primária C");
        await SeedLinkAsync(maria, a, "Vizinho", TiposRede.Primaria);
        await SeedLinkAsync(maria, b, "Familiar", TiposRede.Primaria);
        await SeedLinkAsync(c, maria, "Amigo", TiposRede.Primaria);
        await SeedLinkAsync(maria, c, "Amigo", TiposRede.Subjetiva);

        var html = await client.GetStringAsync($"/Relatorios/PrimaryNetwork?comunidadeId={communityId}&atorId={maria}");
        Assert.Contains("data-metric=\"amplitude\">2<", html);
        Assert.Contains("data-metric=\"densidade\">50%<", html);    // 2 de 4 atores
        Assert.Contains("data-metric=\"centralidade\">1<", html);   // (2 saem + 1 chega) / (4 - 1)
    }

    // ── Apoio ─────────────────────────────────────────────────────────────

    private async Task<HttpClient> LoggedClientAsync()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var client = HttpFlowTestSupport.CreateClient(_factory);
        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, admin.Email, HttpFlowTestSupport.ValidPassword);
        HttpFlowTestSupport.AssertRedirect(login, "/");
        return client;
    }

    private Task<T> Db<T>(Func<ApplicationDbContext, Task<T>> operation) => HttpFlowTestSupport.InDatabaseAsync(_factory, operation);

    private Task<int> SeedCommunityAsync() => Db(async db =>
    {
        var community = new Comunidade
        {
            Nome = $"Comunidade relatórios {Guid.NewGuid():N}",
            Status = "Em processo",
            Ativo = "S",
            FK_Id_Usuario = TestIds.Seed(1),
            Dt_Criacao = DateTime.UtcNow
        };
        db.Comunidades.Add(community);
        await db.SaveChangesAsync();
        return community.Id_Comunidade;
    });

    private Task<int> SeedActorAsync(int communityId, string name, Action<Atores>? configure = null) => Db(async db =>
    {
        var actor = new Atores { Nome = name, Genero = 2, DtCriacao = DateTime.UtcNow, FkIdUsuario = TestIds.Seed(1) };
        configure?.Invoke(actor);
        actor.ConfigureCreationAggregate(communityId, null, null);
        db.Atores.Add(actor);
        await db.SaveChangesAsync();
        return actor.IdAtores;
    });

    private Task SeedLinkAsync(int from, int to, string relation, string network) => Db(async db =>
    {
        db.RedesPrimarias.Add(new RedePrimaria
        {
            FkIdAtorPrincipal = from, FkIdAtorRelacionados = to, TipoRelacao = relation, TipoRede = network
        });
        await db.SaveChangesAsync();
        return 0;
    });

    private Task SeedActionAsync(int communityId, string name, int quantity, int axisId, int actorId) => Db(async db =>
    {
        var diary = new DiarioCampo
        {
            FkIdComunidade = communityId, Data = new DateTime(2026, 11, 3), Descricao = "Diário de teste",
            DtCriacao = DateTime.UtcNow, FkIdUsuario = TestIds.Seed(1)
        };
        var action = new DiarioDAcoes { Diario = diary, Nome = name, PeovedorEx = "", Quantidade = quantity };
        var detail = new DetalhesDAcoes { DiarioDAcoes = action, Nome = name };
        detail.DetalhesEixos.Add(new DetalhesEixos { Detalhes = detail, FkIdEixo = axisId });
        action.Detalhes.Add(detail);
        action.DAtores.Add(new DAAtores { DiarioDAcoes = action, FK_id_Atores = actorId });
        db.DiariosCampo.Add(diary);
        db.DiarioDAcoes.Add(action);
        await db.SaveChangesAsync();
        return 0;
    });

    private static async Task AssertFileAsync(HttpResponseMessage response, string contentType, string magic)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(response.Content.Headers.ContentDisposition?.FileName);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(magic, Encoding.ASCII.GetString(bytes, 0, magic.Length));
    }
}
