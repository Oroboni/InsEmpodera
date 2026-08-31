using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Empodera.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class PersonalProcessCrudTests : ControllerTestBase
{
    [Fact]
    public async Task CreateEditDelete_PersistsActorUserAndAxisRelationshipsWithRandomKeys()
    {
        var actor = await CreateActorAsync(name: "Ator do diário pessoal");
        var firstAxis = await CreateAxisAsync("Saúde");
        var secondAxis = await CreateAxisAsync("Educação");
        var controller = Attach(new PersonalProcessController(Db));

        var create = await controller.Create(new DiarioProcessoPessoal
        {
            FK_id_Atores = actor.IdAtores,
            Data = new DateTime(2026, 8, 31),
            Descricao = "Acompanhamento inicial"
        }, [firstAxis.IdEixo]);

        var createRedirect = Assert.IsType<RedirectToActionResult>(create);
        Assert.Equal(nameof(PersonalProcessController.Index), createRedirect.ActionName);
        Db.ChangeTracker.Clear();
        var saved = await Db.DiariosProcessoPessoal.Include(d => d.Eixos).SingleAsync();
        Assert.InRange(saved.IdDiarioProcesso, 100000, 999999);
        Assert.Equal(actor.IdAtores, saved.FK_id_Atores);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.Single(saved.Eixos);
        Assert.InRange(saved.Eixos[0].IdDiarioProcessoEixo, 100000, 999999);
        Assert.Equal(firstAxis.IdEixo, saved.Eixos[0].FkIdEixo);

        var edit = await controller.Edit(saved.IdDiarioProcesso, new DiarioProcessoPessoal
        {
            IdDiarioProcesso = saved.IdDiarioProcesso,
            FK_id_Atores = actor.IdAtores,
            Data = new DateTime(2026, 9, 1),
            Descricao = "Acompanhamento atualizado"
        }, [secondAxis.IdEixo]);

        Assert.IsType<RedirectToActionResult>(edit);
        Db.ChangeTracker.Clear();
        var updated = await Db.DiariosProcessoPessoal.Include(d => d.Eixos).SingleAsync();
        Assert.Equal("Acompanhamento atualizado", updated.Descricao);
        Assert.Equal(new DateTime(2026, 9, 1), updated.Data);
        Assert.Equal([secondAxis.IdEixo], updated.Eixos.Select(e => e.FkIdEixo).ToArray());

        var delete = await controller.Delete(saved.IdDiarioProcesso);

        Assert.IsType<RedirectToActionResult>(delete);
        Assert.False(await Db.DiariosProcessoPessoal.AnyAsync());
        Assert.False(await Db.DiariosProcessoEixos.AnyAsync());
    }

    [Fact]
    public async Task Create_WithUnknownActorOrAxis_DoesNotPersistAndReturnsValidationErrors()
    {
        var controller = Attach(new PersonalProcessController(Db));

        var result = await controller.Create(new DiarioProcessoPessoal
        {
            FK_id_Atores = 999999,
            Data = DateTime.Today,
            Descricao = "Registro inválido"
        }, [999999]);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.False(await Db.DiariosProcessoPessoal.AnyAsync());
    }

    [Fact]
    public async Task GeneralExcelBackup_RestoresPersonalProcessAndAxisLink()
    {
        var actor = await CreateActorAsync(name: "Ator preservado no backup");
        var axis = await CreateAxisAsync("Eixo preservado no backup");
        var diary = new DiarioProcessoPessoal
        {
            FK_id_Atores = actor.IdAtores,
            Data = new DateTime(2026, 8, 31),
            Descricao = "Registro que deve voltar do Excel",
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow,
            FkIdUsuario = 1,
            FkIdUsuarioM = 1,
            Eixos = [new DiarioProcessoEixo { FkIdEixo = axis.IdEixo }]
        };
        Db.DiariosProcessoPessoal.Add(diary);
        await Db.SaveChangesAsync();
        var diaryId = diary.IdDiarioProcesso;
        var backup = new ExcelBackupService(Db);
        var bytes = await backup.ExportAsync(CancellationToken.None);

        Db.DiariosProcessoPessoal.Remove(diary);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        Assert.False(await Db.DiariosProcessoPessoal.AnyAsync(d => d.IdDiarioProcesso == diaryId));

        await using var stream = new MemoryStream(bytes);
        await backup.ImportMissingAsync(stream, CancellationToken.None);

        var restored = await Db.DiariosProcessoPessoal.AsNoTracking()
            .Include(d => d.Eixos)
            .SingleAsync(d => d.IdDiarioProcesso == diaryId);
        Assert.Equal("Registro que deve voltar do Excel", restored.Descricao);
        Assert.Equal([axis.IdEixo], restored.Eixos.Select(e => e.FkIdEixo).ToArray());
    }
}
