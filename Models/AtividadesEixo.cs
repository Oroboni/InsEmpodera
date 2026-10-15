using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtividadesEixo
    {
        public Guid IdAEixo { get; set; }       
        public Guid FkIdEixo { get; set; }     
        public Guid FkIdAtividade { get; set; }   

        public Eixo Eixo { get; set; } = null!;
        public Atividades Atividades { get; set; } = null!;
    }
}
