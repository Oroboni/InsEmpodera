using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaPrimeiroContato
    {
        public int IdFicha { get; set; }       
        public int FKidAtores{ get; set; }       
        public string Localizacao { get; set; } = null!;
        public DateTime Data { get; set; }
        public string LContato { get; set; } = null!;
        public string FonteDados { get; set; } = null!;
        public string EstaFamiliar { get; set; } = null!;
        public string EstruFamiliar { get; set; } = null!;
        public int NFIlhos { get; set; }        
        public int NFilhas { get; set; }         
        public int AEscolar { get; set; }       
        public string SLer { get; set; } = null!;
        public string SCalc { get; set; } = null!;
        public string SComp { get; set; } = null!;
        public int QReabili { get; set; }
        public string LTrat { get; set; } = null!;
        public string Coment { get; set; } = null!;
        public string CPrimeiroContato { get; set; } = null!;
        public string EParceiro { get; set; } = null!;
        public string FPeloParceirto { get; set; } = null!;
        public DateTime DtContato { get; set; } 
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }     

        public Atores Ator { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public List<FonteInf> Fontes { get; set; } = new();
        public List<FichaCondicoes> Condicoes { get; set; } = new();
        public List<FichaPeticoes> Peticoes { get; set; } = new();
        public List<FichaResp> Respostas { get; set; } = new();
        public List<FichaResult> Resultados { get; set; } = new();
    }
}