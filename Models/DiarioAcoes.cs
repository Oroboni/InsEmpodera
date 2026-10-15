using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioAcoes
    {
        public Guid IdDAcoes { get; set; }       
        public Guid FkIdAcoes { get; set; }      
        public Guid FkIdDiario { get; set; }     

        public Acoes Acoes { get; set; } = null!;
        public DiarioCampo Diario { get; set; } = null!;
    }
}
