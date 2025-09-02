using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class AnexosDiario
    {
        public int IdAnexos { get; set; }
        public int DiarioId { get; set; }
        public string Caminho { get; set; } = string.Empty;
    }
}