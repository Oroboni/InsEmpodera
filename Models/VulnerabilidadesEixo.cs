using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class VulnerabilidadesEixo
    {
        public int IdVEixo { get; set; }          // Id_V_Eixo
        public int FkIdEixo { get; set; }         // Fk_Id_Eixo
        public int FkIdVulnerabilidade { get; set; } // Fk_Id_Vulnerabilidades

        public Eixo Eixo { get; set; } = null!;
        public Vulnerabilidade Vulnerabilidade { get; set; } = null!;
    }
}