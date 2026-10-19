using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioEixo
    {
        public Guid IdDiarioEixo { get; set; }  
        public Guid FkIdDiario { get; set; }     
        public Guid FkIdEixo { get; set; }       

        public DiarioCampo Diario { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}
