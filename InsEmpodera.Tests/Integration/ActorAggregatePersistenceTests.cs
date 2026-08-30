using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class ActorAggregatePersistenceTests : ControllerTestBase
{
    [Fact]
    public async Task MainActorCreate_PersistsLinkAndAllCanonicalResourceFlags()
    {
        var community = await CreateCommunityAsync();
        var actor = NewActor("Ator com agregado");
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));

        var result = await controller.Create(
            actor,
            community.Id_Comunidade,
            recursos: new List<string> { "RedePrimaria", "Saude", "valor-forjado" },
            vulnerabilidades: new List<string> { "Moradia", "Prevencao" });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Atores
            .Include(item => item.Comunidades)
            .Include(item => item.RecursosAtores)
            .SingleAsync(item => item.Nome == actor.Nome);
        Assert.Single(saved.Comunidades);
        Assert.Equal(community.Id_Comunidade, saved.Comunidades[0].FkIdComunidade);
        Assert.Equal(20, saved.RecursosAtores.Count);
        Assert.Equal(10, saved.RecursosAtores.Count(item => item.Tipo == "Recurso"));
        Assert.Equal(10, saved.RecursosAtores.Count(item => item.Tipo == "Vulnerabilidade"));

        var canonicalNames = ActorAggregateExtensions.CanonicalResourceNames.ToHashSet(StringComparer.Ordinal);
        Assert.True(canonicalNames.SetEquals(saved.RecursosAtores.Select(item => item.Nome)));
        AssertFlag(saved, "Recurso", "RedePrimaria", "S");
        AssertFlag(saved, "Recurso", "Saude", "S");
        AssertFlag(saved, "Recurso", "Moradia", "N");
        AssertFlag(saved, "Vulnerabilidade", "Moradia", "S");
        AssertFlag(saved, "Vulnerabilidade", "Prevencao", "S");
        AssertFlag(saved, "Vulnerabilidade", "Saude", "N");
        Assert.DoesNotContain(saved.RecursosAtores, item => item.Nome == "valor-forjado");
    }

    [Fact]
    public async Task MainActorCreate_WhenResourceInsertFails_RollsBackActorLinkAndResources()
    {
        var community = await CreateCommunityAsync();
        var actor = NewActor("Ator rollback principal");
        await InstallResourceFailureTriggerAsync();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));

        await Assert.ThrowsAsync<DbUpdateException>(() => controller.Create(
            actor,
            community.Id_Comunidade,
            recursos: new List<string> { "Saude" }));

        Db.ChangeTracker.Clear();
        await AssertAggregateAbsentAsync(actor.Nome);
    }

    [Fact]
    public async Task CommunityActorCreate_WhenResourceInsertFails_RollsBackActorLinkAndResources()
    {
        var community = await CreateCommunityAsync();
        var actor = NewActor("Ator rollback comunidade");
        await InstallResourceFailureTriggerAsync();
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));

        await Assert.ThrowsAsync<DbUpdateException>(() => controller.Create_Atores(
            actor,
            recursos: new List<string> { "Educacao" },
            vulnerabilidades: new List<string> { "Substancias" },
            ComunidadeId: community.Id_Comunidade));

        Db.ChangeTracker.Clear();
        await AssertAggregateAbsentAsync(actor.Nome);
    }

    [Fact]
    public async Task CommunityActorCrud_CreateEditAndDelete_PersistsTheCompleteFlowWithRandomKeys()
    {
        var community = await CreateCommunityAsync("Comunidade do CRUD de atores");
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));
        var actor = NewActor("Ator criado pela comunidade");

        var createResult = await controller.Create_Atores(
            actor,
            recursos: new List<string> { "Saude", "Educacao" },
            vulnerabilidades: new List<string> { "Moradia" },
            ComunidadeId: community.Id_Comunidade);

        var createRedirect = Assert.IsType<RedirectToActionResult>(createResult);
        Assert.Equal("AtoresVinculados", createRedirect.ActionName);
        Assert.Equal(community.Id_Comunidade, createRedirect.RouteValues?["id"]);

        Db.ChangeTracker.Clear();
        var saved = await Db.Atores
            .Include(item => item.Comunidades)
            .Include(item => item.RecursosAtores)
            .SingleAsync(item => item.Nome == "Ator criado pela comunidade");
        AssertRandomKey(saved.IdAtores);
        Assert.Single(saved.Comunidades);
        AssertRandomKey(saved.Comunidades[0].IdAtorComunidade);
        Assert.Equal(20, saved.RecursosAtores.Count);
        Assert.Equal(20, saved.RecursosAtores.Select(item => item.Id_Recursos_Atores).Distinct().Count());
        Assert.All(saved.RecursosAtores, item => AssertRandomKey(item.Id_Recursos_Atores));

        var editedActor = NewActor("Ator editado pela comunidade");
        editedActor.IdAtores = saved.IdAtores;
        editedActor.Telefone = "(11) 99999-0000";
        var editResult = await controller.Edit_Atores(
            editedActor,
            community.Id_Comunidade,
            recursos: new List<string> { "Lazer" },
            vulnerabilidades: new List<string> { "Substancias", "Prevencao" });

        var editRedirect = Assert.IsType<RedirectToActionResult>(editResult);
        Assert.Equal("AtoresVinculados", editRedirect.ActionName);
        Assert.Equal(community.Id_Comunidade, editRedirect.RouteValues?["id"]);

        Db.ChangeTracker.Clear();
        var edited = await Db.Atores
            .Include(item => item.RecursosAtores)
            .SingleAsync(item => item.IdAtores == saved.IdAtores);
        Assert.Equal("Ator editado pela comunidade", edited.Nome);
        Assert.Equal("(11) 99999-0000", edited.Telefone);
        AssertFlag(edited, "Recurso", "Lazer", "S");
        AssertFlag(edited, "Recurso", "Saude", "N");
        AssertFlag(edited, "Vulnerabilidade", "Substancias", "S");
        AssertFlag(edited, "Vulnerabilidade", "Prevencao", "S");
        AssertFlag(edited, "Vulnerabilidade", "Moradia", "N");

        var deleteResult = await controller.Delete_Atores(saved.IdAtores);

        var deleteRedirect = Assert.IsType<RedirectToActionResult>(deleteResult);
        Assert.Equal("AtoresVinculados", deleteRedirect.ActionName);
        Assert.Equal(community.Id_Comunidade, deleteRedirect.RouteValues?["id"]);
        Db.ChangeTracker.Clear();
        Assert.Equal(
            "N",
            await Db.Atores
                .Where(item => item.IdAtores == saved.IdAtores)
                .Select(item => item.Ativo)
                .SingleAsync());
    }

    private async Task InstallResourceFailureTriggerAsync()
    {
        await Db.Database.ExecuteSqlRawAsync(
            """
            CREATE TRIGGER fail_actor_resource_insert
            BEFORE INSERT ON RecursosAtores
            BEGIN
                SELECT RAISE(ABORT, 'forced actor resource failure');
            END;
            """);
    }

    private async Task AssertAggregateAbsentAsync(string actorName)
    {
        Assert.False(await Db.Atores.AsNoTracking().AnyAsync(actor => actor.Nome == actorName));
        Assert.False(await Db.AtorComunidades.AsNoTracking()
            .AnyAsync(link => link.Ator.Nome == actorName));
        Assert.False(await Db.RecursosAtores.AsNoTracking()
            .AnyAsync(resource => resource.Atores != null && resource.Atores.Nome == actorName));
    }

    private static void AssertFlag(Atores actor, string type, string name, string expected) =>
        Assert.Equal(
            expected,
            actor.RecursosAtores.Single(item => item.Tipo == type && item.Nome == name).Pode);

    private static void AssertRandomKey(int id) => Assert.InRange(id, 100000, 999999);

    private static Atores NewActor(string name) => new()
    {
        Nome = name,
        Genero = 1,
        Idade = 30,
        Ativo = "S"
    };
}
