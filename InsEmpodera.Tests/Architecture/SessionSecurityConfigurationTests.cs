using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class SessionSecurityConfigurationTests
{
    [Fact]
    public void IdentityCookie_IsExplicitlyHardenedAndProductionForcesSecureTransport()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "Program.cs")) +
                     File.ReadAllText(Path.Combine(
                         root, "Services", "Identity", "IdentityServiceCollectionExtensions.cs"));

        Assert.Contains("options.Cookie.HttpOnly = true", source, StringComparison.Ordinal);
        Assert.Contains("options.Cookie.SameSite = SameSiteMode.Lax", source, StringComparison.Ordinal);
        Assert.Contains("options.Cookie.IsEssential = true", source, StringComparison.Ordinal);
        Assert.Contains("CookieSecurePolicy.Always", source, StringComparison.Ordinal);
        Assert.Contains("builder.Environment.IsDevelopment()", source, StringComparison.Ordinal);
        Assert.Contains("builder.Environment.IsEnvironment(\"Testing\")", source, StringComparison.Ordinal);
        Assert.Contains("options.ExpireTimeSpan = TimeSpan.FromMinutes(30)", source, StringComparison.Ordinal);
        Assert.Contains("options.SlidingExpiration = true", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "InsEmpodera.csproj")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
