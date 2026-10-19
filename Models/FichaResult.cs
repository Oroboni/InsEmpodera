using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResult
    {
        public Guid IdCondicoes { get; set; }     
        public Guid FkIdFicha { get; set; }       
        public string Result { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}
