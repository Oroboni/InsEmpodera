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

        public Atores AtorPrincipal { get; set; } = null!;
        public Atores AtorRelacionado { get; set; } = null!;
    }
}
