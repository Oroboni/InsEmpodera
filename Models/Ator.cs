using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Ator
    {
        [Key]
        public int IdAtores { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string PapelSocial1 { get; set; } = string.Empty;
        public string PapelSocial2 { get; set; } = string.Empty;
        public int Telefone { get; set; }
        public string Extra { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}