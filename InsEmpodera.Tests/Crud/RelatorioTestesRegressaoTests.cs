using System.Net;
using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InsEmpodera.Tests.Crud;

// Regressões dos relatórios de testes: formulários incompletos não podem virar erro 500,
// datas futuras são recusadas e registros excluídos não aparecem nem abrem pela URL.
public sealed class RelatorioTestesRegressaoTests : ControllerTestBase
{
    [Fact(DisplayName = "C1 — comunidade só com espaços volta ao formulário sem gravar")]
    public async Task Comunidade_SoEspacos_VoltaComMensagem()
    {
        var controller = Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db));
        var antes = await Db.Comunidades.CountAsync();

        var result = controller.ComunidadesDetalhes(new Comunidade { Nome = "   ", Local = "   ", LocalMapa = "   " }, 0);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(antes, await Db.Comunidades.CountAsync());
    }

    [Fact(DisplayName = "C2 — ficha de primeiro contato vazia volta ao formulário sem gravar")]
    public async Task Ficha_Vazia_VoltaComMensagens()
    {
        var controller = Attach(new FichaPrimeiroContatoController(Db));

        var result = await controller.Create(new FichaPrimeiroContato(), [], [], [], []);

        Assert.IsType<ViewResult>(result);
        Assert.Contains(nameof(FichaPrimeiroContato.FkIdComunidade), controller.ModelState.Keys);
        Assert.Contains(nameof(FichaPrimeiroContato.FK_id_Atores), controller.ModelState.Keys);
        Assert.Contains(nameof(FichaPrimeiroContato.DtContato), controller.ModelState.Keys);
        Assert.False(await Db.FichasPrimeiroContato.AnyAsync());
    }

    [Fact(DisplayName = "A2 — ficha com data futura é recusada")]
    public async Task Ficha_DataFutura_Recusada()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var controller = Attach(new FichaPrimeiroContatoController(Db));
        var ficha = new FichaPrimeiroContato
        {
            FkIdComunidade = community.Id_Comunidade, FK_id_Atores = actor.IdAtores,
            DtContato = DateTime.Today.AddDays(1), HoraContato = new TimeSpan(9, 0, 0),
            CEstabeleceu = "Visita", NovoParceiro = "Sim", FornecidoParceiro = "Não"
        };

        var result = await controller.Create(ficha, [], [], [], []);

        Assert.IsType<ViewResult>(result);
        Assert.Contains(nameof(FichaPrimeiroContato.DtContato), controller.ModelState.Keys);
        Assert.False(await Db.FichasPrimeiroContato.AnyAsync());
    }

    [Fact(DisplayName = "C3 — atividade vazia volta ao formulário sem gravar")]
    public async Task Atividade_Vazia_VoltaComMensagens()
    {
        var controller = Attach(new AtividadesController(Db));

        var result = await controller.Create(new Atividades(), new List<int>(), 0);

        Assert.IsType<ViewResult>(result);
        Assert.Contains("EixosSelecionados", controller.ModelState.Keys);
        Assert.False(await Db.Atividades.AnyAsync());
    }

    [Fact(DisplayName = "#07 — diário de campo com data futura é recusado")]
    public async Task Diario_DataFutura_Recusado()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new DiarioCampoController(Db));
        var diario = new DiarioCampo
        {
            FkIdComunidade = community.Id_Comunidade, Data = new DateTime(2099, 12, 31),
            Descricao = "Relato", Localizacao = string.Empty
        };

        var result = await controller.Create(diario, []);

        Assert.IsType<ViewResult>(result);
        Assert.Contains(nameof(DiarioCampo.Data), controller.ModelState.Keys);
        Assert.False(await Db.DiariosCampo.AnyAsync());
    }

    [Fact(DisplayName = "#14/M2 — salvar diário volta para a lista na mesma comunidade")]
    public async Task Diario_Salvo_VoltaParaComunidade()
    {
        var community = await CreateCommunityAsync();
        var controller = Attach(new DiarioCampoController(Db));
        var diario = new DiarioCampo
        {
            FkIdComunidade = community.Id_Comunidade, Data = DateTime.Today,
            Descricao = "Relato", Localizacao = string.Empty
        };

        var result = await controller.Create(diario, []);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(community.Id_Comunidade, redirect.RouteValues?["comunidadeId"]);
    }

    [Fact(DisplayName = "C5 — comunidade excluída some das consultas")]
    public async Task ComunidadeExcluida_NaoApareceNasConsultas()
    {
        var community = await CreateCommunityAsync();
        community.Ativo = "N";
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        Assert.False(await Db.Comunidades.AnyAsync(c => c.Id_Comunidade == community.Id_Comunidade));
        Assert.True(await Db.Comunidades.IgnoreQueryFilters().AnyAsync(c => c.Id_Comunidade == community.Id_Comunidade));
    }
}

public sealed class PaginasDeErroHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public PaginasDeErroHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "C4 — endereço inexistente mostra a página 404 do EcoRedes, mesmo sem login")]
    public async Task EnderecoInexistente_MostraPagina404()
    {
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var resposta = await client.GetAsync("/Erro/404");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("data-testid=\"pagina-erro\"", await resposta.Content.ReadAsStringAsync());
    }
}
