using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Permissao
    {
        [Key]
        public int IdPermissoes { get; set; }
        public int PerfilAcessoId { get; set; }
        public string PermissaoNome { get; set; } = string.Empty;
    }
}