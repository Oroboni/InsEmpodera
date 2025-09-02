using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class AvaliacaoPessoal
    {
        public int IdAvaliacao { get; set; }
        public int AtorId { get; set; }
        public DateTime Data { get; set; }
        public string Indicador { get; set; } = string.Empty;
        public int Nota { get; set; }
    }
}