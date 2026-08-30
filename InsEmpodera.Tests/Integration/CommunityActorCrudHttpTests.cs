using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class CommunityActorCrudHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public CommunityActorCrudHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CommunityActor_CreateEditDelete_WorksThroughTheRealHttpRoutes()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync();
        var actorName = $"Ator HTTP {Guid.NewGuid():N}";
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var create = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Comunidade/Create_Atores",
            new Dictionary<string, string>
            {
                ["ComunidadeId"] = communityId.ToString(),
                ["Nome"] = actorName,
                ["Genero"] = "1",
                ["Idade"] = "34",
                ["Telefone"] = "(11) 98888-1234",
                ["recursos"] = "Saude",
                ["vulnerabilidades"] = "Moradia"
            },
            $"/Comunidade/Create_Atores/{communityId}");
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);

        var actor = await LoadActorByNameAsync(actorName);
        AssertRandomKey(actor.IdAtores);
        Assert.Single(actor.Comunidades);
        AssertRandomKey(actor.Comunidades[0].IdAtorComunidade);
        Assert.Equal(20, actor.RecursosAtores.Count);
        Assert.Equal(20, actor.RecursosAtores.Select(item => item.Id_Recursos_Atores).Distinct().Count());
        Assert.All(actor.RecursosAtores, item => AssertRandomKey(item.Id_Recursos_Atores));

        var editedName = $"Ator HTTP editado {Guid.NewGuid():N}";
        var editPage = $"/Comunidade/Edit_Atores/{actor.IdAtores}?comunidadeId={communityId}";
        using var edit = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Comunidade/Edit_Atores",
            new Dictionary<string, string>
            {
                ["IdAtores"] = actor.IdAtores.ToString(),
                ["ComunidadeId"] = communityId.ToString(),
                ["Nome"] = editedName,
                ["Genero"] = "2",
                ["Idade"] = "35",
                ["Telefone"] = "(11) 97777-4321",
                ["recursos"] = "Lazer",
                ["vulnerabilidades"] = "Prevencao"
            },
            editPage);
        Assert.Equal(HttpStatusCode.Redirect, edit.StatusCode);

        var edited = await LoadActorByIdAsync(actor.IdAtores);
        Assert.Equal(editedName, edited.Nome);
        Assert.Equal("(11) 97777-4321", edited.Telefone);
        AssertFlag(edited, "Recurso", "Lazer", "S");
        AssertFlag(edited, "Recurso", "Saude", "N");
        AssertFlag(edited, "Vulnerabilidade", "Prevencao", "S");
        AssertFlag(edited, "Vulnerabilidade", "Moradia", "N");

        using var delete = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/Comunidade/Delete_Atores/{actor.IdAtores}",
            tokenPage: editPage);
        Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);
        Assert.Equal(
            "N",
            await HttpFlowTestSupport.InDatabaseAsync(
                _factory,
                db => db.Atores
                    .Where(item => item.IdAtores == actor.IdAtores)
                    .Select(item => item.Ativo)
                    .SingleAsync()));
    }

    private Task<int> SeedCommunityAsync() =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var community = new Comunidade
            {
                Nome = $"Comunidade atores HTTP {Guid.NewGuid():N}",
                Local = "Rua do teste, 100, São Paulo",
                LocalMapa = "Rua do teste, 100, São Paulo",
                Status = "Em processo",
                Ativo = "S",
                FK_Id_Usuario = 1,
                Dt_Criacao = DateTime.UtcNow,
                Dt_Modificacao = DateTime.UtcNow
            };
            db.Comunidades.Add(community);
            await db.SaveChangesAsync();
            return community.Id_Comunidade;
        });

    private Task<Atores> LoadActorByNameAsync(string name) =>
        HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Atores
                .AsNoTracking()
                .Include(item => item.Comunidades)
                .Include(item => item.RecursosAtores)
                .SingleAsync(item => item.Nome == name));

    private Task<Atores> LoadActorByIdAsync(int id) =>
        HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Atores
                .AsNoTracking()
                .Include(item => item.RecursosAtores)
                .SingleAsync(item => item.IdAtores == id));

    private static void AssertFlag(Atores actor, string type, string name, string expected) =>
        Assert.Equal(
            expected,
            actor.RecursosAtores.Single(item => item.Tipo == type && item.Nome == name).Pode);

    private static void AssertRandomKey(int id) => Assert.InRange(id, 100000, 999999);
}
