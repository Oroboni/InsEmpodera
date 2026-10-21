using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Vulnerabilidade
    {
        public int IdVulnerabilidade { get; set; } 
        [StringLength(150)]
        public string Nome { get; set; } = null!;
        [StringLength(255)]
        public string Localizacao { get; set; } = null!;
        [StringLength(2000)]
        public string Servicos { get; set; } = null!;
        public int FkIdComunidade { get; set; } 
        
        public Comunidade Comunidade { get; set; } = null!;
        public List<VulnerabilidadesEixo> VulnerabilidadesEixos { get; set; } = new();
    }
}
