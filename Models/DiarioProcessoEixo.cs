namespace Empodera.Models;

public class DiarioProcessoEixo
{
    public int IdDiarioProcessoEixo { get; set; }
    public int FkIdDiarioProcesso { get; set; }
    public int FkIdEixo { get; set; }

    public DiarioProcessoPessoal DiarioProcesso { get; set; } = null!;
    public Eixo Eixo { get; set; } = null!;
}
