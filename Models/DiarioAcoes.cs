using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioAcoes
    {
        [Key]
        public int IdDAcoes { get; set; }
        public int AcoesId { get; set; }
        public int DiarioId { get; set; }
    } 
}