using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class FirstContactCrudTests : ControllerTestBase
{
    [Fact]
    public async Task Create_PersistsFormAndAllMultiSelectCollections()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var controller = Attach(new FichaPrimeiroContatoController(Db));
        var form = NewForm(actor.IdAtores, community.Id_Comunidade);

        var result = await controller.Create(form,
            new List<string> { "Condição A", "Condição B" },
            new List<string> { "Petição A" },
            new List<string> { "Resposta A" },
            new List<string> { "Resultado A" });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.FichasPrimeiroContato.SingleAsync(item => item.IdFicha == form.IdFicha);
        Assert.Equal("EmProgresso", saved.Status);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.Equal(2, await Db.FichaCondicoes.CountAsync(item => item.FkIdFicha == saved.IdFicha));
        Assert.Single(await Db.FichaPeticoes.Where(item => item.FkIdFicha == saved.IdFicha).ToListAsync());
        Assert.Single(await Db.FichaRespostas.Where(item => item.FkIdFicha == saved.IdFicha).ToListAsync());
        Assert.Single(await Db.FichaResultados.Where(item => item.FkIdFicha == saved.IdFicha).ToListAsync());
    }

    [Fact]
    public async Task Read_ReturnsFormWithRelationshipsAndMissingReturnsNotFound()
    {
        var form = await AddFormAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db));

        var existing = await controller.Edit(form.IdFicha);
        var missing = await controller.Edit(999_999);

        Assert.Equal(form.IdFicha, Assert.IsType<FichaPrimeiroContato>(Assert.IsType<ViewResult>(existing).Model).IdFicha);
        Assert.IsType<NotFoundResult>(missing);
    }

    [Fact]
    public async Task Edit_UpdatesMainFieldsAndAtomicallyReplacesSelections()
    {
        var form = await AddFormAsync();
        Db.FichaCondicoes.Add(new FichaCondicoes { FkIdFicha = form.IdFicha, Cond = "Antiga" });
        Db.FichaPeticoes.Add(new FichaPeticoes { FkIdFicha = form.IdFicha, Pet = "Antiga" });
        await Db.SaveChangesAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db));
        var update = NewForm(form.FK_id_Atores, form.FkIdComunidade!.Value);
        update.IdFicha = form.IdFicha;
        update.Coment = "Comentário editado";
        update.Status = "Concluida";

        var result = await controller.Edit(form.IdFicha, update,
            new List<string> { "Nova" }, new List<string> { "Nova" },
            new List<string> { "Nova" }, new List<string> { "Nova" });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.FichasPrimeiroContato.AsNoTracking().SingleAsync(item => item.IdFicha == form.IdFicha);
        Assert.Equal("Comentário editado", saved.Coment);
        Assert.Equal(form.DtCriacao, saved.DtCriacao);
        Assert.Equal(form.FkIdUsuario, saved.FkIdUsuario);
        Assert.Equal(new[] { "Nova" }, await Db.FichaCondicoes.Where(item => item.FkIdFicha == form.IdFicha).Select(item => item.Cond).ToArrayAsync());
        Assert.Equal(new[] { "Nova" }, await Db.FichaPeticoes.Where(item => item.FkIdFicha == form.IdFicha).Select(item => item.Pet).ToArrayAsync());
    }

    [Fact]
    public async Task Edit_IdMismatchReturnsNotFoundWithoutChangingRecord()
    {
        var form = await AddFormAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db));
        var update = NewForm(form.FK_id_Atores, form.FkIdComunidade!.Value);
        update.IdFicha = form.IdFicha + 1;

        var result = await controller.Edit(form.IdFicha, update, null, null, null, null);

        Assert.IsType<NotFoundResult>(result);
        Assert.NotNull(await Db.FichasPrimeiroContato.FindAsync(form.IdFicha));
    }

    [Fact]
    public async Task StatusActions_UpdateStateAndRejectExternalReturnUrl()
    {
        var form = await AddFormAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db));

        var completed = await controller.Concluir(form.IdFicha, "https://malicioso.example");
        Assert.IsType<RedirectToActionResult>(completed);
        Assert.Equal("Concluida", (await Db.FichasPrimeiroContato.FindAsync(form.IdFicha))!.Status);

        var abandoned = await controller.Abandonar(form.IdFicha, "/FichaPrimeiroContato");
        Assert.IsType<LocalRedirectResult>(abandoned);
        Assert.Equal("Abandonada", (await Db.FichasPrimeiroContato.FindAsync(form.IdFicha))!.Status);
    }

    [Fact]
    public async Task Delete_RemovesFormAndDependentSelections()
    {
        var form = await AddFormAsync();
        Db.FichaCondicoes.Add(new FichaCondicoes { FkIdFicha = form.IdFicha, Cond = "Excluir" });
        Db.FichaResultados.Add(new FichaResult { FkIdFicha = form.IdFicha, Result = "Excluir" });
        await Db.SaveChangesAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db));

        var result = await controller.DeleteConfirmed(form.IdFicha);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(await Db.FichasPrimeiroContato.FindAsync(form.IdFicha));
        Assert.False(await Db.FichaCondicoes.AnyAsync(item => item.FkIdFicha == form.IdFicha));
        Assert.False(await Db.FichaResultados.AnyAsync(item => item.FkIdFicha == form.IdFicha));
    }

    [Fact]
    public async Task EditGet_WithoutSessionRedirectsBeforeExposingData()
    {
        var form = await AddFormAsync();
        var controller = Attach(new FichaPrimeiroContatoController(Db), userId: null);

        var result = await controller.Edit(form.IdFicha);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Account", redirect.ControllerName);
    }

    private async Task<FichaPrimeiroContato> AddFormAsync()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var form = NewForm(actor.IdAtores, community.Id_Comunidade);
        form.FkIdUsuario = 1;
        form.DtCriacao = DateTime.UtcNow;
        form.DtModificacao = DateTime.UtcNow;
        Db.FichasPrimeiroContato.Add(form);
        await Db.SaveChangesAsync();
        return form;
    }

    private static FichaPrimeiroContato NewForm(int actorId, int communityId) => new()
    {
        FK_id_Atores = actorId,
        FkIdComunidade = communityId,
        Endereco = "Rua do Primeiro Contato, 10",
        Telefone = "11999990000",
        DtContato = new DateTime(2026, 8, 23),
        HoraContato = new TimeSpan(10, 30, 0),
        Status = "EmProgresso"
    };
}
