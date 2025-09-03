using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedePrimaria
    {
        [Key]
        public int IdRedePrimaria { get; set; }
        public int AtorPrincipalId { get; set; }
        public int AtorRelacionadoId { get; set; }
        public string TipoRelacao { get; set; } = string.Empty;
    }
}