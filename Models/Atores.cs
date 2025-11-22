using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Atores
    {
        public int IdAtores { get; set; }   
        public string Nome { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public DateTime DtNascimento { get; set; }
        public string PapelSocial1 { get; set; } = null!;
        public string PapelSocial2 { get; set; } = null!;
        public string Telefone { get; set; } = null!; 
        public string Extra { get; set; } = null!;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public string Status { get; set; } = null!;
        public string MotivoStatus { get; set; } = null!;
        public int FkIdUsuario { get; set; }    // FK_Id_Usuario

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