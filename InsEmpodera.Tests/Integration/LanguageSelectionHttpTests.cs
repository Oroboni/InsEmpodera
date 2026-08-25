using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class LanguageSelectionHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public LanguageSelectionHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task LoginLanguageSelector_IsAccessibleAndOffersAllSupportedModes()
    {
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using var response = await client.GetAsync("/Account");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("login-language-switcher", html);
        Assert.Contains("aria-label=\"Selecionar idioma\"", html);
        Assert.Contains("value=\"auto\"", html);
        Assert.Contains("value=\"pt-BR\"", html);
        Assert.Contains("value=\"en\"", html);
        Assert.Contains("value=\"es\"", html);
    }

    [Fact]
    public async Task ExplicitLoginSelection_OverridesBrowserAndSurvivesLoginAndLogout()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(
            _factory, profileId: 1, preferredLanguage: IdiomaPreferido.Portugues);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9");

        using (var selected = await HttpFlowTestSupport.PostFormAsync(
                   client,
                   "/Language/Set",
                   new Dictionary<string, string> { ["culture"] = "en", ["returnUrl"] = "/Account" }))
            HttpFlowTestSupport.AssertRedirect(selected, "/Account");

        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using (var home = await client.GetAsync("/"))
            Assert.Contains("lang=\"en\"", await home.Content.ReadAsStringAsync(),
                StringComparison.OrdinalIgnoreCase);

        using (var logout = await HttpFlowTestSupport.PostFormAsync(client, "/Account/Logout", tokenPage: "/"))
            HttpFlowTestSupport.AssertRedirect(logout, "/Account");

        using var loginPage = await client.GetAsync("/Account");
        Assert.Contains("lang=\"en\"", await loginPage.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AutomaticSelection_ReturnsToBrowserLanguage()
    {
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");

        using (var english = await HttpFlowTestSupport.PostFormAsync(
                   client,
                   "/Language/Set",
                   new Dictionary<string, string> { ["culture"] = "en", ["returnUrl"] = "/Account" }))
            HttpFlowTestSupport.AssertRedirect(english, "/Account");

        using (var automatic = await HttpFlowTestSupport.PostFormAsync(
                   client,
                   "/Language/Set",
                   new Dictionary<string, string> { ["culture"] = "auto", ["returnUrl"] = "/Account" }))
            HttpFlowTestSupport.AssertRedirect(automatic, "/Account");

        using var page = await client.GetAsync("/Account");
        Assert.Contains("lang=\"es\"", await page.Content.ReadAsStringAsync(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Selection_RejectsMissingTokenUnsupportedCultureAndExternalRedirect()
    {
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using var noToken = await client.PostAsync("/Language/Set", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["culture"] = "en" }));
        Assert.Equal(HttpStatusCode.BadRequest, noToken.StatusCode);

        using var unsupported = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Language/Set",
            new Dictionary<string, string> { ["culture"] = "xx", ["returnUrl"] = "/Account" });
        Assert.Equal(HttpStatusCode.BadRequest, unsupported.StatusCode);

        using var external = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Language/Set",
            new Dictionary<string, string>
            {
                ["culture"] = "en",
                ["returnUrl"] = "https://example.invalid/phishing"
            });
        HttpFlowTestSupport.AssertRedirect(external, "/Account");
    }
}
