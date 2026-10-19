namespace Empodera.Services;

/// <summary>
/// Nomes dos idiomas da interface para os seletores de idioma (login e Configurações).
/// O nome principal vem no idioma em uso (em português: "Inglês"); o nome nativo
/// ("English (US)") fica como apoio, para quem não lê o idioma atual achar o seu.
/// </summary>
public static class IdiomasInterface
{
    public static readonly string[] Codigos = ["pt-BR", "en", "es"];

    public static string Sigla(string codigo) => codigo switch
    {
        "en" => "EN",
        "es" => "ES",
        _ => "PT"
    };

    /// <summary>Nome do idioma <paramref name="codigo"/> escrito no idioma <paramref name="culturaAtual"/>.</summary>
    public static string Nome(string codigo, string culturaAtual) => (Base(culturaAtual), codigo) switch
    {
        ("en", "pt-BR") => "Portuguese",
        ("en", "en") => "English",
        ("en", "es") => "Spanish",
        ("es", "pt-BR") => "Portugués",
        ("es", "en") => "Inglés",
        ("es", "es") => "Español",
        (_, "en") => "Inglês",
        (_, "es") => "Espanhol",
        _ => "Português"
    };

    /// <summary>Nome do idioma escrito nele mesmo.</summary>
    public static string NomeNativo(string codigo) => codigo switch
    {
        "en" => "English (US)",
        "es" => "Español",
        _ => "Português do Brasil"
    };

    private static string Base(string cultura) =>
        cultura.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en"
        : cultura.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "es"
        : "pt";
}
