using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Comunidade
    {
        public int IdComunidade { get; set; } 
        public string Nome { get; set; } = null!;
        public string Local { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Complemento { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string DescricaoAcessibilidade { get; set; } = null!;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }
        public string Ativo { get; set; } = "S";

        public Usuario Usuario { get; set; } = null!;
        public List<RedeRecursos> RedeRecursos { get; set; } = new();
        public List<AtorComunidade> AtorComunidades { get; set; } = new();
        public List<DiarioCampo> DiarioCampos { get; set; } = new();
        public List<Vulnerabilidade> Vulnerabilidades { get; set; } = new();
        public List<Atividades> Atividades { get; set; } = new();
    }
}