using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DAAtores
    {
        public int Id { get; set; }             
        public int FkIdDDacoes { get; set; }    
        public int FKidAtores { get; set; }     

        public DiarioDAcoes DiarioDAcoes { get; set; } = null!;
        public Atores Ator { get; set; } = null!;
    }
}