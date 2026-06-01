using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeRecursos
    {
        public int Id_Rede { get; set; }         
        public int? FK_id_Atores{ get; set; }     
        public int FkIdComunidade { get; set; } 
        public string? Nome { get; set; }
        public string Tipo { get; set; } = null!;
        public string? Dispositivo { get; set; }
        public string? Localizacao { get; set; }
        public string? Servicos { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }   

        public Atores Ator { get; set; } = null!;
        public Comunidade Comunidade { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public List<RedeEixo> RedeEixos { get; set; } = new();
    }
}