using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class PerfilAcesso
    {
        [Key]
        public int IdPAcesso { get; set; }

        [Required(ErrorMessage = "O nome do perfil é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        public DateTime DtCriacao { get; set; }
        
        public DateTime DtModificacao { get; set; }

        // public List<Permissao> Permissoes { get; set; } // (Descomente quando for implementar)
    }
}