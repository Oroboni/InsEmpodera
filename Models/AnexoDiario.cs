using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AnexosDiario
    {
        [Key]
        public int IdAnexos { get; set; }
        public int DiarioId { get; set; }
        public string Caminho { get; set; } = string.Empty;
    }
}