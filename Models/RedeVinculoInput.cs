namespace Empodera.Models;

// Vínculo de rede (primária ou subjetiva) enviado pelo formulário de Atores.
// Quando AtorId não é informado, o ator relacionado é localizado pelo nome na
// comunidade escolhida ou criado nela.
public class RedeVinculoInput
{
    public string TipoRede { get; set; } = TiposRede.Primaria;
    public int? AtorId { get; set; }
    public string? Nome { get; set; }
    public int? ComunidadeId { get; set; }
    public string TipoRelacao { get; set; } = string.Empty;
}
