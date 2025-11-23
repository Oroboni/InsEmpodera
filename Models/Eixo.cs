using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Eixo
    {
        public int IdEixo { get; set; }       
        public string Nome { get; set; } = null!;

        public List<RedeEixo> RedeEixos { get; set; } = new();
        public List<DetalhesEixos> DetalhesEixos { get; set; } = new();
        public List<DiarioEixo> DiarioEixos { get; set; } = new();
        public List<VulnerabilidadesEixo> VulnerabilidadesEixos { get; set; } = new();
        public List<AtividadesEixo> AtividadesEixo { get; set; } = new();
    }
}