using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResult
    {
        public int IdCondicoes { get; set; }     
        public int FkIdFicha { get; set; }       
        public string Result { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}