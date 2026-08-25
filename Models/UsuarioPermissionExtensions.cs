namespace Empodera.Models;

public static class UsuarioPermissionExtensions
{
    public static bool CanList(this Usuario? user, string module) =>
        PermissionFor(user, module)?.PodeListar == "S";

    public static bool CanViewDetails(this Usuario? user, string module) =>
        PermissionFor(user, module)?.PodeDetalhar == "S";

    public static bool CanCreate(this Usuario? user, string module) =>
        PermissionFor(user, module)?.PodeCriar == "S";

    public static bool CanUpdate(this Usuario? user, string module) =>
        PermissionFor(user, module)?.PodeAtualizar == "S";

    public static bool CanDelete(this Usuario? user, string module) =>
        PermissionFor(user, module)?.PodeDeletar == "S";

    private static Permissoes? PermissionFor(Usuario? user, string module)
    {
        if (user?.Ativo != "S" || user.Perfil?.Permissoes is null)
            return null;

        var matches = user.Perfil.Permissoes
            .Where(permission => string.Equals(permission.Modulo, module, StringComparison.Ordinal))
            .Take(2)
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }
}