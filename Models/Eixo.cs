using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Eixo
    {
        [Key]
        public int IdEixo { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}