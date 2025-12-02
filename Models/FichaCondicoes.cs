using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaCondicoes
    {
        public int IdCondicoes { get; set; }    
        public int FkIdFicha { get; set; }   
        public string Cond { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}