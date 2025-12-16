using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Empodera.Models
{
    public class FichaPrimeiroContato
    {
        public int IdFicha { get; set; }
        public int FKidAtores { get; set; }

        public string? Endereco { get; set; }
        public string? Complemento { get; set; }
        public string? Emprego { get; set; }

        public string? CEstabeleceu { get; set; }

        public string? NovoParceiro { get; set; }

        public string? FornecidoParceiro { get; set; }

        public string? Telefone { get; set; }
        public string? LContato { get; set; }
        public string? FonteDados { get; set; }
        public string? EstaFamiliar { get; set; }
        public string? EstruFamiliar { get; set; }

        public int? NFIlhos { get; set; }
        public int? NFilhas { get; set; }
        public int? AEscolar { get; set; }
        public string? Status { get; set; } = "EmProgresso";

        public string? SLer { get; set; }

        public string? SCalc { get; set; }

        public string? SComp { get; set; }

        public int? QReabili { get; set; }

        public string? LTrat { get; set; }

        public string? Coment { get; set; }

        [DataType(DataType.Date)]
        public DateTime DtContato { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HoraContato { get; set; }

        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int? FkIdComunidade { get; set; }
        [ForeignKey("FkIdComunidade")]
        public virtual Comunidade? Comunidade { get; set; }
        public int FkIdUsuario { get; set; }

        [ForeignKey("FKidAtores")]
        public virtual Atores Ator { get; set; } = null!;

        [ForeignKey("FkIdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;

        public virtual List<FonteInf> Fontes { get; set; } = new();
        public ICollection<FichaCondicoes>? FichaCondicoes { get; set; }
        public ICollection<FichaPeticoes>? FichaPeticoes { get; set; }
        public ICollection<FichaResp>? FichaRespostas { get; set; }
        public ICollection<FichaResult>? FichaResultados { get; set; }

    }
}