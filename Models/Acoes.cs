using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Acoes
    {
        [Key]
        public int IdAcoes { get; set; }
        public int Quantidade { get; set; }
        public int AtividadeId { get; set; }
        public string? Nome { get; set; }

        public ICollection<AcoesAtores> AcoesAtores { get; set; } = new List<AcoesAtores>();
        public ICollection<DiarioAcoes> DiarioAcoes { get; set; } = new List<DiarioAcoes>();
        public Atividade? Atividade { get; set; }

    }
}