using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Foto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ocupacao { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime DtNascimento { get; set; }
        public int NivelPermissao { get; set; }
        public DateTime DtCriacao { get; set; }
    }
}