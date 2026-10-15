using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class DAAtores
    {
        public Guid Id { get; set; }             
        public Guid FkIdDDacoes { get; set; }    
        public Guid FK_id_Atores { get; set; }     

        public DiarioDAcoes DiarioDAcoes { get; set; } = null!;
        public Atores Ator { get; set; } = null!;
    }
}
