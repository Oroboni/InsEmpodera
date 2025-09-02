using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class Permissao
    {
        public int IdPermissoes { get; set; }
        public int PerfilAcessoId { get; set; }
        public string PermissaoNome { get; set; } = string.Empty;
    }
}