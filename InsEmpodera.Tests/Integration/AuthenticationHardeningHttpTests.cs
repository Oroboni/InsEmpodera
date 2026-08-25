using System.Net;
using System.Text.RegularExpressions;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class AuthenticationHardeningHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public AuthenticationHardeningHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetLogin_WithCredentialsInQuery_NeverAuthenticates()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var loginPage = await client.GetAsync(
            $"/Account?Email={Uri.EscapeDataString(user.Email)}&Password={Uri.EscapeDataString(HttpFlowTestSupport.ValidPassword)}");

        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/Account", protectedPage.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PostLogin_WithoutOrWithInvalidAntiforgeryToken_IsRejected()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        var credentials = new Dictionary<string, string>
        {
            ["Email"] = user.Email,
            ["Password"] = HttpFlowTestSupport.ValidPassword
        };

        using var withoutToken = await client.PostAsync("/Account", new FormUrlEncodedContent(credentials));
        Assert.Equal(HttpStatusCode.BadRequest, withoutToken.StatusCode);

        credentials["__RequestVerificationToken"] = "token-invalido";
        using var invalidToken = await client.PostAsync("/Account", new FormUrlEncodedContent(credentials));
        Assert.Equal(HttpStatusCode.BadRequest, invalidToken.StatusCode);

        using var protectedPage = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Equal("/Account", protectedPage.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task InvalidLogin_UsesSameGenericErrorForUnknownEmailAndWrongPassword()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var wrongPasswordClient = HttpFlowTestSupport.CreateClient(_factory);
        using var unknownEmailClient = HttpFlowTestSupport.CreateClient(_factory);

        using var wrongPassword = await HttpFlowTestSupport.LoginUsingFormAsync(
            wrongPasswordClient, user.Email, "senha-incorreta");
        using var unknownEmail = await HttpFlowTestSupport.LoginUsingFormAsync(
            unknownEmailClient, $"inexistente-{Guid.NewGuid():N}@test.local", HttpFlowTestSupport.ValidPassword);

        Assert.Equal(HttpStatusCode.OK, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknownEmail.StatusCode);
        var wrongPasswordHtml = await wrongPassword.Content.ReadAsStringAsync();
        var unknownEmailHtml = await unknownEmail.Content.ReadAsStringAsync();
        var wrongPasswordError = ExtractAlert(wrongPasswordHtml);
        var unknownEmailError = ExtractAlert(unknownEmailHtml);

        Assert.Equal("E-mail ou senha inválidos.", wrongPasswordError);
        Assert.Equal(wrongPasswordError, unknownEmailError);
        Assert.DoesNotContain(user.Email, wrongPasswordHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("não encontrado", unknownEmailError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("incorreta", wrongPasswordError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inexistente", unknownEmailError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithLegacyValidHash_AuthenticatesAndUpgradesStoredHash()
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var legacyHasher = new PasswordHasher<Usuario>(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
        }));
        var legacyHash = legacyHasher.HashPassword(user, HttpFlowTestSupport.ValidPassword);
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var persisted = await db.Usuarios.SingleAsync(candidate => candidate.IdUsuario == user.IdUsuario);
            persisted.Senha = legacyHash;
            await db.SaveChangesAsync();
        });

        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);

        HttpFlowTestSupport.AssertRedirect(login, "/");
        var upgradedHash = await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Usuarios
                .Where(candidate => candidate.IdUsuario == user.IdUsuario)
                .Select(candidate => candidate.Senha)
                .SingleAsync());
        Assert.NotEqual(legacyHash, upgradedHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(user, upgradedHash, HttpFlowTestSupport.ValidPassword));
    }

    private static string ExtractAlert(string html)
    {
        var match = Regex.Match(
            html,
            @"<div[^>]*role=""alert""[^>]*>\s*(.*?)\s*</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        Assert.True(match.Success, "A resposta de login não exibiu uma mensagem de erro acessível.");
        return WebUtility.HtmlDecode(Regex.Replace(match.Groups[1].Value, "<[^>]+>", string.Empty)).Trim();
    }
}