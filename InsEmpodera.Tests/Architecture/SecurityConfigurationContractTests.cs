using System.Text.Json;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class SecurityConfigurationContractTests
{
    [Fact(DisplayName = "Configuração — nenhum appsettings versionado contém conexão de banco")]
    public void SourceSettings_DoNotDeclareDatabaseConnectionStrings()
    {
        var root = FindRepositoryRoot();
        var settingsFiles = Directory
            .EnumerateFiles(root, "appsettings*.json", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(root, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.NotEmpty(settingsFiles);

        foreach (var settingsFile in settingsFiles)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(settingsFile));
            var relativePath = Path.GetRelativePath(root, settingsFile);

            Assert.False(
                document.RootElement.TryGetProperty("ConnectionStrings", out _),
                $"O arquivo '{relativePath}' contém ConnectionStrings. " +
                "Forneça a conexão por ConnectionStrings__DefaultConnection usando variável de ambiente ou User Secrets.");
        }
    }

    [Fact(DisplayName = "Configuração — produção restringe os hosts aceitos")]
    public void ProductionSettings_RestrictsAllowedHosts()
    {
        var root = FindRepositoryRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "appsettings.json")));
        var json = document.RootElement;

        Assert.True(
            json.TryGetProperty("AllowedHosts", out var allowedHosts),
            "appsettings.json deve declarar AllowedHosts explicitamente.");

        Assert.False(
            string.IsNullOrWhiteSpace(allowedHosts.GetString()) || allowedHosts.GetString() == "*",
            "AllowedHosts não pode ser vazio nem '*'. Use uma lista de hosts separados por ponto e vírgula.");
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

    private static bool IsGeneratedPath(string root, string path)
    {
        var relativePath = Path.GetRelativePath(root, path);
        var segments = relativePath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        return segments.Any(segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(".git", StringComparison.OrdinalIgnoreCase));
    }
}
