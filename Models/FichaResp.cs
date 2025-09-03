using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResp
    {
        [Key]
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Resp { get; set; } = string.Empty;
    }
}