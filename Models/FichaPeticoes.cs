using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class FichaPeticoes
    {
        public int IdPeticoes { get; set; }
        public int FichaId { get; set; }
        public string Pet { get; set; } = string.Empty;
    }
}