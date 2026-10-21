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
        [StringLength(150)]
        public string? Nome { get; set; }
        [StringLength(50)]
        public string Tipo { get; set; } = null!;
        [StringLength(150)]
        public string? Dispositivo { get; set; }
        [StringLength(255)]
        public string? Localizacao { get; set; }
        public string? Servicos { get; set; }
        public DateTime DtCriacao { get; set; }
        public DateTime? DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }   

        public Atores Ator { get; set; } = null!;
        public Comunidade Comunidade { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public List<RedeEixo> RedeEixos { get; set; } = new();
    }
}
