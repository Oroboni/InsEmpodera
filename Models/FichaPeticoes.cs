using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaPeticoes
    {
        [Key]
        public int IdPeticoes { get; set; }
        public int FichaId { get; set; }
        public string Pet { get; set; } = string.Empty;
    }
}