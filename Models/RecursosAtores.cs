using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RecursosAtores
    {
        public int Id_Recursos_Atores { get; set; }
        public int FK_id_Atores { get; set; }     
        [StringLength(50)]
        public string Tipo { get; set; } = null!;
        [StringLength(150)]
        public string Nome { get; set; } = null!;
        public string? Pode { get; set; } = null!;
        public Atores? Atores { get; set; }
    }
}
