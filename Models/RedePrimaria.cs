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
        public string TipoRelacao { get; set; } = null!;

        public Atores AtorPrincipal { get; set; } = null!;
        public Atores AtorRelacionado { get; set; } = null!;
    }
}