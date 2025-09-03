using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtividadesEixo
    {
        [Key]
        public int IdAEixo { get; set; }
        public int EixoId { get; set; }
        public int AtividadeId { get; set; }
    }
}