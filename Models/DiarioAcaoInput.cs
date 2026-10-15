namespace Empodera.Models;

/// <summary>
/// Dados de uma ação montada dinamicamente no formulário de diário de campo.
/// </summary>
public sealed class DiarioAcaoInput
{
    public string Nome { get; set; } = string.Empty;
    public string? Provedor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public Guid[] FkIdAtores { get; set; } = [];
    public Guid[] FkIdEixo { get; set; } = [];
}
