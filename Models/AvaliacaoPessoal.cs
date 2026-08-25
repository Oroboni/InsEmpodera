using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AvaliacaoPessoal
    {
        public int IdAvaliacao { get; set; }   
        public int FK_id_Atores{ get; set; }     
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int CCrimes { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Substancias { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Moradia { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Prevencao { get; set; }     
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int AssBasica { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Educacao { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Saude { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Ocupacao { get; set; }
        [Range(1, 5, ErrorMessage = "A pontuação deve estar entre 1 e 5.")]
        public int Lazer { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }    

        public Usuario Usuario { get; set; } = null!;
        public Atores Ator { get; set; } = null!;
    }
}