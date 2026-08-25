using Empodera.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace Empodera.Services;

public static class UserCultureService
{
    public const string FallbackCulture = "pt-BR";
    public const string PreferenceModeCookieName = ".Empodera.LanguageMode";

    private static readonly IReadOnlyDictionary<string, string> SupportedCultures =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pt"] = "pt-BR",
            ["pt-BR"] = "pt-BR",
            ["en"] = "en",
            ["en-US"] = "en",
            ["es"] = "es",
            ["es-ES"] = "es"
        };

    public static string FromBrowser(StringValues acceptLanguage)
    {
        var requestedLanguages = acceptLanguage.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select((item, index) =>
            {
                var parts = item.Split(';', 2);
                var quality = 1d;
                if (parts.Length == 2 && parts[1].Trim().StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                    double.TryParse(parts[1].Trim()[2..], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out quality);

                return new { Language = parts[0].Trim().ToLowerInvariant(), Quality = quality, Index = index };
            })
            .OrderByDescending(item => item.Quality)
            .ThenBy(item => item.Index);

        foreach (var item in requestedLanguages)
        {
            var language = item.Language;
            if (language.StartsWith("pt")) return "pt-BR";
            if (language.StartsWith("en")) return "en";
            if (language.StartsWith("es")) return "es";
        }

        return FallbackCulture;
    }

    public static void ApplyPreference(HttpResponse response, IdiomaPreferido preference)
    {
        if (preference == IdiomaPreferido.Default)
        {
            FollowBrowser(response);
            return;
        }

        var culture = preference switch
        {
            IdiomaPreferido.Portugues => "pt-BR",
            IdiomaPreferido.Ingles => "en",
            IdiomaPreferido.Espanhol => "es",
            _ => FallbackCulture
        };

        ApplyCulture(response, culture);
    }

    public static bool TryApplyCulture(HttpResponse response, string? requestedCulture)
    {
        if (string.IsNullOrWhiteSpace(requestedCulture)
            || !SupportedCultures.TryGetValue(requestedCulture.Trim(), out var culture))
            return false;

        ApplyCulture(response, culture);
        return true;
    }

    public static void FollowBrowser(HttpResponse response)
    {
        response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        AppendModeCookie(response, "browser");
    }

    public static bool HasSavedMode(HttpRequest request) =>
        request.Cookies.ContainsKey(PreferenceModeCookieName);

    private static void ApplyCulture(HttpResponse response, string culture)
    {
        response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            CreateCookieOptions(response));
        AppendModeCookie(response, "explicit");
    }

    private static void AppendModeCookie(HttpResponse response, string mode) =>
        response.Cookies.Append(PreferenceModeCookieName, mode, CreateCookieOptions(response));

    private static CookieOptions CreateCookieOptions(HttpResponse response) => new()
    {
        Expires = DateTimeOffset.UtcNow.AddYears(1),
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = response.HttpContext.Request.IsHttps
    };
}
