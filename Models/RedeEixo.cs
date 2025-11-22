using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class RedeEixo
    {
        public int IdRedeEixo { get; set; }    // Id_Rede_Eixo
        public int FkIdRede { get; set; }      // Fk_Id_Rede
        public int FkIdEixo { get; set; }      // Fk_Id_Eixo

        public RedeRecursos RedeRecursos { get; set; } = null!;
        public Eixo Eixo { get; set; } = null!;
    }
}