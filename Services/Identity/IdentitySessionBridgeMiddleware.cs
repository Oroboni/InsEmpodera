using System.Security.Claims;

namespace Empodera.Services.Identity;

/// <summary>
/// Mantém somente a compatibilidade visual com código legado. A sessão é sempre
/// derivada do principal autenticado e nunca é usada para criar uma identidade.
/// </summary>
public sealed class IdentitySessionBridgeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            SetIfPresent(context, "ID", context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            SetIfPresent(context, "Email", context.User.FindFirstValue(ClaimTypes.Email));
            SetIfPresent(
                context,
                "Nome",
                context.User.FindFirstValue(ClaimTypes.GivenName) ?? context.User.Identity.Name);
        }
        else
        {
            context.Session.Remove("ID");
            context.Session.Remove("Email");
            context.Session.Remove("Nome");
        }

        await next(context);
    }

    private static void SetIfPresent(HttpContext context, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            context.Session.SetString(key, value);
    }
}
