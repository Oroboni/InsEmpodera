using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Empodera.Services;

// Aviso de sucesso exibido como toast na próxima tela (_AvisosDaPagina).
// Seguro fora do pipeline HTTP (testes que criam o controller direto, sem TempData).
public static class Avisos
{
    public static void AvisarSucesso(this Controller controller, string mensagem)
    {
        var http = controller.HttpContext;
        if (http?.RequestServices?.GetService(typeof(ITempDataDictionaryFactory)) is ITempDataDictionaryFactory fabrica)
            fabrica.GetTempData(http)["Sucesso"] = mensagem;
    }
}
