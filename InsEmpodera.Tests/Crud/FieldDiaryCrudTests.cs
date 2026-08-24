using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class FieldDiaryCrudTests : ControllerTestBase
{
    [Fact]
    public async Task Create_PersistsDiaryAuditAndAxes()
    {
        var community = await CreateCommunityAsync();
        var firstAxis = await CreateAxisAsync("Diário A");
        var secondAxis = await CreateAxisAsync("Diário B");
        var controller = Attach(new DiarioCampoController(Db));
        var diary = NewDiary(community.Id_Comunidade, "Criado");

        var result = await controller.Create(diary, new[] { firstAxis.IdEixo, secondAxis.IdEixo });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.DiariosCampo.SingleAsync(item => item.Descricao == "Criado");
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.NotEqual(default, saved.DtCriacao);
        Assert.Equal(2, await Db.DiarioEixos.CountAsync(item => item.FkIdDiario == saved.IdDCampo));
    }

    [Fact]
    public async Task Create_InvalidModelDoesNotPersist()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new DiarioCampoController(Db));
        controller.ModelState.AddModelError(nameof(DiarioCampo.Descricao), "Obrigatória");
        var before = await Db.DiariosCampo.CountAsync();

        var result = await controller.Create(NewDiary(community.Id_Comunidade, ""), Array.Empty<int>());

        Assert.IsType<ViewResult>(result);
        Assert.Equal(before, await Db.DiariosCampo.CountAsync());
    }

    [Fact]
    public async Task Read_ReturnsDetailsAndMissingReturnsNotFound()
    {
        var diary = await AddDiaryAsync();
        var controller = Attach(new DiarioCampoController(Db));

        var details = await controller.Details(diary.IdDCampo);
        var missing = await controller.Details(999_999);

        Assert.Equal(diary.IdDCampo, Assert.IsType<DiarioCampo>(Assert.IsType<ViewResult>(details).Model).IdDCampo);
        Assert.IsType<NotFoundResult>(missing);
    }

    [Fact]
    public async Task Edit_UpdatesFieldsAndReplacesAxes()
    {
        var diary = await AddDiaryAsync();
        var oldAxis = await CreateAxisAsync("Antigo");
        var newAxis = await CreateAxisAsync("Novo");
        Db.DiarioEixos.Add(new DiarioEixo { FkIdDiario = diary.IdDCampo, FkIdEixo = oldAxis.IdEixo });
        await Db.SaveChangesAsync();
        var controller = Attach(new DiarioCampoController(Db));
        var update = NewDiary(diary.FkIdComunidade, "Descrição editada");
        update.IdDCampo = diary.IdDCampo;
        update.Localizacao = "Novo local";

        var result = await controller.Edit(diary.IdDCampo, update, new[] { newAxis.IdEixo });

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.DiariosCampo.FindAsync(diary.IdDCampo);
        Assert.Equal("Descrição editada", saved!.Descricao);
        Assert.Equal("Novo local", saved.Localizacao);
        var link = await Db.DiarioEixos.SingleAsync(item => item.FkIdDiario == diary.IdDCampo);
        Assert.Equal(newAxis.IdEixo, link.FkIdEixo);
    }

    [Fact]
    public async Task Edit_IdMismatchAndMissingReturnNotFound()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new DiarioCampoController(Db));
        Assert.IsType<NotFoundResult>(await controller.Edit(10, NewDiary(community.Id_Comunidade, "Mismatch"), Array.Empty<int>()));

        var missing = NewDiary(community.Id_Comunidade, "Missing");
        missing.IdDCampo = 999_999;
        Assert.IsType<NotFoundResult>(await controller.Edit(missing.IdDCampo, missing, Array.Empty<int>()));
    }

    [Fact]
    public async Task Delete_RemovesDiaryAndDependentAxisLinks()
    {
        var diary = await AddDiaryAsync();
        var axis = await CreateAxisAsync();
        Db.DiarioEixos.Add(new DiarioEixo { FkIdDiario = diary.IdDCampo, FkIdEixo = axis.IdEixo });
        await Db.SaveChangesAsync();
        var controller = Attach(new DiarioCampoController(Db));

        var result = await controller.DeleteConfirmed(diary.IdDCampo);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(await Db.DiariosCampo.FindAsync(diary.IdDCampo));
        Assert.False(await Db.DiarioEixos.AnyAsync(item => item.FkIdDiario == diary.IdDCampo));
    }

    [Fact]
    public async Task Mutations_WithoutPermissionDoNotChangeDatabase()
    {
        var diary = await AddDiaryAsync();
        var controller = Attach(new DiarioCampoController(Db), userId: 4);

        var deleteResult = await controller.DeleteConfirmed(diary.IdDCampo);

        Assert.IsType<RedirectToActionResult>(deleteResult);
        Assert.NotNull(await Db.DiariosCampo.FindAsync(diary.IdDCampo));
    }

    [Fact]
    public async Task Mutations_WithoutSessionRedirectToLogin()
    {
        var diary = await AddDiaryAsync();
        var controller = Attach(new DiarioCampoController(Db), userId: null);

        var result = await controller.DeleteConfirmed(diary.IdDCampo);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Account", redirect.ControllerName);
        Assert.NotNull(await Db.DiariosCampo.FindAsync(diary.IdDCampo));
    }

    private async Task<DiarioCampo> AddDiaryAsync()
    {
        var community = await CreateCommunityAsync();
        var diary = NewDiary(community.Id_Comunidade, $"Diário {Guid.NewGuid():N}");
        diary.FkIdUsuario = 1;
        diary.DtCriacao = DateTime.UtcNow;
        diary.DtModificacao = DateTime.UtcNow;
        Db.DiariosCampo.Add(diary);
        await Db.SaveChangesAsync();
        return diary;
    }

    private static DiarioCampo NewDiary(int communityId, string description) => new()
    {
        FkIdComunidade = communityId,
        Data = new DateTime(2026, 8, 23),
        Descricao = description,
        Localizacao = "Local de teste",
        Foto = "sem-foto.png"
    };
}
