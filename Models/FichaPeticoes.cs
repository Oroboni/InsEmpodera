using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaPeticoes
    {
        public int IdPeticoes { get; set; }     
        public int FkIdFicha { get; set; }    
        [StringLength(150)]
        public string Pet { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}
