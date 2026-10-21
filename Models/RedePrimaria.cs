using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedePrimaria
    {
        public int IdRedePrimaria { get; set; }   
        public int FkIdAtorPrincipal { get; set; }    
        public int FkIdAtorRelacionados { get; set; } 
        [StringLength(100)]
        public string TipoRelacao { get; set; } = null!;
        // "Primaria" ou "Subjetiva" (ver TiposRede).
        [StringLength(20)]
        public string TipoRede { get; set; } = TiposRede.Primaria;

        public Atores AtorPrincipal { get; set; } = null!;
        public Atores AtorRelacionado { get; set; } = null!;
    }

    public static class TiposRede
    {
        public const string Primaria = "Primaria";
        public const string Subjetiva = "Subjetiva";

        public static bool EhValido(string? tipo) => tipo is Primaria or Subjetiva;
    }
}
