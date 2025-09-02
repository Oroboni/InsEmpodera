using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class FichaPrimeiroContato
    {
        public int IdFicha { get; set; }
        public int AtorId { get; set; }
        public string Localizacao { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string LContato { get; set; } = string.Empty;
        public string FonteDados { get; set; } = string.Empty;
        public string EstaFamiliar { get; set; } = string.Empty;
        public string EstruFamiliar { get; set; } = string.Empty;
        public int NFilhos { get; set; }
        public int NFilhas { get; set; }
        public int AEscola { get; set; }
        public string SLer { get; set; } = string.Empty;
        public string SCalc { get; set; } = string.Empty;
        public string SComp { get; set; } = string.Empty;
        public int QReabili { get; set; }
        public string LTrat { get; set; } = string.Empty;
        public string Coment { get; set; } = string.Empty;
        public string CPrimeiroContato { get; set; } = string.Empty;
        public string EParceiro { get; set; } = string.Empty;
        public string FPeloParceirto { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
    }
}