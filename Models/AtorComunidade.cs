using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtorComunidade
    {
        [Key]
        public int IdAComunidade { get; set; }
        public int ComunidadeId { get; set; }
        public int AtorId { get; set; }
        public List<Ator>? Atores { get; set; }
        public List<Comunidade>? Comunidades { get; set; }
    }
}