using System.ComponentModel.DataAnnotations;

namespace Empodera.Models;

public class DiarioProcessoPessoal
{
    public int IdDiarioProcesso { get; set; }

    [Display(Name = "Ator")]
    public int FK_id_Atores { get; set; }

    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "Informe a descrição do processo.")]
    [StringLength(4000, ErrorMessage = "A descrição deve ter no máximo 4.000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    public DateTime DtCriacao { get; set; }
    public DateTime DtModificacao { get; set; }
    public int FkIdUsuario { get; set; }
    public int? FkIdUsuarioM { get; set; }

    public Atores Ator { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public Usuario? UsuarioModificacao { get; set; }
    public List<DiarioProcessoEixo> Eixos { get; set; } = new();
}
