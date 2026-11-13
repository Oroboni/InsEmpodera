using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers;

// Páginas de erro no visual do EcoRedes (404, 403, 500...), no lugar da tela cinza do navegador.
// Usadas por UseExceptionHandler("/Erro") e UseStatusCodePagesWithReExecute("/Erro/{0}") no Program.cs.
// AllowAnonymous: a política padrão exige login; sem isto o erro de quem não entrou virava redirecionamento.
[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErroController : Controller
{
    private readonly ILogger<ErroController> _logger;

    public ErroController(ILogger<ErroController> logger) => _logger = logger;

    [Route("Erro")]
    [Route("Erro/{codigo:int}")]
    [IgnoreAntiforgeryToken]
    public IActionResult Index(int? codigo)
    {
        var excecao = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var status = codigo ?? (excecao is null ? StatusCodes.Status404NotFound : StatusCodes.Status500InternalServerError);
        if (excecao is not null)
            _logger.LogError(excecao.Error, "Erro não tratado em {Caminho}", excecao.Path);

        Response.StatusCode = status;
        ViewBag.Codigo = status;
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewBag.Logado = HttpContext.Session.GetString("ID") is not null;
        return View("~/Views/Shared/Error.cshtml");
    }
}
