using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioEixo
    {
        [Key]
        public int IdDiarioEixo { get; set; }
        public int DiarioId { get; set; }
        public int EixoId { get; set; }
    }
}