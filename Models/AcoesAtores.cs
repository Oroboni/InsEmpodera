using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
     public class AcoesAtores
    {
        public int IdAAtores { get; set; }      
        public int FKidAtores { get; set; }     
        public int FkIdAcoes { get; set; }     

        public Atores Ator { get; set; } = null!;
        public Acoes Acoes { get; set; } = null!;
    }
}