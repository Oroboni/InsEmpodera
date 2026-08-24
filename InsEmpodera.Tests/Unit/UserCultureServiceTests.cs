using Xunit;
using Empodera.Services;
using Microsoft.Extensions.Primitives;

namespace InsEmpodera.Tests.Unit;

public sealed class UserCultureServiceTests
{
    [Theory]
    [InlineData("pt-BR,pt;q=0.9,en;q=0.8", "pt-BR")]
    [InlineData("en-US,en;q=0.9", "en")]
    [InlineData("es-MX,es;q=0.9", "es")]
    [InlineData("fr-FR,es;q=0.8,en;q=0.9", "en")]
    [InlineData("fr-FR", "pt-BR")]
    [InlineData("", "pt-BR")]
    public void FromBrowser_SelectsTheBestSupportedLanguage(string header, string expected)
    {
        var result = UserCultureService.FromBrowser(new StringValues(header));
        Assert.Equal(expected, result);
    }
}
