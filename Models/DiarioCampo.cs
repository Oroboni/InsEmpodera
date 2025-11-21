using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Adicione isso

namespace Empodera.Models
{

    public class DiarioCampo
{
    public int Id { get; set; }
    public int ComunidadeId { get; set; }
    public Comunidade? Comunidade { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimaAtualizacao { get; set; }
    public string? Descricao { get; set; } // salvar com marcações de @
    public string? CEP { get; set; }
    public string? Endereco { get; set; }
    public string? CriadoPor { get; set; }
    public string? AtualizadoPor { get; set; }
   public ICollection<AnexosDiario> Anexos { get; set; }
    public ICollection<DiarioEixo> Eixos { get; set; }
    public ICollection<DiarioAcoes> DiarioAcoes { get; set; } = new List<DiarioAcoes>();

}

    
    /*
    public class DiarioCampo
    {
        [Key]
        public int IdDCampo { get; set; }

        public int ComunidadeId { get; set; }
        
        public int? AtorId { get; set; } // Link com o Ator
        
        [ForeignKey("AtorId")]
        public virtual Ator? Ator { get; set; } // Navegação

        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
    */
}