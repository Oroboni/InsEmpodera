using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DetalhesEixos
    {
        public int IdDiarioEixo { get; set; }   
        public int FkIdDetalhes { get; set; }  
        public int FkIdEixo { get; set; }    

        public DetalhesDAcoes Detalhes { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}