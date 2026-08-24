using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Controllers;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class CrudAuthorizationTests : ControllerTestBase
{
    [Fact]
    public async Task ReadOnlyProfile_CannotDeleteAnyCrudRecord()
    {
        var community = await CreateCommunityAsync();
        var actor = await CreateActorAsync(community);
        var axis = await CreateAxisAsync();
        var activity = new Atividades
        {
            Nome = "Protegida", Descricao = "Protegida", FkIdComunidade = community.Id_Comunidade,
            FkIdUsuario = 1, DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var diary = new DiarioCampo
        {
            FkIdComunidade = community.Id_Comunidade, FkIdUsuario = 1, Data = DateTime.UtcNow,
            Descricao = "Protegido", Localizacao = "Local", Foto = "foto.png",
            DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var assessment = new AvaliacaoPessoal
        {
            FK_id_Atores = actor.IdAtores, FkIdUsuario = 1, DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var form = new FichaPrimeiroContato
        {
            FK_id_Atores = actor.IdAtores, FkIdComunidade = community.Id_Comunidade, FkIdUsuario = 1,
            DtContato = DateTime.UtcNow, DtCriacao = DateTime.UtcNow, DtModificacao = DateTime.UtcNow
        };
        var user = new Usuario
        {
            Nome = "Protegido", Email = $"protected-{Guid.NewGuid():N}@test.local", Ocupacao = "Teste",
            DtNascimento = new DateTime(1990, 1, 1), DtCriacao = DateTime.UtcNow, Ativo = "S", FkIdPerfil = 1,
            Senha = "temporária"
        };
        user.Senha = new PasswordHasher<Usuario>().HashPassword(user, user.Senha);
        Db.AddRange(activity, diary, assessment, form, user);
        await Db.SaveChangesAsync();
        Db.AtividadesEixo.Add(new AtividadesEixo { FkIdAtividade = activity.IdAtividade, FkIdEixo = axis.IdEixo });
        await Db.SaveChangesAsync();

        Assert.IsType<RedirectToActionResult>(await Attach(new UsersController(NullLogger<UsersController>.Instance, Db), 4).Delete(user.IdUsuario));
        Assert.IsType<RedirectToActionResult>(await Attach(new ComunidadeController(NullLogger<ComunidadeController>.Instance, Db), 4).Delete(community.Id_Comunidade));
        Assert.IsType<RedirectToActionResult>(await Attach(new AtoresController(NullLogger<AtoresController>.Instance, Db), 4).Delete(actor.IdAtores));
        Assert.IsType<RedirectToActionResult>(await Attach(new AtividadesController(Db), 4).Delete(activity.IdAtividade));
        Assert.IsType<RedirectToActionResult>(await Attach(new DiarioCampoController(Db), 4).DeleteConfirmed(diary.IdDCampo));
        Assert.IsType<RedirectToActionResult>(await Attach(new PersonalAssessmentController(Db), 4).Delete(assessment.IdAvaliacao));
        Assert.IsType<RedirectToActionResult>(await Attach(new FichaPrimeiroContatoController(Db), 4).DeleteConfirmed(form.IdFicha));

        Assert.Equal("S", (await Db.Usuarios.FindAsync(user.IdUsuario))!.Ativo);
        Assert.Equal("S", (await Db.Comunidades.FindAsync(community.Id_Comunidade))!.Ativo);
        Assert.Equal("S", (await Db.Atores.FindAsync(actor.IdAtores))!.Ativo);
        Assert.NotNull(await Db.Atividades.FindAsync(activity.IdAtividade));
        Assert.NotNull(await Db.DiariosCampo.FindAsync(diary.IdDCampo));
        Assert.NotNull(await Db.AvaliacaoPessoal.FindAsync(assessment.IdAvaliacao));
        Assert.NotNull(await Db.FichasPrimeiroContato.FindAsync(form.IdFicha));
    }

    [Fact]
    public async Task LoggedUser_CannotDeactivateOwnAccount()
    {
        var controller = Attach(new UsersController(NullLogger<UsersController>.Instance, Db), 1);
        var result = await controller.Delete(1);
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("S", (await Db.Usuarios.FindAsync(1))!.Ativo);
    }

    [Fact]
    public async Task DeleteEndpoints_MissingIdsDoNotSucceedSilently()
    {
        Assert.IsType<NotFoundResult>(await Attach(new UsersController(NullLogger<UsersController>.Instance, Db)).Delete(999_999));
        Assert.IsType<NotFoundResult>(await Attach(new AtividadesController(Db)).Delete(999_999));
        Assert.IsType<NotFoundResult>(await Attach(new PersonalAssessmentController(Db)).Delete(999_999));
        Assert.IsType<NotFoundResult>(await Attach(new DiarioCampoController(Db)).Delete(999_999));
        Assert.IsType<NotFoundResult>(await Attach(new FichaPrimeiroContatoController(Db)).Delete(999_999));
    }
}
