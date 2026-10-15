using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaPeticoes
    {
        public Guid IdPeticoes { get; set; }     
        public Guid FkIdFicha { get; set; }    
        public string Pet { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}
