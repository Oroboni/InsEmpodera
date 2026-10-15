using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DetalhesEixos
    {
        public Guid IdDiarioEixo { get; set; }   
        public Guid FkIdDetalhes { get; set; }  
        public Guid FkIdEixo { get; set; }    

        public DetalhesDAcoes Detalhes { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}
