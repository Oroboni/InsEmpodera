using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FonteInf
    {
        public Guid IdFonte { get; set; }        
        public Guid FkIdFicha { get; set; }       
        public string Nome { get; set; } = null!;
        public int? Genero { get; set; }
        public int Idade { get; set; }
        public string PapelSocial1 { get; set; } = null!;
        public string PapelSocial2 { get; set; } = null!;
        public string Interesse { get; set; } = null!;
        public string Telefone { get; set; } = null!; 
        public string Extra { get; set; } = null!;
        public Guid Fk_Id_Ator { get; set; }

        public FichaPrimeiroContato Ficha { get; set; } = null!;
        public Atores? Ator { get; set; } = null!;
    }
}
