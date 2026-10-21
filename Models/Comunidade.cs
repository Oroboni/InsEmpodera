using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Comunidade
    {
        public int Id_Comunidade { get; set; }
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Local { get; set; } = string.Empty;
        [StringLength(500)]
        public string? LocalMapa { get; set; } = string.Empty;
        [StringLength(500)]
        public string? LocalSecundario { get; set; } = string.Empty;
        [StringLength(500)]
        public string? LocalMapaSecundario { get; set; } = string.Empty;
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;
        [StringLength(255)]
        public string? Complemento { get; set; } = string.Empty;
        public string? Descricao { get; set; } = string.Empty;
        public string? Descricao_Acessibilidade { get; set; } = string.Empty;
        public DateTime Dt_Criacao { get; set; }
        public DateTime? Dt_Modificacao { get; set; }
        public int FK_Id_Usuario { get; set; }
        public int? FK_Id_UsuarioM { get; set; }
        public string Ativo { get; set; } = "S";
        public Usuario Usuario { get; set; } = null!;
        public List<RedeRecursos> RedeRecursos { get; set; } = new();
        public List<AtorComunidade> AtorComunidades { get; set; } = new();
        public List<DiarioCampo> DiarioCampos { get; set; } = new();
        public List<Vulnerabilidade> Vulnerabilidades { get; set; } = new();
        public List<Atividades> Atividades { get; set; } = new();
        public virtual ICollection<FichaPrimeiroContato> FichasPrimeiroContato { get; set; }
   = new List<FichaPrimeiroContato>();
    }
}
