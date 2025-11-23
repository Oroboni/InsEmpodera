using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AnexosDiario
    {
        public int IdAnexos { get; set; }      
        public int FkIdDiario { get; set; }     
        public string Caminho { get; set; } = null!;

        public DiarioCampo Diario { get; set; } = null!;
    }
}