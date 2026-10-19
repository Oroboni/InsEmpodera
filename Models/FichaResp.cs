using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResp
    {
        public Guid IdCondicoes { get; set; }     
        public Guid FkIdFicha { get; set; }     
        public string Resp { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}
