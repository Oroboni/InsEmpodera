using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Empodera.Data;
using Empodera.Models;
using InsEmpodera.Controllers;

namespace Empodera.Services
{
    public class AtorRelatorioDto
    {
        public int IdAtor { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string PapelSocial1 { get; set; } = string.Empty;
        public string PapelSocial2 { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Extra { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public string Comunidades { get; set; } = string.Empty; // nomes concatenados
    }
}
