namespace Empodera.Models;

/// <summary>
/// Dados de uma ação montada dinamicamente no formulário de diário de campo.
/// </summary>
public sealed class DiarioAcaoInput
{
    public string Nome { get; set; } = string.Empty;
    public string Provedor { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public int? FkIdAtor { get; set; }
    public int[] FkIdEixo { get; set; } = [];
}
