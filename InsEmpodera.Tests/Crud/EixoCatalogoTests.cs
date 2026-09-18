using Empodera.Data;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Crud;

public sealed class EixoCatalogoTests : ControllerTestBase
{
    [Fact(DisplayName = "Catálogo de eixos — inicialização cria os dez eixos apenas uma vez")]
    public async Task EnsureCreated_SeedsWithoutDuplicating()
    {
        await EixoCatalogo.EnsureCreatedAsync(Db);
        await EixoCatalogo.EnsureCreatedAsync(Db);

        var nomes = await Db.Eixos.Select(e => e.Nome).ToListAsync();
        Assert.Equal(10, nomes.Count);
        Assert.Equal(EixoCatalogo.Nomes.OrderBy(n => n), nomes.OrderBy(n => n));
    }

    [Fact(DisplayName = "Catálogo de eixos — seleção oculta eixos legados sem apagar vínculos")]
    public async Task ListarDisponiveis_HidesLegacyAxisButPreservesDatabaseRow()
    {
        await EixoCatalogo.EnsureCreatedAsync(Db);
        var legacy = await CreateAxisAsync("Eixo legado");

        var options = await EixoCatalogo.ListarDisponiveisAsync(Db);

        Assert.Equal(10, options.Count);
        Assert.DoesNotContain(options, e => e.IdEixo == legacy.IdEixo);
        Assert.True(await Db.Eixos.AnyAsync(e => e.IdEixo == legacy.IdEixo));
    }
}
