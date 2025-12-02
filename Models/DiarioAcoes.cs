using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioAcoes
    {
        public int IdDAcoes { get; set; }       
        public int FkIdAcoes { get; set; }      
        public int FkIdDiario { get; set; }     

        public Acoes Acoes { get; set; } = null!;
        public DiarioCampo Diario { get; set; } = null!;
    }
}