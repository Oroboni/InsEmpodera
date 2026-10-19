using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Empodera.Services;

/// <summary>
/// Cores da plataforma escolhidas por cada usuário em Configurações > Cores da plataforma.
/// Ficam em App_Data/temas-usuarios/{id do usuário}.json e sobrepõem, só para aquele usuário,
/// o tema base (chave "Tema" do appsettings). Sem o arquivo, vale o tema base de wwwroot/css/tema.css.
/// </summary>
public sealed partial class TemaPersonalizadoService
{
    /// <summary>Variáveis CSS que podem ser personalizadas (sem o prefixo "--").</summary>
    public static readonly IReadOnlyList<string> Tokens =
    [
        "color-primary", "color-primary-light", "color-primary-dark",
        "color-primary-50", "color-primary-100", "color-primary-200", "color-primary-300",
        "color-primary-400", "color-primary-500", "color-primary-600", "color-primary-700", "color-primary-800",
        "color-secondary", "color-secondary-light", "color-secondary-dark",
        "color-gray-100", "color-gray-200", "color-gray-300", "color-gray-400",
        "color-gray-600", "color-gray-800", "color-gray-900"
    ];

    private static readonly HashSet<string> TokensPermitidos = new(Tokens, StringComparer.Ordinal);
    private readonly string _pasta;
    private readonly ConcurrentDictionary<Guid, Dictionary<string, string>?> _cache = new();

    public TemaPersonalizadoService(IWebHostEnvironment ambiente)
    {
        _pasta = Path.Combine(ambiente.ContentRootPath, "App_Data", "temas-usuarios");
    }

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex CorHex();

    // O nome do arquivo vem só do Id (Guid), nunca de algo digitado.
    private string Arquivo(Guid usuarioId) => Path.Combine(_pasta, $"{usuarioId:N}.json");

    public IReadOnlyDictionary<string, string>? Obter(Guid usuarioId) =>
        _cache.GetOrAdd(usuarioId, id =>
        {
            try
            {
                var arquivo = Arquivo(id);
                if (!File.Exists(arquivo)) return null;
                var lido = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(arquivo));
                return lido is null ? null : Filtrar(lido);
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                return null; // arquivo inválido: segue com o tema base
            }
        });

    /// <summary>Salva as cores do usuário. Ignora tokens desconhecidos; rejeita valores que não sejam #RRGGBB.</summary>
    public void Salvar(Guid usuarioId, IDictionary<string, string> cores)
    {
        var validas = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (token, valor) in cores)
        {
            if (!TokensPermitidos.Contains(token)) continue;
            if (!CorHex().IsMatch(valor ?? string.Empty))
                throw new ArgumentException($"Cor inválida para {token}: use o formato #RRGGBB.");
            validas[token] = valor!.ToUpperInvariant();
        }
        if (validas.Count == 0)
            throw new ArgumentException("Nenhuma cor válida foi enviada.");

        Directory.CreateDirectory(_pasta);
        File.WriteAllText(Arquivo(usuarioId), JsonSerializer.Serialize(validas, new JsonSerializerOptions { WriteIndented = true }));
        _cache[usuarioId] = validas;
    }

    /// <summary>Remove a personalização do usuário; ele volta ao tema base.</summary>
    public void Restaurar(Guid usuarioId)
    {
        var arquivo = Arquivo(usuarioId);
        if (File.Exists(arquivo)) File.Delete(arquivo);
        _cache[usuarioId] = null;
    }

    /// <summary>CSS com as variáveis personalizadas (inclui os canais *-rgb usados em transparências).</summary>
    public string? GerarCss(Guid usuarioId)
    {
        var cores = Obter(usuarioId);
        if (cores is null || cores.Count == 0) return null;

        var css = new StringBuilder(":root[data-tema] {");
        foreach (var token in Tokens)
        {
            if (!cores.TryGetValue(token, out var valor)) continue;
            css.Append("--").Append(token).Append(':').Append(valor).Append(';');
            if (token.StartsWith("color-primary", StringComparison.Ordinal))
            {
                var r = int.Parse(valor.AsSpan(1, 2), NumberStyles.HexNumber);
                var g = int.Parse(valor.AsSpan(3, 2), NumberStyles.HexNumber);
                var b = int.Parse(valor.AsSpan(5, 2), NumberStyles.HexNumber);
                css.Append("--").Append(token).Append("-rgb:").Append(r).Append(' ').Append(g).Append(' ').Append(b).Append(';');
            }
        }
        return css.Append('}').ToString();
    }

    private static Dictionary<string, string> Filtrar(Dictionary<string, string> origem) =>
        origem.Where(par => TokensPermitidos.Contains(par.Key) && CorHex().IsMatch(par.Value ?? string.Empty))
            .ToDictionary(par => par.Key, par => par.Value.ToUpperInvariant(), StringComparer.Ordinal);
}
