using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class FichaPrimeiroContato
    {
        public int IdFicha { get; set; }

        [Display(Name = "Ator")]
        [Required(ErrorMessage = "O campo Ator é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione um ator válido.")]
        public int FKidAtores { get; set; }

        // Sugestão: Adicione [Required] nos outros campos de texto obrigatórios também
        [Required(ErrorMessage = "O Endereço é obrigatório.")]
        public string Endereco { get; set; } = null!;

        public string? Complemento { get; set; } = null!;

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string Emprego { get; set; } = null!;

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string CEstabeleceu { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string NovoParceiro { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string FornecidoParceiro { get; set; } = null!;
        public string? Telefone { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string LContato { get; set; } = null!;
        
        [Required(ErrorMessage = "Campo obrigatório.")]
        public string FonteDados { get; set; } = null!;
        
        public string EstaFamiliar { get; set; } = null!;
        
        public string EstruFamiliar { get; set; } = null!;
        public int NFIlhos { get; set; }
        public int NFilhas { get; set; }
        public int AEscolar { get; set; }

        public StatusFicha Status { get; set; } = StatusFicha.EmProgresso;

        public string SLer { get; set; } = null!;
        public string SCalc { get; set; } = null!;
        public string SComp { get; set; } = null!;
        public int QReabili { get; set; }
        public string LTrat { get; set; } = null!;
        public string Coment { get; set; } = null!;
        public DateTime DtContato { get; set; }
        public DateTime HoraContato { get; set; }
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

    public enum StatusFicha
    {
        EmProgresso = 1,
        Concluida = 2,
        Abandonada = 3
    }
}