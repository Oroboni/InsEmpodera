using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Permissoes
    {
        public Guid IdPermissoes { get; set; }
        public Guid FkIdPerfil { get; set; }
        public string Modulo { get; set; } = null!;
        public string PodeListar { get; set; } = null!;
        public string PodeDetalhar { get; set; } = null!;
        public string PodeCriar { get; set; } = null!;
        public string PodeAtualizar { get; set; } = null!;
        public string PodeDeletar { get; set; } = null!;

        public Perfil Perfil { get; set; } = null!;
    }
}
