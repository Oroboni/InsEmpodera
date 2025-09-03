using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Vulnerabilidade
    {
        [Key]
        public int IdVulnerabilidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public int ComunidadeId { get; set; }
    }
}