using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtorComunidade
    {
        public Guid IdAtorComunidade { get; set; }   
        public Guid FkIdComunidade { get; set; }     
        public Guid FK_id_Atores{ get; set; }           
        public int? TipoRelacionamento { get; set; }

        public Comunidade Comunidade { get; set; } = null!;
        public Atores Ator { get; set; } = null!;
    }
}
