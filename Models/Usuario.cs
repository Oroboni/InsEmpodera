using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = null!;
        public string Senha { get; set; } = null!;
        public string? Foto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Ocupacao { get; set; } = null!;
        public string? Genero { get; set; } = null!;
        public DateTime DtNascimento { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime? DtAtualizacao { get; set; }
        public string Ativo { get; set; } = "S";
        public int FkIdPerfil { get; set; }
        
        public Perfil Perfil { get; set; } = null!;
        public List<Comunidade> Comunidades { get; set; } = new();
        public List<Atores> Atores { get; set; } = new();
        public List<RedeRecursos> RedeRecursos { get; set; } = new();
        public List<DiarioCampo> DiarioCampos { get; set; } = new();
        public List<AvaliacaoPessoal> Avaliacoes { get; set; } = new();
        public List<FichaPrimeiroContato> FichasPrimeiroContato { get; set; } = new();
        public List<Atividades> Atividades { get; set; } = new();
    }
}