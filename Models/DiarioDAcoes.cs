using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
   public class DiarioDAcoes
    {
        public Guid IdDAcoes { get; set; }       
        public Guid FkIdDiario { get; set; }    
        public string Nome { get; set; } = null!;
        public string PeovedorEx { get; set; } = null!;
        public int Quantidade { get; set; }

        public DiarioCampo Diario { get; set; } = null!;
        public List<DetalhesDAcoes> Detalhes { get; set; } = new();
        public List<DAAtores> DAtores { get; set; } = new();
    }
}
