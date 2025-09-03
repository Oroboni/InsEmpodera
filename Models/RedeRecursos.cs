using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeRecurso
    {
        [Key]
        public int IdRede { get; set; }
        public int AtorId { get; set; }
        public int ComunidadeId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Dispositivo { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}