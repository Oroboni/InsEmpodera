using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class AtividadesEixo
    {
        public int IdAEixo { get; set; }       
        public int FkIdEixo { get; set; }     
        public int FkIdAtividade { get; set; }   

        public Eixo Eixo { get; set; } = null!;
        public Atividades Atividades { get; set; } = null!;
    }
}