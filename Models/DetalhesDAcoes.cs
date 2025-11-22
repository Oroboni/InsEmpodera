using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DetalhesDAcoes
    {
        public int Id { get; set; }             
        public string Nome { get; set; } = null!;
        public int FkIdDDacoes { get; set; }    
        public DiarioDAcoes DiarioDAcoes { get; set; } = null!;
        public List<DetalhesEixos> DetalhesEixos { get; set; } = new();
    }
}