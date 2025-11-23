using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioEixo
    {
        public int IdDiarioEixo { get; set; }  
        public int FkIdDiario { get; set; }     
        public int FkIdEixo { get; set; }       

        public DiarioCampo Diario { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}