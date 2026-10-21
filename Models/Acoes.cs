using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
   public class Acoes
    {
        public int IdAcoes { get; set; }    
        public int Quantidade { get; set; }
        public int FkIdAtividade { get; set; }  
        [StringLength(150)]
        public string Nome { get; set; } = null!;
        [StringLength(150)]
        public string Provedor { get; set; } = null!;

        public Atividades Atividades { get; set; } = null!;
        public List<AcoesAtores> AcoesAtores { get; set; } = new();
        public List<DiarioAcoes> DiarioAcoes { get; set; } = new();

    }
}
