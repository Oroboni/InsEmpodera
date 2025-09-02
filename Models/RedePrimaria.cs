using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class RedePrimaria
    {
        public int IdRedePrimaria { get; set; }
        public int AtorPrincipalId { get; set; }
        public int AtorRelacionadoId { get; set; }
        public string TipoRelacao { get; set; } = string.Empty;
    }
}