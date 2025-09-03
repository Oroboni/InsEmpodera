using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AvaliacaoPessoal
    {
        [Key]
        public int IdAvaliacao { get; set; }
        public int AtorId { get; set; }
        public int CCrimes { get; set; }
        public int Substancias { get; set; }
        public int Moradia { get; set; }
        public int Prevenção { get; set; }
        public int AssBasica { get; set; }
        public int Educacao { get; set; }
        public int Saude { get; set; }
        public int Ocupacao { get; set; }
        public int Lazer { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}