using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaResult
    {
        [Key]
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}