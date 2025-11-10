using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Atividade
    {
        [Key]
        public int IdAtividade { get; set; }

        public string Nome { get; set; } = string.Empty;
        
        public string Descricao { get; set; } = string.Empty;

        // [NOVAS PROPRIEDADES ADICIONADAS]
        public DateTime DtCriacao { get; set; }
        
        public DateTime DtModificacao { get; set; }
        public virtual ICollection<AtividadesEixo> AtividadesEixo { get; set; }
    }
}