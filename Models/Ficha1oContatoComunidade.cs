using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Empodera.Models
{
    public class Ficha1oContatoComunidade
    {
        [Key]
        public int IdFichaComunidade { get; set; }
        public int IdFicha { get; set; }
        public int FkIdComunidade { get; set; }

        [ForeignKey("IdFicha")]
        public virtual FichaPrimeiroContato FichaPrimeiroContato { get; set; } = null!;
        
        [ForeignKey("FkIdComunidade")]
        public virtual Comunidade Comunidade { get; set; } = null!;
    }
}