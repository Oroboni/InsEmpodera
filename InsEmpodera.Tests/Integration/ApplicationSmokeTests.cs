using Xunit;
using System.Net;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsEmpodera.Tests.Integration;

public sealed class ApplicationSmokeTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private const string TestPassword = "Teste@123";
    private readonly EmpoderaWebApplicationFactory _factory;

    public ApplicationSmokeTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PublicPagesAndLanguageApi_AreAvailable()
    {
        using var client = CreateClient();
        foreach (var path in new[] { "/Account", "/Account/Forgot", "/Language/Catalog?page=login" })
        {
            using var response = await client.GetAsync(path);
            Assert.True(response.IsSuccessStatusCode, $"GET {path} returned {(int)response.StatusCode}.");
        }
    }

    [Fact]
    public async Task HomeWithoutSession_RedirectsToLogin()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task ValidLogin_OpensMainApplicationPages()
    {
        await SetKnownPasswordAsync();
        using var client = CreateClient();
        using var login = await client.GetAsync($"/Account?Email={Uri.EscapeDataString("joao@email.com")}&Password={Uri.EscapeDataString(TestPassword)}");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/", login.Headers.Location?.OriginalString);

        var mainPages = new[]
        {
            "/", "/Users", "/AccessProfile", "/Comunidade", "/Atores", "/Atividades",
            "/DiarioCampo", "/FichaPrimeiroContato", "/PersonalAssessment", "/PersonalProcess",
            "/Relatorios", "/Report"
        };
        foreach (var path in mainPages)
        {
            using var response = await client.GetAsync(path);
            Assert.True(response.IsSuccessStatusCode, $"GET {path} returned {(int)response.StatusCode}.");
        }
    }

    [Theory]
    [InlineData("pt-BR,pt;q=0.9", "pt-BR")]
    [InlineData("en-US,en;q=0.9", "en")]
    [InlineData("es-ES,es;q=0.9", "es")]
    public async Task LoginPage_AcceptsSupportedBrowserLanguages(string acceptLanguage, string expectedLanguage)
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLanguage);
        using var response = await client.GetAsync("/Account");
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains($"lang=\"{expectedLanguage}\"", html, StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true,
        BaseAddress = new Uri("https://localhost")
    });

    private async Task SetKnownPasswordAsync()
    {
        using var client = CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Usuarios.SingleAsync(item => item.Email == "joao@email.com");
        user.Senha = new PasswordHasher<Usuario>().HashPassword(user, TestPassword);
        await db.SaveChangesAsync();
    }
}
