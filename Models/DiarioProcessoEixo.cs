namespace Empodera.Models;

public class DiarioProcessoEixo
{
    public Guid IdDiarioProcessoEixo { get; set; }
    public Guid FkIdDiarioProcesso { get; set; }
    public Guid FkIdEixo { get; set; }

    public DiarioProcessoPessoal DiarioProcesso { get; set; } = null!;
    public Eixo Eixo { get; set; } = null!;
}
