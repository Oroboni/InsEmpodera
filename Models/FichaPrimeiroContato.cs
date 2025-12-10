using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Empodera.Models
{
    public class FichaPrimeiroContato
    {
        [Key]
        public int IdFicha { get; set; }

        [Display(Name = "Ator")]
        [Required(ErrorMessage = "O campo Ator é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione um ator válido.")]
        public int FKidAtores { get; set; }

        public string? Endereco { get; set; }
        public string? Complemento { get; set; }
        public string? Emprego { get; set; }

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string CEstabeleceu { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string NovoParceiro { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string FornecidoParceiro { get; set; } = null!;
        
        public string? Telefone { get; set; }
        public string? LContato { get; set; }
        public string? FonteDados { get; set; }
        public string? EstaFamiliar { get; set; }
        public string? EstruFamiliar { get; set; }
        
        public int NFIlhos { get; set; }
        public int NFilhas { get; set; }
        public int AEscolar { get; set; }

        public StatusFicha Status { get; set; } = StatusFicha.EmProgresso;

        public string SLer { get; set; } = null!;
        
        public string SCalc { get; set; } = null!;
        
        public string SComp { get; set; } = null!;
        
        public int QReabili { get; set; }
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string LTrat { get; set; } = null!;
        
        public string Coment { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        [Display(Name = "Data do Contato")]
        [DataType(DataType.Date)]
        public DateTime DtContato { get; set; }
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        [Display(Name = "Hora do Contato")]
        [DataType(DataType.Time)]
        public DateTime HoraContato { get; set; }
        
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        
        public int FkIdUsuario { get; set; }

        // Propriedades de navegação
        [ForeignKey("FKidAtores")]
        public virtual Atores Ator { get; set; } = null!;
        
        [ForeignKey("FkIdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;

        // Coleções relacionadas
        public virtual List<FonteInf> Fontes { get; set; } = new();
        public virtual List<FichaCondicoes> Condicoes { get; set; } = new();
        public virtual List<FichaPeticoes> Peticoes { get; set; } = new();
        public virtual List<FichaResp> Respostas { get; set; } = new();
        public virtual List<FichaResult> Resultados { get; set; } = new();

        public virtual ICollection<Ficha1oContatoComunidade> FichaComunidades { get; set; } = new List<Ficha1oContatoComunidade>();
        
        [NotMapped]
        public Comunidade? ComunidadePrincipal => FichaComunidades.FirstOrDefault()?.Comunidade;
    }

    public enum StatusFicha
    {
        [Display(Name = "Em Progresso")]
        EmProgresso = 1,
        
        [Display(Name = "Concluída")]
        Concluida = 2,
        
        [Display(Name = "Abandonada")]
        Abandonada = 3
    }
}