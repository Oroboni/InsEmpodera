using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Empodera.Services;

/// <summary>
/// Cores do site definidas pela administração em Configurações > Cores do sistema.
/// Ficam em App_Data/tema-personalizado.json e sobrepõem o tema base (chave "Tema" do appsettings).
/// Sem o arquivo, vale apenas o tema base de wwwroot/css/tema.css.
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
    private readonly string _arquivo;
    private readonly object _trava = new();
    private Dictionary<string, string>? _cache;
    private bool _carregado;

    public TemaPersonalizadoService(IWebHostEnvironment ambiente)
    {
        _arquivo = Path.Combine(ambiente.ContentRootPath, "App_Data", "tema-personalizado.json");
    }

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex CorHex();

    public IReadOnlyDictionary<string, string>? Obter()
    {
        lock (_trava)
        {
            if (_carregado) return _cache;
            _carregado = true;
            try
            {
                if (File.Exists(_arquivo))
                {
                    var lido = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_arquivo));
                    _cache = lido is null ? null : Filtrar(lido);
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                _cache = null; // arquivo inválido: segue com o tema base
            }
            return _cache;
        }
    }

    /// <summary>Salva as cores. Ignora tokens desconhecidos; rejeita valores que não sejam #RRGGBB.</summary>
    public void Salvar(IDictionary<string, string> cores)
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

        lock (_trava)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_arquivo)!);
            File.WriteAllText(_arquivo, JsonSerializer.Serialize(validas, new JsonSerializerOptions { WriteIndented = true }));
            _cache = validas;
            _carregado = true;
        }
    }

    /// <summary>Remove a personalização e volta ao tema base.</summary>
    public void Restaurar()
    {
        lock (_trava)
        {
            if (File.Exists(_arquivo)) File.Delete(_arquivo);
            _cache = null;
            _carregado = true;
        }
    }

    /// <summary>CSS com as variáveis personalizadas (inclui os canais *-rgb usados em transparências).</summary>
    public string? GerarCss()
    {
        var cores = Obter();
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
