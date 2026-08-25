using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class AuthenticationBoundaryHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public AuthenticationBoundaryHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_NormalizesEmailButNeverTrimsPassword()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var normalizedClient = HttpFlowTestSupport.CreateClient(_factory);
        using var passwordWithSpacesClient = HttpFlowTestSupport.CreateClient(_factory);

        using var normalized = await HttpFlowTestSupport.LoginUsingFormAsync(
            normalizedClient,
            $"  {user.Email.ToUpperInvariant()}  ",
            HttpFlowTestSupport.ValidPassword);
        HttpFlowTestSupport.AssertRedirect(normalized, "/");

        using var passwordWithSpaces = await HttpFlowTestSupport.LoginUsingFormAsync(
            passwordWithSpacesClient,
            user.Email,
            $" {HttpFlowTestSupport.ValidPassword} ");
        Assert.Equal(HttpStatusCode.OK, passwordWithSpaces.StatusCode);
        using var protectedPage = await passwordWithSpacesClient.GetAsync("/");
        HttpFlowTestSupport.AssertRedirect(protectedPage, "/Account");
    }

    [Theory]
    [InlineData("")]
    [InlineData("X")]
    [InlineData("n")]
    [InlineData("s")]
    public async Task Login_RejectsEveryStatusOtherThanCanonicalActive(string? status)
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var persisted = await db.Usuarios.SingleAsync(candidate => candidate.IdUsuario == user.IdUsuario);
            persisted.Ativo = status!;
            await db.SaveChangesAsync();
        });
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var protectedPage = await client.GetAsync("/");
        HttpFlowTestSupport.AssertRedirect(protectedPage, "/Account");
    }

    [Fact]
    public async Task ActiveStatus_IsRequiredByTheDatabase()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
            {
                var persisted = await db.Usuarios.SingleAsync(candidate => candidate.IdUsuario == user.IdUsuario);
                persisted.Ativo = null!;
                await db.SaveChangesAsync();
            }));
    }

    [Fact]
    public async Task SessionCookie_UsesExplicitSecurityFlags()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);

        HttpFlowTestSupport.AssertRedirect(login, "/");
        var sessionCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"), value =>
            value.StartsWith(".AspNetCore.Session=", StringComparison.Ordinal));
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", sessionCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetLogout_IsRejectedAndDoesNotClearSession()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var logout = await client.GetAsync("/Account/Logout");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, logout.StatusCode);
        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, protectedPage.StatusCode);
    }

    [Fact]
    public async Task PostLogout_WithoutAntiforgeryToken_IsRejectedAndDoesNotClearSession()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var logout = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent([]));

        Assert.Equal(HttpStatusCode.BadRequest, logout.StatusCode);
        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, protectedPage.StatusCode);
    }

    [Fact]
    public async Task PostLogout_WithValidToken_ClearsSessionButPreservesCulture()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(
            _factory, profileId: 1, preferredLanguage: IdiomaPreferido.Ingles);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var logout = await HttpFlowTestSupport.PostFormAsync(
            client, "/Account/Logout", tokenPage: "/");

        HttpFlowTestSupport.AssertRedirect(logout, "/Account");
        Assert.DoesNotContain(logout.Headers.TryGetValues("Set-Cookie", out var cookies)
                ? cookies
                : Array.Empty<string>(),
            value => value.StartsWith(".AspNetCore.Culture=", StringComparison.Ordinal));
        using var protectedPage = await client.GetAsync("/");
        HttpFlowTestSupport.AssertRedirect(protectedPage, "/Account");

        using var loginPage = await client.GetAsync("/Account");
        Assert.Contains("lang=\"en\"", await loginPage.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }
}
