using System.Net;
using System.Text.RegularExpressions;
using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InsEmpodera.Tests.Infrastructure;

internal static class HttpFlowTestSupport
{
    internal const string ValidPassword = "Fluxo@123";

    internal static HttpClient CreateClient(EmpoderaWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });

    internal static async Task<Usuario> SeedUserAsync(
        EmpoderaWebApplicationFactory factory,
        int profileId,
        string active = "S",
        IdiomaPreferido preferredLanguage = IdiomaPreferido.Default)
    {
        return await InDatabaseAsync(factory, async db =>
        {
            var user = new Usuario
            {
                Nome = $"Usuário de fluxo {Guid.NewGuid():N}",
                Email = $"flow-{Guid.NewGuid():N}@test.local",
                Senha = ValidPassword,
                Ocupacao = "Teste automatizado",
                Genero = 1,
                DtNascimento = new DateTime(1990, 1, 1),
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow,
                FkIdPerfil = profileId,
                Ativo = active,
                IdiomaPreferido = preferredLanguage
            };
            user.Senha = new PasswordHasher<Usuario>().HashPassword(user, ValidPassword);
            db.Usuarios.Add(user);
            await db.SaveChangesAsync();
            return user;
        });
    }

    internal static async Task<HttpResponseMessage> LoginUsingFormAsync(
        HttpClient client,
        string email,
        string password)
    {
        var token = await GetAntiforgeryTokenAsync(client, "/Account");
        return await client.PostAsync("/Account", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        }));
    }

    internal static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string path,
        IReadOnlyDictionary<string, string>? fields = null,
        string tokenPage = "/Account")
    {
        var token = await GetAntiforgeryTokenAsync(client, tokenPage);
        var values = fields?.Select(item => new KeyValuePair<string, string>(item.Key, item.Value)).ToList()
            ?? new List<KeyValuePair<string, string>>();
        values.Add(new KeyValuePair<string, string>("__RequestVerificationToken", token));
        return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    internal static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        using var page = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"A página {path} não renderizou token antifalsificação.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    internal static async Task<T> InDatabaseAsync<T>(
        EmpoderaWebApplicationFactory factory,
        Func<ApplicationDbContext, Task<T>> operation)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await operation(db);
    }

    internal static async Task InDatabaseAsync(
        EmpoderaWebApplicationFactory factory,
        Func<ApplicationDbContext, Task> operation)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await operation(db);
    }

    internal static void AssertRedirect(HttpResponseMessage response, string? expectedPath = null)
    {
        Assert.True((int)response.StatusCode is >= 300 and < 400,
            $"Era esperado redirecionamento, mas foi recebido {(int)response.StatusCode} ({response.ReasonPhrase}).");
        if (expectedPath is not null)
            Assert.Equal(expectedPath, response.Headers.Location?.OriginalString);
    }
}
