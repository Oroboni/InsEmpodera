using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Adicione isso

namespace Empodera.Models
{
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
}