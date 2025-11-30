using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeEixo
    {
        public int IdRedeEixo { get; set; }  
        public int FkIdRede { get; set; }    
        public int FkIdEixo { get; set; }    

        public RedeRecursos RedeRecursos { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}