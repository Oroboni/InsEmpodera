using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Atividades
    {
        public int IdAtividade { get; set; }     
        public string Nome { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string? Foto { get; set; } = null!;
        public int FkIdComunidade { get; set; } 
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public Comunidade Comunidade { get; set; } = null!;
        public List<AtividadesEixo> AtividadesEixos { get; set; } = new();
        public List<Acoes> Acoes { get; set; } = new();
    }
}