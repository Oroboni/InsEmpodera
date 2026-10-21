using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        [StringLength(150)]
        public string Nome { get; set; } = null!;
        [StringLength(255)]
        public string Senha { get; set; } = null!;
        [StringLength(255)]
        public string? Foto { get; set; } = null!;
        [StringLength(254)]
        public string Email { get; set; } = null!;
        [StringLength(100)]
        public string Ocupacao { get; set; } = null!;
        [Range(0, 3)]
        public int? Genero { get; set; }
        public DateTime DtNascimento { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime? DtAtualizacao { get; set; }
        public string Ativo { get; set; } = "S";
        [EnumDataType(typeof(IdiomaPreferido))]
        public IdiomaPreferido IdiomaPreferido { get; set; } = IdiomaPreferido.Default;
        public int FkIdPerfil { get; set; }

        // Campos mantidos pelo ASP.NET Core Identity através do EmpoderaUserStore.
        // Os dados de domínio e o perfil existente continuam sendo a fonte das
        // permissões funcionais do sistema.
        [StringLength(254)]
        public string UserName { get; set; } = null!;
        [StringLength(254)]
        public string NormalizedUserName { get; set; } = null!;
        [StringLength(254)]
        public string NormalizedEmail { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        [StringLength(100)]
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
        [StringLength(100)]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");
        [StringLength(60)]
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; } = true;
        public int AccessFailedCount { get; set; }
        
        public Perfil Perfil { get; set; } = null!;
        public List<Comunidade> Comunidades { get; set; } = new();
        public List<Atores> Atores { get; set; } = new();
        public List<RedeRecursos> RedeRecursos { get; set; } = new();
        public List<DiarioCampo> DiarioCampos { get; set; } = new();
        public List<AvaliacaoPessoal> Avaliacoes { get; set; } = new();
        public List<FichaPrimeiroContato> FichasPrimeiroContato { get; set; } = new();
        public List<Atividades> Atividades { get; set; } = new();
        public List<DiarioProcessoPessoal> DiariosProcessoPessoal { get; set; } = new();
    }
}
