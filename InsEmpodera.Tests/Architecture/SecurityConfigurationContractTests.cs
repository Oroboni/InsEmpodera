using System.Text.Json;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class SecurityConfigurationContractTests
{
    [Fact(DisplayName = "Configuração — produção não contém credencial de banco embutida")]
    public void ProductionSettings_DoNotContainEmbeddedDatabaseCredentials()
    {
        var root = FindRepositoryRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "appsettings.json")));
        var json = document.RootElement;

        Assert.False(json.TryGetProperty("ConnectionStrings", out _));
        Assert.NotEqual("*", json.GetProperty("AllowedHosts").GetString());
    }

    [Fact(DisplayName = "Configuração — proteção futura cobre CSRF, Swagger e limites do login")]
    public void Program_ContainsGlobalSecurityGuards()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "Program.cs")) +
                     File.ReadAllText(Path.Combine(
                         root, "Services", "Identity", "IdentityServiceCollectionExtensions.cs"));

        Assert.Contains("AutoValidateAntiforgeryTokenAttribute", source, StringComparison.Ordinal);
        Assert.Contains("UseAuthentication", source, StringComparison.Ordinal);
        Assert.Contains("FallbackPolicy", source, StringComparison.Ordinal);
        Assert.Contains("AddRateLimiter", source, StringComparison.Ordinal);
        Assert.Contains("UseRateLimiter", source, StringComparison.Ordinal);
        Assert.Contains("Content-Security-Policy", source, StringComparison.Ordinal);
        Assert.Contains("app.Environment.IsDevelopment() || app.Environment.IsEnvironment(\"Testing\")", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "InsEmpodera.csproj")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Raiz do projeto não encontrada.");
    }
}
