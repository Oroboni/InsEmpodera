using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class PersonalAssessmentCrudTests : ControllerTestBase
{
    [Fact]
    public async Task Create_PersistsScoresAndAuditFields()
    {
        var actor = await CreateActorAsync();
        var controller = Attach(new PersonalAssessmentController(Db));
        var assessment = NewAssessment(actor.IdAtores, 2);

        var result = await controller.create(assessment);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(actor.IdAtores, redirect.RouteValues!["atorId"]);
        var saved = await Db.AvaliacaoPessoal.SingleAsync(item => item.IdAvaliacao == assessment.IdAvaliacao);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.NotEqual(default, saved.DtCriacao);
        Assert.Equal(2, saved.Saude);
    }

    [Fact]
    public async Task Read_FiltersAssessmentsByActor()
    {
        var firstActor = await CreateActorAsync(name: "Primeiro");
        var secondActor = await CreateActorAsync(name: "Segundo");
        Db.AvaliacaoPessoal.AddRange(NewPersistedAssessment(firstActor.IdAtores, 1), NewPersistedAssessment(secondActor.IdAtores, 2));
        await Db.SaveChangesAsync();
        var controller = Attach(new PersonalAssessmentController(Db));

        var result = await controller.Index(firstActor.IdAtores);

        var model = Assert.IsAssignableFrom<List<AvaliacaoPessoal>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model);
        Assert.Equal(firstActor.IdAtores, model[0].FK_id_Atores);
    }

    [Fact]
    public async Task Edit_UpdatesEveryScoreAndRedirectsToActor()
    {
        var actor = await CreateActorAsync();
        var assessment = NewPersistedAssessment(actor.IdAtores, 1);
        Db.AvaliacaoPessoal.Add(assessment);
        await Db.SaveChangesAsync();
        var controller = Attach(new PersonalAssessmentController(Db));
        var update = NewAssessment(actor.IdAtores, 5);

        var result = await controller.edit(update, assessment.IdAvaliacao);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(actor.IdAtores, redirect.RouteValues!["atorId"]);
        var saved = await Db.AvaliacaoPessoal.FindAsync(assessment.IdAvaliacao);
        Assert.Equal(5, saved!.CCrimes);
        Assert.Equal(5, saved.Substancias);
        Assert.Equal(5, saved.Lazer);
    }

    [Fact]
    public async Task Edit_MissingRecordReturnsNotFound()
    {
        var controller = Attach(new PersonalAssessmentController(Db));
        Assert.IsType<NotFoundResult>(await controller.edit(NewAssessment(1, 3), 999_999));
    }

    [Fact]
    public async Task Delete_RemovesOnlyRequestedAssessment()
    {
        var actor = await CreateActorAsync();
        var first = NewPersistedAssessment(actor.IdAtores, 1);
        var second = NewPersistedAssessment(actor.IdAtores, 2);
        Db.AvaliacaoPessoal.AddRange(first, second);
        await Db.SaveChangesAsync();
        var controller = Attach(new PersonalAssessmentController(Db));

        var result = await controller.Delete(first.IdAvaliacao);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(await Db.AvaliacaoPessoal.FindAsync(first.IdAvaliacao));
        Assert.NotNull(await Db.AvaliacaoPessoal.FindAsync(second.IdAvaliacao));
    }

    [Fact]
    public async Task Delete_MissingRecordReturnsNotFound()
    {
        var controller = Attach(new PersonalAssessmentController(Db));
        Assert.IsType<NotFoundResult>(await controller.Delete(999_999));
    }

    private static AvaliacaoPessoal NewPersistedAssessment(int actorId, int score)
    {
        var assessment = NewAssessment(actorId, score);
        assessment.FkIdUsuario = 1;
        assessment.DtCriacao = DateTime.UtcNow;
        assessment.DtModificacao = DateTime.UtcNow;
        return assessment;
    }

    private static AvaliacaoPessoal NewAssessment(int actorId, int score) => new()
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
