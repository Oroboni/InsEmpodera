using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaCondicoes
    {
        [Key]
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Cond { get; set; } = string.Empty;
    }
}