using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
     public class AcoesAtores
    {
        public Guid IdAAtores { get; set; }      
        public Guid FK_id_Atores { get; set; }     
        public Guid FkIdAcoes { get; set; }     

        public Atores Ator { get; set; } = null!;
        public Acoes Acoes { get; set; } = null!;
    }
}
