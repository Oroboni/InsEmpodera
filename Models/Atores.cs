using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Atores
    {
        public int IdAtores { get; set; }   
        public string Nome { get; set; } = null!;
        public string? Genero { get; set; }
        public int? Idade { get; set; }
        public string? PapelSocial1 { get; set; }
        public string? PapelSocial2 { get; set; }
        public string? Telefone { get; set; }
        public bool DaEquipe { get; set; } = false;
        public bool Rope { get; set; } = false;
        public bool Lopiniao { get; set; } = false;
        public bool Mcomunidade { get; set; } = false;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public string Ativo { get; set; } = "S";
        public int FkIdUsuario { get; set; }
        public int? FkIdUsuarioM { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public List<RedeRecursos> Redes { get; set; } = new();
        public List<AtorComunidade> Comunidades { get; set; } = new();
        public List<DAAtores> DAAtores { get; set; } = new();
        public List<AcoesAtores> AcoesAtores { get; set; } = new();
        public List<FonteInf> FonteInfos { get; set; } = new();
        public List<FichaPrimeiroContato> FichasPrimeiroContato { get; set; } = new();
        public ICollection<AvaliacaoPessoal> Avaliacoes { get; set; } = new List<AvaliacaoPessoal>();

    }
}