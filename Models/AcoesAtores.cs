using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AcoesAtores
    {
        [Key]
        public int IdAAtores { get; set; }
        public int AtorId { get; set; }
        public int AcoesId { get; set; }
    }
}