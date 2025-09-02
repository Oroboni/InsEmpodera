using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class DiarioCampo
    {
        public int IdDCampo { get; set; }
        public int ComunidadeId { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}