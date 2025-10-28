using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Empodera.Models;

namespace Empodera.Services
{
    public class RelatorioExcelService
    {
        public byte[] GerarRelatorioExcel(IEnumerable<Comunidade> comunidades)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Comunidades");

            // Cabeçalhos
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Nome";
            ws.Cell(1, 3).Value = "Local";
            ws.Cell(1, 4).Value = "Status";
            ws.Cell(1, 5).Value = "Complemento";
            ws.Cell(1, 6).Value = "Descrição";
            ws.Cell(1, 7).Value = "Acessibilidade";
            ws.Cell(1, 8).Value = "Data Criação";
            ws.Cell(1, 9).Value = "Data Modificação";

            var headerRange = ws.Range("A1:I1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int linha = 2;
            foreach (var c in comunidades)
            {
                ws.Cell(linha, 1).Value = c.IdComunidade;
                ws.Cell(linha, 2).Value = c.Nome;
                ws.Cell(linha, 3).Value = c.Local;
                ws.Cell(linha, 4).Value = c.Status;
                ws.Cell(linha, 5).Value = c.Complemento;
                ws.Cell(linha, 6).Value = c.Descricao;
                ws.Cell(linha, 7).Value = c.DescricaoAcessibilidade;
                ws.Cell(linha, 8).Value = c.DtCriacao;
                ws.Cell(linha, 9).Value = c.DtModificacao;
                linha++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
