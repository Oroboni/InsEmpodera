using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResp
    {
        public int IdCondicoes { get; set; }     
        public int FkIdFicha { get; set; }     
        [StringLength(150)]
        public string Resp { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}
