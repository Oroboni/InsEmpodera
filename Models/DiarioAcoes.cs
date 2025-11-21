using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
   public class DiarioAcoes
    {
        [Key]
        public int Id { get; set; }

        public int DiarioId { get; set; }
        public DiarioCampo? Diario { get; set; }

        public int AcoesId { get; set; }
        public Acoes? Acoes { get; set; }
    }
}