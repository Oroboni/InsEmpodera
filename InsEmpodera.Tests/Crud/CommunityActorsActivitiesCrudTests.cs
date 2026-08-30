using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class CommunityActorsActivitiesCrudTests : ControllerTestBase
{
    [Fact]
    public async Task Community_Create_NormalizesAndPersistsAuditFields()
    {
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));
        var community = new Comunidade
        {
            Nome = "Comunidade CRUD",
            Local = "Centro Social, Rua Um, 10, Fortaleza",
            LocalSecundario = "Anexo Comunitário, Rua Dois, 20, Fortaleza",
            Status = "Em Processo",
            Ativo = "S"
        };

        var result = controller.ComunidadesDetalhes(community, 0);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ComunidadesDetalhes", redirect.ActionName);
        var saved = await Db.Comunidades.SingleAsync(item => item.Nome == community.Nome);
        Assert.Equal("Em processo", saved.Status);
        Assert.Equal(1, saved.FK_Id_Usuario);
        Assert.False(string.IsNullOrWhiteSpace(saved.LocalMapa));
        Assert.Equal("Anexo Comunitário, Rua Dois, 20, Fortaleza", saved.LocalSecundario);
        Assert.False(string.IsNullOrWhiteSpace(saved.LocalMapaSecundario));
        Assert.NotEqual(default, saved.Dt_Criacao);
    }

    [Fact]
    public async Task Community_Read_ExistingReturnsModelAndMissingReturnsNotFound()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));

        var existing = controller.ComunidadesDetalhes(community.Id_Comunidade);
        var missing = controller.ComunidadesDetalhes(999_999);

        Assert.Equal(community.Id_Comunidade, Assert.IsType<Comunidade>(Assert.IsType<ViewResult>(existing).Model).Id_Comunidade);
        Assert.IsType<NotFoundResult>(missing);
    }

    [Fact]
    public async Task Community_Edit_UpdatesOnlyEditableFieldsAndAudit()
    {
        var original = await CreateCommunityAsync("Antes");
        var createdAt = original.Dt_Criacao;
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));
        var update = new Comunidade
        {
            Id_Comunidade = original.Id_Comunidade,
            Nome = "Depois",
            Local = "Avenida Nova, 200, Recife",
            LocalSecundario = "Polo Comunitário, Avenida Sul, 50, Recife",
            Status = "Em Diagnóstico",
            Complemento = "Complemento atualizado",
            Descricao = "Descrição atualizada",
            Ativo = "N"
        };

        var result = controller.ComunidadesDetalhes(update, original.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Comunidades.AsNoTracking().SingleAsync(item => item.Id_Comunidade == original.Id_Comunidade);
        Assert.Equal("Depois", saved.Nome);
        Assert.Equal("Em diagnóstico", saved.Status);
        Assert.Equal("Polo Comunitário, Avenida Sul, 50, Recife", saved.LocalSecundario);
        Assert.False(string.IsNullOrWhiteSpace(saved.LocalMapaSecundario));
        Assert.Equal(createdAt, saved.Dt_Criacao);
        Assert.Equal("S", saved.Ativo);
        Assert.Equal(1, saved.FK_Id_UsuarioM);
    }

    [Fact]
    public async Task Community_Edit_MissingRecordReturnsNotFound()
    {
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));
        var result = controller.ComunidadesDetalhes(new Comunidade { Id_Comunidade = 999_999, Nome = "Ausente" }, 999_999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Community_Delete_SoftDeletesCommunityAndLinkedActors()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));

        var result = await controller.Delete(community.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("N", (await Db.Comunidades.FindAsync(community.Id_Comunidade))!.Ativo);
        Assert.Equal("N", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
    }

    [Fact]
    public async Task Actor_Create_PersistsActorAndCommunityLink()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));
        var actor = NewActor("Ator criado");

        var result = await controller.Create(actor, community.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Atores.SingleAsync(item => item.Nome == actor.Nome);
        Assert.Equal(1, saved.FkIdUsuario);
        Assert.True(await Db.AtorComunidades.AnyAsync(link => link.FK_id_Atores == saved.IdAtores && link.FkIdComunidade == community.Id_Comunidade));
    }

    [Fact]
    public async Task Actor_Create_InvalidCommunityDoesNotLeaveOrphan()
    {
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));
        var before = await Db.Atores.CountAsync();

        var result = await controller.Create(NewActor("Sem comunidade"), 999_999);

        Assert.IsType<ViewResult>(result);
        Assert.Equal(before, await Db.Atores.CountAsync());
    }

    [Fact]
    public async Task Actor_Edit_UpdatesFieldsAndMovesCommunityLink()
    {
        var first = await CreateCommunityAsync("Primeira");
        var second = await CreateCommunityAsync("Segunda");
        var actor = await CreateActorAsync(first, "Antes");
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));
        var update = NewActor("Depois");
        update.Telefone = "11999990000";

        var result = await controller.Edit(update, actor.IdAtores, second.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Atores.FindAsync(actor.IdAtores);
        Assert.Equal("Depois", saved!.Nome);
        Assert.Equal(1, saved.FkIdUsuarioM);
        Assert.Equal(second.Id_Comunidade, (await Db.AtorComunidades.SingleAsync(item => item.FK_id_Atores == actor.IdAtores)).FkIdComunidade);
    }

    [Fact]
    public async Task Actor_Delete_PerformsSoftDelete()
    {
        var actor = await CreateActorAsync();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));
        var result = await controller.Delete(actor.IdAtores);
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("N", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
    }

    [Fact]
    public async Task Activity_Create_PersistsSelectedAxes()
    {
        var community = await CreateCommunityAsync();
        var firstAxis = await CreateAxisAsync("Eixo A");
        var secondAxis = await CreateAxisAsync("Eixo B");
        var controller = Attach(new AtividadesController(Db));
        var activity = NewActivity("Atividade criada");

        var result = await controller.Create(activity, new List<int> { firstAxis.IdEixo, secondAxis.IdEixo }, community.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await Db.Atividades.SingleAsync(item => item.Nome == activity.Nome);
        Assert.Equal(community.Id_Comunidade, saved.FkIdComunidade);
        Assert.Equal(2, await Db.AtividadesEixo.CountAsync(item => item.FkIdAtividade == saved.IdAtividade));
    }

    [Fact]
    public async Task Activity_Edit_ReplacesAxisLinksWithoutDuplicates()
    {
        var community = await CreateCommunityAsync();
        var firstAxis = await CreateAxisAsync("Antigo");
        var secondAxis = await CreateAxisAsync("Novo");
        var activity = NewActivity("Antes");
        activity.FkIdComunidade = community.Id_Comunidade;
        activity.FkIdUsuario = 1;
        Db.Atividades.Add(activity);
        await Db.SaveChangesAsync();
        Db.AtividadesEixo.Add(new AtividadesEixo { FkIdAtividade = activity.IdAtividade, FkIdEixo = firstAxis.IdEixo });
        await Db.SaveChangesAsync();
        var controller = Attach(new AtividadesController(Db));

        var result = await controller.Edit(activity.IdAtividade,
            new Atividades { IdAtividade = activity.IdAtividade, Nome = "Depois", Descricao = "Atualizada" },
            new List<int> { secondAxis.IdEixo }, community.Id_Comunidade);

        Assert.IsType<RedirectToActionResult>(result);
        var savedLinks = await Db.AtividadesEixo.Where(item => item.FkIdAtividade == activity.IdAtividade).ToListAsync();
        Assert.Single(savedLinks);
        Assert.Equal(secondAxis.IdEixo, savedLinks[0].FkIdEixo);
        Assert.Equal("Depois", (await Db.Atividades.FindAsync(activity.IdAtividade))!.Nome);
    }

    [Fact]
    public async Task Activity_Delete_RemovesActivityAndLinks()
    {
        var community = await CreateCommunityAsync();
        var axis = await CreateAxisAsync();
        var activity = NewActivity("Excluir");
        activity.FkIdComunidade = community.Id_Comunidade;
        activity.FkIdUsuario = 1;
        Db.Atividades.Add(activity);
        await Db.SaveChangesAsync();
        Db.AtividadesEixo.Add(new AtividadesEixo { FkIdAtividade = activity.IdAtividade, FkIdEixo = axis.IdEixo });
        await Db.SaveChangesAsync();
        var controller = Attach(new AtividadesController(Db));

        var result = await controller.Delete(activity.IdAtividade);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(await Db.Atividades.FindAsync(activity.IdAtividade));
        Assert.False(await Db.AtividadesEixo.AnyAsync(item => item.FkIdAtividade == activity.IdAtividade));
    }

    private static Atores NewActor(string name) => new()
    {
        Nome = name,
        Idade = 29,
        Genero = 1,
        Ativo = "S"
    };

    private static Atividades NewActivity(string name) => new()
    {
        Nome = name,
        Descricao = "Descrição de teste"
    };
}
