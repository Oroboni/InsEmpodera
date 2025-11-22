using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FonteInf
    {
        public int IdFonte { get; set; }        
        public int FkIdFicha { get; set; }       
        public string Nome { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public int Idade { get; set; }
        public string PapelSocial1 { get; set; } = null!;
        public string PapelSocial2 { get; set; } = null!;
        public string Telefone { get; set; } = null!; 
        public string Extra { get; set; } = null!;

        public FichaPrimeiroContato Ficha { get; set; } = null!;
    }
}