using System.Net;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class IdentityAuthenticationHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public IdentityAuthenticationHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "Identity — cinco senhas incorretas bloqueiam a conta por quinze minutos")]
    public async Task RepeatedWrongPasswords_LockTheAccount()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var rejected = await HttpFlowTestSupport.LoginUsingFormAsync(
                client, user.Email, $"Incorreta@{attempt}9");
            Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        }

        using var correctPasswordWhileLocked = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);
        Assert.Equal(HttpStatusCode.OK, correctPasswordWhileLocked.StatusCode);
        Assert.Contains(
            "temporariamente bloqueado",
            await correctPasswordWhileLocked.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);

        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var persisted = await db.Usuarios.SingleAsync(item => item.IdUsuario == user.IdUsuario);
            Assert.True(persisted.LockoutEnd > DateTimeOffset.UtcNow);
            // Identity zera o contador quando transforma as falhas em um lockout.
            Assert.Equal(0, persisted.AccessFailedCount);
        });
    }

    [Fact(DisplayName = "Identity — alterar o security stamp revoga imediatamente o cookie emitido")]
    public async Task SecurityStampChange_RevokesExistingCookie()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var persisted = await db.Usuarios.SingleAsync(item => item.IdUsuario == user.IdUsuario);
            persisted.SecurityStamp = Guid.NewGuid().ToString("N");
            await db.SaveChangesAsync();
        });

        using var protectedPage = await client.GetAsync("/Users");
        Assert.Equal(HttpStatusCode.Forbidden, protectedPage.StatusCode);
    }

    [Fact(DisplayName = "Identity — UserManager cria usuário com chave inteira aleatória")]
    public async Task UserManager_CreatesRandomIntegerUserId()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var email = $"identity-{Guid.NewGuid():N}@test.local";
        var user = new Usuario
        {
            Nome = "Usuário Identity",
            Email = email,
            UserName = email,
            Ocupacao = "Teste",
            DtNascimento = new DateTime(1990, 1, 1),
            DtCriacao = DateTime.UtcNow,
            Ativo = "S",
            FkIdPerfil = 3,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Identity@123");

        Assert.True(result.Succeeded, string.Join(" ", result.Errors.Select(error => error.Description)));
        Assert.InRange(user.IdUsuario, 100_000, 999_999);
        Assert.Equal(email.ToUpperInvariant(), user.NormalizedEmail);
        Assert.Equal(email.ToUpperInvariant(), user.NormalizedUserName);
    }

    [Fact(DisplayName = "Autorização — usuário não administrador não pode promover uma conta para outro perfil")]
    public async Task NonAdminUser_CannotAssignAnotherProfile()
    {
        var editor = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 2);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, editor.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/Users/Edit/{editor.IdUsuario}",
            new Dictionary<string, string>
            {
                ["IdUsuario"] = editor.IdUsuario.ToString(),
                ["Nome"] = editor.Nome,
                ["Email"] = editor.Email,
                ["Senha"] = string.Empty,
                ["FkIdPerfil"] = "1",
                ["Ativo"] = "S",
                ["Ocupacao"] = editor.Ocupacao,
                ["Genero"] = "1"
            },
            tokenPage: $"/Users/Edit/{editor.IdUsuario}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
            Assert.Equal(2, (await db.Usuarios.SingleAsync(item => item.IdUsuario == editor.IdUsuario)).FkIdPerfil));
    }
}
