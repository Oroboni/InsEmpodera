using System.Reflection;
using Empodera.Controllers;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class SecurityHardeningHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public SecurityHardeningHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "Segurança HTTP — respostas incluem cabeçalhos defensivos")]
    public async Task PublicResponse_ContainsDefensiveSecurityHeaders()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/Account");

        response.EnsureSuccessStatusCode();
        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", Header(response, "X-Frame-Options"));
        Assert.Equal("strict-origin-when-cross-origin", Header(response, "Referrer-Policy"));
        Assert.Contains("frame-ancestors 'none'", Header(response, "Content-Security-Policy"), StringComparison.Ordinal);
        Assert.Contains("form-action 'self'", Header(response, "Content-Security-Policy"), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Segurança do login — endpoint possui limitação de tentativas")]
    public void LoginPost_HasNamedRateLimitPolicy()
    {
        var action = typeof(AccountController).GetMethod(
            nameof(AccountController.Index),
            BindingFlags.Instance | BindingFlags.Public,
            [typeof(string), typeof(string)]);

        var attribute = Assert.Single(action!.GetCustomAttributes<EnableRateLimitingAttribute>());
        Assert.Equal("login", attribute.PolicyName);
    }

    private static string Header(HttpResponseMessage response, string name)
    {
        Assert.True(response.Headers.TryGetValues(name, out var values), $"Cabeçalho ausente: {name}");
        return Assert.Single(values);
    }
}
