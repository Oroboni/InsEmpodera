using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedePrimaria
    {
        public Guid IdRedePrimaria { get; set; }   
        public Guid FkIdAtorPrincipal { get; set; }    
        public Guid FkIdAtorRelacionados { get; set; } 
        public string TipoRelacao { get; set; } = null!;
        // "Primaria" ou "Subjetiva" (ver TiposRede).
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
