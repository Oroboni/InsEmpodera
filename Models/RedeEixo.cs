using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeEixo
    {
        [Key]
        public int IdRedeEixo { get; set; }
        public int RedeId { get; set; }
        public int EixoId { get; set; }
    }
}