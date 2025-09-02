using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class FichaResult
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}