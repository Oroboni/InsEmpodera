using Empodera.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace Empodera.Services;

public static class UserCultureService
{
    public const string FallbackCulture = "pt-BR";

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
            response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
            return;
        }

        var culture = preference switch
        {
            IdiomaPreferido.Portugues => "pt-BR",
            IdiomaPreferido.Ingles => "en",
            IdiomaPreferido.Espanhol => "es",
            _ => FallbackCulture
        };

        response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = response.HttpContext.Request.IsHttps
            });
    }

    public static void ClearPreference(HttpResponse response) =>
        response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
}
