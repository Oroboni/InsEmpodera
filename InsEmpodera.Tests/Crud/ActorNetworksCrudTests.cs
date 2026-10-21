using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InsEmpodera.Tests.Crud;

// Redes Primária/Subjetiva: com chaves AUTO_INCREMENT o Id de um ator novo só existe depois de salvo,
// então os vínculos com o ator principal novo e com o ator relacionado criado na hora usam as entidades.
public sealed class ActorNetworksCrudTests : ControllerTestBase
{
    [Fact(DisplayName = "Redes — novo ator grava vínculos com ator existente e com ator criado na hora")]
    public async Task Create_LinksExistingAndNewlyCreatedActors()
    {
        var community = await CreateCommunityAsync();
        var existing = await CreateActorAsync(community, "Maria Existente");
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));

        var result = await controller.Create(
            new Atores { Nome = "Ator Principal" }, community.Id_Comunidade,
            vinculosRede:
            [
                new RedeVinculoInput { TipoRede = TiposRede.Primaria, AtorId = existing.IdAtores, TipoRelacao = "Irmã" },
                new RedeVinculoInput { TipoRede = TiposRede.Subjetiva, Nome = "Vizinho Novo", ComunidadeId = community.Id_Comunidade, TipoRelacao = "Confiança" },
                new RedeVinculoInput { TipoRede = TiposRede.Primaria, Nome = "Vizinho Novo", ComunidadeId = community.Id_Comunidade, TipoRelacao = "Vizinho" }
            ],
            redesEnviadas: true);

        Assert.IsType<RedirectToActionResult>(result);
        var principal = await Db.Atores.SingleAsync(a => a.Nome == "Ator Principal");
        var novo = await Db.Atores.Include(a => a.Comunidades).SingleAsync(a => a.Nome == "Vizinho Novo");
        Assert.Equal(community.Id_Comunidade, Assert.Single(novo.Comunidades).FkIdComunidade);

        var redes = await Db.RedesPrimarias.Where(r => r.FkIdAtorPrincipal == principal.IdAtores)
            .OrderBy(r => r.TipoRelacao).ToListAsync();
        Assert.Equal(3, redes.Count);
        Assert.Equal((existing.IdAtores, TiposRede.Primaria), (redes.Single(r => r.TipoRelacao == "Irmã").FkIdAtorRelacionados, TiposRede.Primaria));
        Assert.All(redes.Where(r => r.TipoRelacao != "Irmã"), r => Assert.Equal(novo.IdAtores, r.FkIdAtorRelacionados));
    }

    [Fact(DisplayName = "Redes — edição substitui os vínculos e ignora vínculo do ator com ele mesmo")]
    public async Task Edit_ReplacesLinksAndIgnoresSelfLink()
    {
        var community = await CreateCommunityAsync();
        var principal = await CreateActorAsync(community, "Principal Edição");
        var other = await CreateActorAsync(community, "Outra Pessoa");
        Db.RedesPrimarias.Add(new RedePrimaria
        {
            FkIdAtorPrincipal = principal.IdAtores, FkIdAtorRelacionados = other.IdAtores,
            TipoRede = TiposRede.Primaria, TipoRelacao = "Antigo"
        });
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        var controller = Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db));

        var result = await controller.Edit(
            new Atores { IdAtores = principal.IdAtores, Nome = principal.Nome }, principal.IdAtores, community.Id_Comunidade,
            vinculosRede:
            [
                new RedeVinculoInput { TipoRede = TiposRede.Subjetiva, AtorId = other.IdAtores, TipoRelacao = "Novo" },
                new RedeVinculoInput { TipoRede = TiposRede.Primaria, AtorId = principal.IdAtores, TipoRelacao = "Ele mesmo" }
            ],
            redesEnviadas: true);

        Assert.IsType<RedirectToActionResult>(result);
        var rede = Assert.Single(await Db.RedesPrimarias.AsNoTracking().Where(r => r.FkIdAtorPrincipal == principal.IdAtores).ToListAsync());
        Assert.Equal(("Novo", TiposRede.Subjetiva, other.IdAtores), (rede.TipoRelacao, rede.TipoRede, rede.FkIdAtorRelacionados));
    }
}
