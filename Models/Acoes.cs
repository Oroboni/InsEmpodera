using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
   public class Acoes
    {
        public Guid IdAcoes { get; set; }    
        public int Quantidade { get; set; }
        public Guid FkIdAtividade { get; set; }  
        public string Nome { get; set; } = null!;
        public string Provedor { get; set; } = null!;

        public Atividades Atividades { get; set; } = null!;
        public List<AcoesAtores> AcoesAtores { get; set; } = new();
        public List<DiarioAcoes> DiarioAcoes { get; set; } = new();

    }
}
