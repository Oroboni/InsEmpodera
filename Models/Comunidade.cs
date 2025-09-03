using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Comunidade
    {
        [Key]
        public int IdComunidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string DescricaoAcessibilidade { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}