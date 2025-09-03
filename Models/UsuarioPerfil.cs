using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Empodera.Models
{
    public class UsuarioPerfil
    {
        [Key]
        public int IdPUsuario { get; set; }
        public int UsuarioId { get; set; }
        public int PerfilAcessoId { get; set; }
    }
}