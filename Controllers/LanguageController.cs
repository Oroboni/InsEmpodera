using Microsoft.AspNetCore.Mvc;
using Empodera.Services;
using System.Globalization;

namespace Empodera.Controllers;

public sealed class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Catalog([FromQuery] string? page) =>
        Json(LocalizedHtmlMiddleware.GetCatalog(CultureInfo.CurrentUICulture, page));

}
