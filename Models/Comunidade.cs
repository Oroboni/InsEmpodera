using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class Comunidade
    {
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