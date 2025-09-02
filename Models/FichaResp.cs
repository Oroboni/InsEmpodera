using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class FichaResp
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Resp { get; set; } = string.Empty;
    }
}