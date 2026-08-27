using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class AuthenticationSessionFlowTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public AuthenticationSessionFlowTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task LoginForm_WithValidCredentials_EstablishesIdentityAndUsesBrowserCulture()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(
            _factory, profileId: 1, preferredLanguage: IdiomaPreferido.Ingles);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9");

        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);

        HttpFlowTestSupport.AssertRedirect(login, "/");
        Assert.Contains(login.Headers.GetValues("Set-Cookie"), value =>
            value.StartsWith(".Empodera.Identity=", StringComparison.Ordinal));
        Assert.Contains(login.Headers.GetValues("Set-Cookie"), value =>
            value.StartsWith(".Empodera.LanguageMode=browser", StringComparison.Ordinal));

        using var home = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains("lang=\"pt-BR\"", await home.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginForm_WithWrongPassword_DoesNotCreateAuthenticatedSession()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, user.Email, "senha-incorreta");

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("login-form", await login.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/Account", protectedPage.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LoginForm_InactiveUserIsRejectedEvenWithCorrectPassword()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1, active: "N");
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var protectedPage = await client.GetAsync("/Users");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/Account", protectedPage.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_ClearsSessionButPreservesLanguagePreference()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(
            _factory, profileId: 1, preferredLanguage: IdiomaPreferido.Espanhol);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var logout = await HttpFlowTestSupport.PostFormAsync(client, "/Account/Logout", tokenPage: "/");

        HttpFlowTestSupport.AssertRedirect(logout, "/Account");
        Assert.DoesNotContain(logout.Headers.TryGetValues("Set-Cookie", out var cookies)
                ? cookies
                : Array.Empty<string>(),
            value => value.StartsWith(".AspNetCore.Culture=", StringComparison.Ordinal));
        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/Account", protectedPage.Headers.Location?.OriginalString);

        using var loginPage = await client.GetAsync("/Account");
        Assert.Contains("lang=\"es\"", await loginPage.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }
}
