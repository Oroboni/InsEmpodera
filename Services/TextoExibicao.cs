using System.Globalization;

namespace Empodera.Services;

// Pequenos ajustes de texto para exibição (ex.: "joao" -> "Joao" na saudação).
public static class TextoExibicao
{
    public static string Nome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;
        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', partes.Select(p => p.Length == 1
            ? p.ToUpper(CultureInfo.CurrentCulture)
            : char.ToUpper(p[0], CultureInfo.CurrentCulture) + p[1..]));
    }
}
