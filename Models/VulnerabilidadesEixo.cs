using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class VulnerabilidadesEixo
    {
        [Key]
        public int IdVEixo { get; set; }
        public int EixoId { get; set; }
        public int VulnerabilidadeId { get; set; }
    }
}