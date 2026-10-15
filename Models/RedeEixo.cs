using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeEixo
    {
        public Guid IdRedeEixo { get; set; }  
        public Guid FkIdRede { get; set; }    
        public Guid FkIdEixo { get; set; }    

        public RedeRecursos RedeRecursos { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}
