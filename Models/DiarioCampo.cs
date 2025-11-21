using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioCampo
    {
        [Key]
        public int Id { get; set; }

        // 🔥 Associação obrigatória com o Ator
        public int AtorId { get; set; }
        public Ator? Ator { get; set; }

        // 🔥 Associação com comunidade
        public int ComunidadeId { get; set; }
        public Comunidade? Comunidade { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? UltimaAtualizacao { get; set; }

        public string? Descricao { get; set; }
        public string? CEP { get; set; }
        public string? Endereco { get; set; }

        public string? CriadoPor { get; set; }
        public string? AtualizadoPor { get; set; }

        public ICollection<AnexosDiario> Anexos { get; set; } = new List<AnexosDiario>();
        public ICollection<DiarioEixo> Eixos { get; set; } = new List<DiarioEixo>();
        public ICollection<DiarioAcoes> DiarioAcoes { get; set; } = new List<DiarioAcoes>();
    }
}
