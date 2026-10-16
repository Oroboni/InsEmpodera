namespace Empodera.Services;

/// <summary>
/// Fotos de perfil dos usuários. Ficam em App_Data/fotos-usuarios (fora da pasta pública wwwroot)
/// e são entregues por UsersController.Foto somente a quem está autenticado.
/// O nome do arquivo é sempre derivado do Id do usuário; nada vindo do navegador vira caminho.
/// </summary>
public sealed class FotoUsuarioService
{
    public const long TamanhoMaximo = 2 * 1024 * 1024; // 2 MB

    private static readonly (string Extensao, string Tipo, byte[] Assinatura, int Deslocamento)[] Formatos =
    [
        (".jpg", "image/jpeg", [0xFF, 0xD8, 0xFF], 0),
        (".png", "image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], 0),
        (".webp", "image/webp", "WEBP"u8.ToArray(), 8)
    ];

    private readonly string _pasta;

    public FotoUsuarioService(IWebHostEnvironment ambiente)
    {
        _pasta = Path.Combine(ambiente.ContentRootPath, "App_Data", "fotos-usuarios");
    }

    /// <summary>Confere tamanho e formato real (assinatura do arquivo). Retorna a mensagem de erro ou null.</summary>
    public static string? Validar(IFormFile? arquivo)
    {
        if (arquivo is null) return null;
        if (arquivo.Length == 0) return "O arquivo da foto está vazio.";
        if (arquivo.Length > TamanhoMaximo) return "A foto deve ter no máximo 2 MB.";
        var cabecalho = new byte[12];
        using (var leitura = arquivo.OpenReadStream())
        {
            var lidos = 0;
            while (lidos < cabecalho.Length)
            {
                var n = leitura.Read(cabecalho, lidos, cabecalho.Length - lidos);
                if (n == 0) break;
                lidos += n;
            }
        }
        var valido = Formatos.Any(f => cabecalho.AsSpan(f.Deslocamento, f.Assinatura.Length).SequenceEqual(f.Assinatura)
            && (f.Extensao != ".webp" || cabecalho.AsSpan(0, 4).SequenceEqual("RIFF"u8)));
        return valido ? null : "Envie uma imagem JPG, PNG ou WEBP.";
    }

    /// <summary>Valida e grava a foto. Retorna o nome do arquivo salvo ou lança ArgumentException com a mensagem para o usuário.</summary>
    public async Task<string> SalvarAsync(Guid usuarioId, IFormFile arquivo, CancellationToken cancelamento = default)
    {
        if (arquivo.Length == 0)
            throw new ArgumentException("O arquivo da foto está vazio.");
        if (arquivo.Length > TamanhoMaximo)
            throw new ArgumentException("A foto deve ter no máximo 2 MB.");

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, cancelamento);
        var bytes = memoria.ToArray();

        var formato = Formatos.FirstOrDefault(f => bytes.Length >= f.Deslocamento + f.Assinatura.Length
            && bytes.AsSpan(f.Deslocamento, f.Assinatura.Length).SequenceEqual(f.Assinatura)
            && (f.Extensao != ".webp" || bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)));
        if (formato.Extensao is null)
            throw new ArgumentException("Envie uma imagem JPG, PNG ou WEBP.");

        Directory.CreateDirectory(_pasta);
        Remover(usuarioId);
        var nome = $"{usuarioId:N}{formato.Extensao}";
        await File.WriteAllBytesAsync(Path.Combine(_pasta, nome), bytes, cancelamento);
        return nome;
    }

    public void Remover(Guid usuarioId)
    {
        foreach (var (extensao, _, _, _) in Formatos)
        {
            var caminho = Path.Combine(_pasta, $"{usuarioId:N}{extensao}");
            if (File.Exists(caminho)) File.Delete(caminho);
        }
    }

    /// <summary>Caminho e tipo da foto do usuário, ou null se ele não tiver foto.</summary>
    public (string Caminho, string Tipo, DateTime Modificada)? Obter(Guid usuarioId)
    {
        foreach (var (extensao, tipo, _, _) in Formatos)
        {
            var caminho = Path.Combine(_pasta, $"{usuarioId:N}{extensao}");
            if (File.Exists(caminho)) return (caminho, tipo, File.GetLastWriteTimeUtc(caminho));
        }
        return null;
    }

    /// <summary>URL da foto com versão (para o navegador buscar a nova após troca), ou null.</summary>
    public string? Url(Guid usuarioId)
    {
        var foto = Obter(usuarioId);
        return foto is null ? null : $"/Users/Foto/{usuarioId}?v={foto.Value.Modificada.Ticks:x}";
    }

    public static string Iniciais(string? nome)
    {
        var partes = (nome ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length == 0 ? "?" : string.Concat(partes.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}
