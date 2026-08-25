using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class SensitiveLoggingConfigurationTests
{
    [Fact]
    public void SensitiveEntityValues_AreLoggedOnlyInDevelopmentOrTesting()
    {
        var program = File.ReadAllText(RepositoryPath("Program.cs"));
        var enableIndex = program.IndexOf("options.EnableSensitiveDataLogging();", StringComparison.Ordinal);
        Assert.True(enableIndex >= 0, "A configuração explícita de log sensível não foi encontrada.");

        var guardStart = Math.Max(0, enableIndex - 220);
        var guard = program[guardStart..enableIndex];
        Assert.Contains("builder.Environment.IsDevelopment()", guard, StringComparison.Ordinal);
        Assert.Contains(@"builder.Environment.IsEnvironment(""Testing"")", guard, StringComparison.Ordinal);
        Assert.DoesNotContain("IsProduction()", guard, StringComparison.Ordinal);
    }

    private static string RepositoryPath(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "InsEmpodera.csproj")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return segments.Aggregate(directory!.FullName, Path.Combine);
    }
}
