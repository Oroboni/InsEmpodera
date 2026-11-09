using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FonteInf
    {
        public int IdFonte { get; set; }        
        public int FkIdFicha { get; set; }       
        [StringLength(150)]
        public string Nome { get; set; } = null!;
        [Range(0, 3)]
        public int? Genero { get; set; }
        [Range(0, 130)]
        public int Idade { get; set; }
        [StringLength(255)]
        public string PapelSocial1 { get; set; } = null!;
        [StringLength(255)]
        public string PapelSocial2 { get; set; } = null!;
        [StringLength(255)]
        public string Interesse { get; set; } = null!;
        [StringLength(60)]
        public string Telefone { get; set; } = null!; 
        [StringLength(255)]
        public string Extra { get; set; } = null!;
        public int? Fk_Id_Ator { get; set; }

        public FichaPrimeiroContato Ficha { get; set; } = null!;
        public Atores? Ator { get; set; } = null!;
    }
}
