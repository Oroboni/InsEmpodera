using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Empodera.Data;
using Empodera.Models;
using Empodera.Controllers;

namespace Empodera.Services
{
    public class ImportExcelService
    {
        public byte[] ImportRelatorioAtores (IEnumerable<AtorRelatorioDto> atores)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Atores");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Nome";
            ws.Cell(1, 3).Value = "Gênero";
            ws.Cell(1, 4).Value = "Idade";
            ws.Cell(1, 5).Value = "Papel Social 1";
            ws.Cell(1, 6).Value = "Papel Social 2";
            ws.Cell(1, 7).Value = "Telefone";
            ws.Cell(1, 8).Value = "Extra";
            ws.Cell(1, 9).Value = "Data Criação";
            ws.Cell(1, 10).Value = "Data Modificação";
            ws.Cell(1, 11).Value = "Comunidades";

            var headerRange = ws.Range("A1:K1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int linha = 2;
            foreach (var a in atores)
            {
                ws.Cell(linha, 1).Value = a.IdAtor;
                ws.Cell(linha, 2).Value = a.Nome;
                ws.Cell(linha, 3).Value = a.Genero;
                ws.Cell(linha, 4).Value = a.Idade;
                ws.Cell(linha, 5).Value = a.PapelSocial1;
                ws.Cell(linha, 6).Value = a.PapelSocial2;
                ws.Cell(linha, 7).Value = a.Telefone;
                ws.Cell(linha, 8).Value = a.Extra;
                ws.Cell(linha, 9).Value = a.DtCriacao;
                ws.Cell(linha, 10).Value = a.DtModificacao;
                ws.Cell(linha, 11).Value = a.Comunidades;
                linha++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

    }
}
