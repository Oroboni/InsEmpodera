using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Empodera.Services;

// Registro de operações para os logs (CloudWatch Logs na AWS):
//   INFO  operação concluída (cadastro, edição, exclusão)
//   WARN  situação inesperada (formulário recusado, acesso negado)
//   ERROR falha ao processar (registrado pelo ErroController a partir do UseExceptionHandler)
// Nunca registra valores do formulário, senhas, tokens, e-mails ou dados pessoais:
// só a tela, a ação, o id do usuário e o resultado.
public sealed class RegistroDeOperacoes : IAsyncActionFilter
{
    private readonly ILogger<RegistroDeOperacoes> _logger;

    public RegistroDeOperacoes(ILogger<RegistroDeOperacoes> logger) => _logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executado = await next();
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method) || executado.Exception is not null)
            return;

        var tela = context.RouteData.Values["controller"]?.ToString() ?? "?";
        var acao = context.RouteData.Values["action"]?.ToString() ?? "?";
        var usuario = context.HttpContext.Session.GetString("ID") ?? "anônimo";
        var status = (executado.Result as IStatusCodeActionResult)?.StatusCode;

        if (status is StatusCodes.Status403Forbidden or StatusCodes.Status400BadRequest)
            _logger.LogWarning("Operação negada {Tela}/{Acao} usuário={Usuario} status={Status}", tela, acao, usuario, status);
        else if (!context.ModelState.IsValid)
            _logger.LogWarning("Formulário recusado {Tela}/{Acao} usuário={Usuario} campos={Campos}",
                tela, acao, usuario, string.Join(",", context.ModelState.Where(e => e.Value?.Errors.Count > 0).Select(e => e.Key).Take(10)));
        else if (executado.Result is RedirectToActionResult or RedirectResult or LocalRedirectResult)
            _logger.LogInformation("Operação concluída {Tela}/{Acao} usuário={Usuario}", tela, acao, usuario);
    }
}
