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

    private static Atores NewActor(string name) => new()
    {
        Nome = name,
        Genero = 1,
        Idade = 30,
        Ativo = "S"
    };
}