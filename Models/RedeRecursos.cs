using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeRecursos
    {
        public int IdRede { get; set; }         
        public int FKidAtores{ get; set; }     
        public int FkIdComunidade { get; set; } 
        public string Tipo { get; set; } = null!;
        public string Dispositivo { get; set; } = null!;
        public string Servicos { get; set; } = null!;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }   

        public Atores Ator { get; set; } = null!;
        public Comunidade Comunidade { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public List<RedeEixo> RedeEixos { get; set; } = new();
    }
}