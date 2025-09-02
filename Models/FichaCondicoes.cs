using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class FichaCondicoes
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Cond { get; set; } = string.Empty;
    }
}