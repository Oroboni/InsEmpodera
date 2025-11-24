using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class Perfil
    {
        public int IdPerfil { get; set; }
        public int FkIdUsuario { get; set; }
        public string Nome { get; set; } = null!;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<Usuario> Usuarios { get; set; } = new();
        public List<Permissoes> Permissoes { get; set; } = new();
    }
}