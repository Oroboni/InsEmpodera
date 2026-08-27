using Xunit;
using Empodera.Services;
using Empodera.Models;
using Microsoft.AspNetCore.Http;
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
    [InlineData("en;q=0.8,es;q=0.8", "en")]
    public void FromBrowser_SelectsTheBestSupportedLanguage(string header, string expected)
    {
        var result = UserCultureService.FromBrowser(new StringValues(header));
        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "Idioma explícito aceita todas as variantes suportadas e grava cookies seguros")]
    [InlineData("pt", "pt-BR")]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("es", "es")]
    [InlineData("es-ES", "es")]
    public void TryApplyCulture_AcceptsEverySupportedAlias(string requested, string expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";

        Assert.True(UserCultureService.TryApplyCulture(context.Response, requested));

        var cookies = Uri.UnescapeDataString(context.Response.Headers.SetCookie.ToString());
        Assert.Contains(expected, cookies, StringComparison.Ordinal);
        Assert.Contains($"{UserCultureService.PreferenceModeCookieName}=explicit", cookies, StringComparison.Ordinal);
        Assert.Contains("httponly", cookies, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookies, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookies, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "Idioma explícito rejeita valores ausentes ou não suportados sem criar cookie")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("fr-FR")]
    public void TryApplyCulture_RejectsUnsupportedValuesWithoutSideEffects(string? requested)
    {
        var context = new DefaultHttpContext();

        Assert.False(UserCultureService.TryApplyCulture(context.Response, requested));
        Assert.Equal(0, context.Response.Headers.SetCookie.Count);
    }

    [Fact(DisplayName = "Modo automático remove cultura fixa e registra acompanhamento do navegador")]
    public void FollowBrowser_DeletesExplicitCultureAndStoresBrowserMode()
    {
        var context = new DefaultHttpContext();

        UserCultureService.FollowBrowser(context.Response);

        var cookies = Uri.UnescapeDataString(context.Response.Headers.SetCookie.ToString());
        Assert.Contains(".AspNetCore.Culture=", cookies, StringComparison.Ordinal);
        Assert.Contains("expires=", cookies, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"{UserCultureService.PreferenceModeCookieName}=browser", cookies, StringComparison.Ordinal);
        Assert.DoesNotContain("secure", cookies, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Preferência padrão usa navegador e preferência explícita fixa a cultura")]
    public void ApplyPreference_UsesTheExpectedMode()
    {
        var automatic = new DefaultHttpContext();
        UserCultureService.ApplyPreference(automatic.Response, IdiomaPreferido.Default);
        Assert.Contains("=browser", automatic.Response.Headers.SetCookie.ToString(), StringComparison.Ordinal);

        var explicitCulture = new DefaultHttpContext();
        UserCultureService.ApplyPreference(explicitCulture.Response, IdiomaPreferido.Ingles);
        var cookies = Uri.UnescapeDataString(explicitCulture.Response.Headers.SetCookie.ToString());
        Assert.Contains("en", cookies, StringComparison.Ordinal);
        Assert.Contains("=explicit", cookies, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Detecção do modo salvo diferencia requisição nova de requisição configurada")]
    public void HasSavedMode_ReflectsThePreferenceCookie()
    {
        var context = new DefaultHttpContext();
        Assert.False(UserCultureService.HasSavedMode(context.Request));

        context.Request.Headers.Cookie = $"{UserCultureService.PreferenceModeCookieName}=browser";
        Assert.True(UserCultureService.HasSavedMode(context.Request));
    }

    [Fact(DisplayName = "Novo usuário nasce ativo e com idioma automático")]
    public void UserDefaults_AreCanonicalAndSafe()
    {
        var user = new Usuario();
        Assert.Equal("S", user.Ativo);
        Assert.Equal(IdiomaPreferido.Default, user.IdiomaPreferido);
    }
}
