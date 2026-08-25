using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class RelationalPersistenceFlowTests : ControllerTestBase
{
    [Fact]
    public async Task ActivityEdit_IgnoresOverpostedIdentityAndAuditFields()
    {
        var community = await CreateCommunityAsync();
        var createdAt = new DateTime(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var activity = new Atividades
        {
            Nome = "Original", Descricao = "Original", FkIdComunidade = community.Id_Comunidade,
            FkIdUsuario = 1, DtCriacao = createdAt, DtModificacao = createdAt
        };
        Db.Atividades.Add(activity);
        await Db.SaveChangesAsync();
        var payload = new Atividades
        {
            IdAtividade = activity.IdAtividade,
            Nome = "Editada",
            Descricao = "Editada",
            FkIdUsuario = 5,
            FkIdUsuarioM = 5,
            DtCriacao = DateTime.UtcNow.AddYears(10),
            DtModificacao = DateTime.UtcNow.AddYears(10)
        };

        var result = await Attach(new AtividadesController(Db), userId: 2)
            .Edit(activity.IdAtividade, payload, new List<int>(), community.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Atividades.AsNoTracking().SingleAsync(item => item.IdAtividade == activity.IdAtividade);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.Equal(2, saved.FkIdUsuarioM);
        Assert.Equal(createdAt, saved.DtCriacao);
        Assert.True(saved.DtModificacao < DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task AssessmentEdit_PreservesActorCreatorAndCreationTimestamp()
    {
        var firstActor = await CreateActorAsync(name: "Ator original");
        var secondActor = await CreateActorAsync(name: "Ator adulterado");
        var createdAt = new DateTime(2025, 2, 20, 10, 0, 0, DateTimeKind.Utc);
        var assessment = Assessment(firstActor.IdAtores, 1);
        assessment.FkIdUsuario = 1;
        assessment.DtCriacao = createdAt;
        assessment.DtModificacao = createdAt;
        Db.AvaliacaoPessoal.Add(assessment);
        await Db.SaveChangesAsync();
        var payload = Assessment(secondActor.IdAtores, 5);
        payload.FkIdUsuario = 5;
        payload.DtCriacao = DateTime.UtcNow.AddYears(10);

        var result = await Attach(new PersonalAssessmentController(Db), userId: 2)
            .edit(payload, assessment.IdAvaliacao);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.AvaliacaoPessoal.AsNoTracking()
            .SingleAsync(item => item.IdAvaliacao == assessment.IdAvaliacao);
        Assert.Equal(firstActor.IdAtores, saved.FK_id_Atores);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.Equal(createdAt, saved.DtCriacao);
        Assert.Equal(5, saved.Saude);
    }

    [Fact]
    public async Task DatabaseRejectsCoreOrphansWithoutPersistingThem()
    {
        var marker = $"Órfã {Guid.NewGuid():N}";
        Db.Atividades.Add(new Atividades
        {
            Nome = marker,
            Descricao = "Referências inexistentes",
            FkIdComunidade = 999_991,
            FkIdUsuario = 999_992,
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => Db.SaveChangesAsync());

        Db.ChangeTracker.Clear();
        Assert.False(await Db.Atividades.AnyAsync(item => item.Nome == marker));
    }

    [Fact]
    public async Task DeletingActorAtDatabaseLevel_CascadesCompleteDependentGraphOnly()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var assessment = Assessment(actor.IdAtores, 2);
        assessment.FkIdUsuario = 1;
        assessment.DtCriacao = DateTime.UtcNow;
        assessment.DtModificacao = DateTime.UtcNow;
        var form = new FichaPrimeiroContato
        {
            FK_id_Atores = actor.IdAtores,
            FkIdComunidade = community.Id_Comunidade,
            FkIdUsuario = 1,
            DtContato = DateTime.UtcNow,
            HoraContato = TimeSpan.FromHours(9),
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow
        };
        Db.AddRange(assessment, form);
        await Db.SaveChangesAsync();
        Db.FichaCondicoes.Add(new FichaCondicoes { FkIdFicha = form.IdFicha, Cond = "Condição" });
        Db.FichaResultados.Add(new FichaResult { FkIdFicha = form.IdFicha, Result = "Resultado" });
        Db.RecursosAtores.Add(new RecursosAtores
        {
            FK_id_Atores = actor.IdAtores, Nome = "Saude", Tipo = "Recurso", Pode = "S"
        });
        await Db.SaveChangesAsync();

        Db.Atores.Remove(actor);
        await Db.SaveChangesAsync();

        Assert.NotNull(await Db.Comunidades.FindAsync(community.Id_Comunidade));
        Assert.False(await Db.AtorComunidades.AnyAsync(item => item.FK_id_Atores == actor.IdAtores));
        Assert.False(await Db.AvaliacaoPessoal.AnyAsync(item => item.IdAvaliacao == assessment.IdAvaliacao));
        Assert.False(await Db.FichasPrimeiroContato.AnyAsync(item => item.IdFicha == form.IdFicha));
        Assert.False(await Db.FichaCondicoes.AnyAsync(item => item.FkIdFicha == form.IdFicha));
        Assert.False(await Db.FichaResultados.AnyAsync(item => item.FkIdFicha == form.IdFicha));
        Assert.False(await Db.RecursosAtores.AnyAsync(item => item.FK_id_Atores == actor.IdAtores));
    }

    private static AvaliacaoPessoal Assessment(int actorId, int score) => new()
    {
        FK_id_Atores = actorId,
        CCrimes = score,
        Substancias = score,
        Moradia = score,
        Prevencao = score,
        AssBasica = score,
        Educacao = score,
        Saude = score,
        Ocupacao = score,
        Lazer = score
    };
}
