using System.Net;
using System.Text.Json;
using Empodera.Services.Email;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class PasswordRecoveryIdentityHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private const string GenericResponse =
        "Se existir uma conta associada ao e-mail informado, as instruções serão enviadas em instantes.";
    private readonly EmpoderaWebApplicationFactory _factory;

    public PasswordRecoveryIdentityHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "Recuperação — conta existente e inexistente recebem a mesma resposta pública")]
    public async Task ForgotPassword_DoesNotRevealWhetherAccountExists()
    {
        _factory.PasswordResetEmailSender.Clear();
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var existing = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Account/ForgotPassword",
            new Dictionary<string, string> { ["Email"] = user.Email },
            "/Account/Forgot");
        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Contains(GenericResponse, await existing.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var captured = Assert.Single(_factory.PasswordResetEmailSender.Messages);
        Assert.Equal(user.Email, captured.RecipientEmail);
        Assert.StartsWith("https://localhost/Account/ResetPassword?", captured.ResetUrl, StringComparison.Ordinal);

        using var missing = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Account/ForgotPassword",
            new Dictionary<string, string> { ["Email"] = $"missing-{Guid.NewGuid():N}@test.local" },
            "/Account/Forgot");
        Assert.Equal(HttpStatusCode.OK, missing.StatusCode);
        Assert.Contains(GenericResponse, await missing.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Single(_factory.PasswordResetEmailSender.Messages);
    }

    [Fact(DisplayName = "Recuperação — link Identity troca a senha, encerra acessos antigos e não pode ser reutilizado")]
    public async Task ResetPassword_UsesOneTimeIdentityTokenAndChangesCredentials()
    {
        _factory.PasswordResetEmailSender.Clear();
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using (var request = await HttpFlowTestSupport.PostFormAsync(
                   client,
                   "/Account/ForgotPassword",
                   new Dictionary<string, string> { ["Email"] = user.Email },
                   "/Account/Forgot"))
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);

        var resetMessage = Assert.Single(_factory.PasswordResetEmailSender.Messages);
        var resetUri = new Uri(resetMessage.ResetUrl);
        var query = QueryHelpers.ParseQuery(resetUri.Query);
        var code = Assert.Single(query["code"]);
        var email = Assert.Single(query["email"]);
        var resetPath = resetUri.PathAndQuery;
        const string newPassword = "NovaSenha@456";

        using var reset = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/Account/ResetPassword",
            new Dictionary<string, string>
            {
                ["Email"] = email!,
                ["Code"] = code!,
                ["Password"] = newPassword,
                ["ConfirmPassword"] = newPassword
            },
            resetPath);
        HttpFlowTestSupport.AssertRedirect(reset, "/Account/ResetPasswordConfirmation");

        using var oldPassword = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);
        Assert.Equal(HttpStatusCode.OK, oldPassword.StatusCode);

        using var newPasswordLogin = await HttpFlowTestSupport.LoginUsingFormAsync(client, user.Email, newPassword);
        HttpFlowTestSupport.AssertRedirect(newPasswordLogin, "/");

        using var secondClient = HttpFlowTestSupport.CreateClient(_factory);
        using var reused = await HttpFlowTestSupport.PostFormAsync(
            secondClient,
            "/Account/ResetPassword",
            new Dictionary<string, string>
            {
                ["Email"] = email!,
                ["Code"] = code!,
                ["Password"] = "OutraSenha@789",
                ["ConfirmPassword"] = "OutraSenha@789"
            },
            resetPath);
        Assert.Equal(HttpStatusCode.OK, reused.StatusCode);
        Assert.Contains(
            "inválido, já foi utilizado ou expirou",
            WebUtility.HtmlDecode(await reused.Content.ReadAsStringAsync()));
    }

    [Fact(DisplayName = "SMTP — Gmail oficial usa TLS e nenhuma senha é armazenada na configuração")]
    public void GmailSmtp_UsesOfficialAccountAndNoStoredPassword()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            root, "Services", "Email", "GmailSmtpPasswordResetEmailSender.cs"));
        var optionsSource = File.ReadAllText(Path.Combine(
            root, "Services", "Email", "GmailSmtpOptions.cs"));
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "appsettings.json")));
        var email = settings.RootElement.GetProperty("Email");

        Assert.Equal("empodera.ajuda@gmail.com", email.GetProperty("User").GetString());
        Assert.False(email.TryGetProperty("Password", out _));
        Assert.Contains("smtp.gmail.com", optionsSource, StringComparison.Ordinal);
        Assert.Contains("GmailStartTlsPort = 587", optionsSource, StringComparison.Ordinal);
        Assert.Contains("EnableSsl = true", source, StringComparison.Ordinal);
        Assert.Contains("NetworkCredential", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "InsEmpodera.csproj")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Raiz do projeto não encontrada.");
    }
}
