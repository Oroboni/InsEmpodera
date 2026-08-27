using Microsoft.AspNetCore.Mvc;
using Empodera.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace Empodera.Controllers;

[AllowAnonymous]
public sealed class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Catalog([FromQuery] string? page) =>
        Json(LocalizedHtmlMiddleware.GetCatalog(CultureInfo.CurrentUICulture, page));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set([FromForm] string culture, [FromForm] string? returnUrl)
    {
        if (string.Equals(culture, "auto", StringComparison.OrdinalIgnoreCase))
            UserCultureService.FollowBrowser(Response);
        else if (!UserCultureService.TryApplyCulture(Response, culture))
            return BadRequest();

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Account");
    }

}
