using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtorComunidade
    {
        public int IdAtorComunidade { get; set; }   
        public int FkIdComunidade { get; set; }     
        public int FKidAtores{ get; set; }           

        public Comunidade Comunidade { get; set; } = null!;
        public Atores Ator { get; set; } = null!;
    }
}