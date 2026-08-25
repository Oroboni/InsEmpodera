using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class AccessProfileDeletionHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public AccessProfileDeletionHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Delete_UnusedProfile_RemovesProfileAndPermissions()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var targetId = await SeedProfileAsync(withInactiveUser: false);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/AccessProfile/Delete/{targetId}",
            tokenPage: "/AccessProfile");

        HttpFlowTestSupport.AssertRedirect(response, "/AccessProfile");
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            Assert.False(await db.Perfis.AnyAsync(profile => profile.IdPerfil == targetId));
            Assert.False(await db.Permissoes.AnyAsync(permission => permission.FkIdPerfil == targetId));
        });
    }

    [Fact]
    public async Task Delete_ProfileUsedOnlyByInactiveUser_IsBlockedAndShowsClearError()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var targetId = await SeedProfileAsync(withInactiveUser: true);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/AccessProfile/Delete/{targetId}",
            tokenPage: "/AccessProfile");
        HttpFlowTestSupport.AssertRedirect(response, "/AccessProfile");

        Assert.True(await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Perfis.AnyAsync(profile => profile.IdPerfil == targetId)));
        using var index = await client.GetAsync("/AccessProfile");
        Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        Assert.Contains(
            "inclusive inativos",
            await index.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Delete_OwnLoggedProfile_IsBlocked()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/AccessProfile/Delete/1",
            tokenPage: "/AccessProfile");
        HttpFlowTestSupport.AssertRedirect(response, "/AccessProfile");

        Assert.True(await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Perfis.AnyAsync(profile => profile.IdPerfil == 1)));
        using var index = await client.GetAsync("/AccessProfile");
        Assert.Contains(
            @"role=""alert""",
            await index.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Delete_UserWithoutProfileDeletePermission_CannotMutate()
    {
        var editor = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 2);
        var targetId = await SeedProfileAsync(withInactiveUser: false);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, editor.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/AccessProfile/Delete/{targetId}",
            tokenPage: "/Account");

        HttpFlowTestSupport.AssertRedirect(response, "/AccessProfile");
        Assert.True(await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Perfis.AnyAsync(profile => profile.IdPerfil == targetId)));
    }

    [Fact]
    public async Task Delete_WithoutAntiforgeryToken_IsRejectedAndViewOffersButtonOnlyForSafeProfile()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var unusedId = await SeedProfileAsync(withInactiveUser: false);
        var usedId = await SeedProfileAsync(withInactiveUser: true);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var index = await client.GetAsync("/AccessProfile");
        Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        var html = await index.Content.ReadAsStringAsync();
        Assert.Contains($"/AccessProfile/Delete/{unusedId}", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"/AccessProfile/Delete/{usedId}", html, StringComparison.Ordinal);

        using var response = await client.PostAsync(
            $"/AccessProfile/Delete/{unusedId}",
            new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Perfis.AnyAsync(profile => profile.IdPerfil == unusedId)));
    }

    private async Task<int> SeedProfileAsync(bool withInactiveUser) =>
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var profile = new Perfil
            {
                Nome = $"Perfil exclusão {Guid.NewGuid():N}",
                FkIdUsuario = 1,
                DtCriacao = DateTime.UtcNow,
                DtModificacao = DateTime.UtcNow,
                Permissoes = new List<Permissoes>
                {
                    new()
                    {
                        Modulo = "SER",
                        PodeListar = "N",
                        PodeDetalhar = "N",
                        PodeCriar = "N",
                        PodeAtualizar = "N",
                        PodeDeletar = "N"
                    }
                }
            };
            db.Perfis.Add(profile);
            await db.SaveChangesAsync();

            if (withInactiveUser)
            {
                var user = new Usuario
                {
                    Nome = "Usuário inativo associado",
                    Email = $"profile-use-{Guid.NewGuid():N}@test.local",
                    Senha = "Perfil@123",
                    Ocupacao = "Teste",
                    DtNascimento = new DateTime(1990, 1, 1),
                    DtCriacao = DateTime.UtcNow,
                    DtAtualizacao = DateTime.UtcNow,
                    FkIdPerfil = profile.IdPerfil,
                    Ativo = "N"
                };
                user.Senha = new PasswordHasher<Usuario>().HashPassword(user, user.Senha);
                db.Usuarios.Add(user);
                await db.SaveChangesAsync();
            }

            return profile.IdPerfil;
        });
}
