using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DiarioCampo
    {
        public int IdDCampo { get; set; }       
        public int FkIdComunidade { get; set; }  
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = null!;
        public string Localizacao { get; set; } = null!;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public string Foto { get; set; } = null!;
        public int FkIdUsuario { get; set; }     

        public Comunidade Comunidade { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public List<DiarioAcoes> DiarioAcoes { get; set; } = new();
        public List<DiarioDAcoes> DiarioDAcoes { get; set; } = new();
        public List<DiarioEixo> DiarioEixos { get; set; } = new();
        public List<AnexosDiario> Anexos { get; set; } = new();
    }
}