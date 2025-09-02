using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class Vulnerabilidade
    {
        public int IdVulnerabilidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public int ComunidadeId { get; set; }
    }
}