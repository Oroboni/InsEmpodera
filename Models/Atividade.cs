using System;
using System.Collections.Generic;

namespace entre.Models
{
    public class Atividade
    {
        public int IdAtividade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }
}