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
        public int FK_id_Atores { get; set; }

        [StringLength(255)]
        public string? Endereco { get; set; }
        [StringLength(255)]
        public string? Complemento { get; set; }
        [StringLength(150)]
        public string? Emprego { get; set; }

        [StringLength(100)]
        public string? CEstabeleceu { get; set; }

        public string? NovoParceiro { get; set; }

        public string? FornecidoParceiro { get; set; }

        [StringLength(60)]
        public string? Telefone { get; set; }
        [StringLength(150)]
        public string? LContato { get; set; }
        [StringLength(150)]
        public string? FonteDados { get; set; }
        [StringLength(100)]
        public string? EstaFamiliar { get; set; }
        [StringLength(255)]
        public string? EstruFamiliar { get; set; }

        [Range(0, 30)]
        public int? NFIlhos { get; set; }
        [Range(0, 30)]
        public int? NFilhas { get; set; }
        [Range(0, 30)]
        public int? AEscolar { get; set; }
        [StringLength(20)]
        public string? Status { get; set; } = "EmProgresso";

        public string? SLer { get; set; }

        public string? SCalc { get; set; }

        public string? SComp { get; set; }

        [Range(0, 100)]
        public int? QReabili { get; set; }

        [StringLength(150)]
        public string? LTrat { get; set; }

        [StringLength(2000)]
        public string? Coment { get; set; }

        [DataType(DataType.Date)]
        public DateTime DtContato { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HoraContato { get; set; }

        public DateTime DtCriacao { get; set; }
        public DateTime? DtModificacao { get; set; }
        public int? FkIdComunidade { get; set; }
        [ForeignKey("FkIdComunidade")]
        public virtual Comunidade? Comunidade { get; set; }
        public int FkIdUsuario { get; set; }

        [ForeignKey("FK_id_Atores")]
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
