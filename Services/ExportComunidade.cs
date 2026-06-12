using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Empodera.Models;

namespace Empodera.Services
{
    public class ExportComunidade
    {
        public byte[] GerarRelatorioComunidade(
            Comunidade?[] comunidades,
            Comunidade comunidade,
            IEnumerable<Atores> atores)
        {
            using var workbook = new XLWorkbook();

            // =====================================================
            // ABA COMUNIDADE
            // =====================================================

            var ws = workbook.Worksheets.Add("Comunidade");

            ws.Cell(1, 1).Value = "Nome";
            ws.Cell(1, 2).Value = comunidade.Nome;

            ws.Cell(2, 1).Value = "Local";
            ws.Cell(2, 2).Value = comunidade.Local;

            ws.Cell(3, 1).Value = "Complemento";
            ws.Cell(3, 2).Value = comunidade.Complemento;

            ws.Cell(4, 1).Value = "Descrição";
            ws.Cell(4, 2).Value = comunidade.Descricao;

            ws.Cell(5, 1).Value =
                "Descrição Acessibilidade";

            ws.Cell(5, 2).Value =
                comunidade.Descricao_Acessibilidade;

            ws.Cell(6, 1).Value = "Data Criação";

            ws.Cell(6, 2).Value =
                comunidade.Dt_Criacao.ToString("dd/MM/yyyy");

            var estiloComunidade =
                ws.Range("A1:A6");

            estiloComunidade.Style.Font.Bold = true;

            estiloComunidade.Style.Fill.BackgroundColor =
                XLColor.LightGreen;

            // =====================================================
            // ABA RECURSOS
            // =====================================================

            var wsRecursos =
                workbook.Worksheets.Add("Recursos");

            wsRecursos.Cell(1, 1).Value = "Nome";
            wsRecursos.Cell(1, 2).Value = "Tipo";
            wsRecursos.Cell(1, 3).Value = "Serviços";
            wsRecursos.Cell(1, 4).Value = "Localização";

            var headerRecursos =
                wsRecursos.Range("A1:D1");

            headerRecursos.Style.Font.Bold = true;

            headerRecursos.Style.Fill.BackgroundColor =
                XLColor.Orange;

            int linhaRecurso = 2;

            if (comunidade.RedeRecursos != null &&
                comunidade.RedeRecursos.Any())
            {
                foreach (var rede in comunidade.RedeRecursos)
                {
                    wsRecursos.Cell(linhaRecurso, 1).Value =
                        rede.Nome;

                    wsRecursos.Cell(linhaRecurso, 2).Value =
                        rede.Tipo;

                    wsRecursos.Cell(linhaRecurso, 3).Value =
                        rede.Servicos;

                    wsRecursos.Cell(linhaRecurso, 4).Value =
                        rede.Localizacao;

                    linhaRecurso++;
                }
            }

            // =====================================================
            // ABA ATIVIDADES
            // =====================================================

            var wsAtividades =
                workbook.Worksheets.Add("Atividades");

            wsAtividades.Cell(1, 1).Value = "Nome";
            wsAtividades.Cell(1, 2).Value = "Descrição";

            var headerAtividades =
                wsAtividades.Range("A1:B1");

            headerAtividades.Style.Font.Bold = true;

            headerAtividades.Style.Fill.BackgroundColor =
                XLColor.LightYellow;

            int linhaAtividade = 2;

            if (comunidade.Atividades != null &&
                comunidade.Atividades.Any())
            {
                foreach (var atividade in comunidade.Atividades)
                {
                    wsAtividades.Cell(linhaAtividade, 1).Value =
                        atividade.Nome;

                    wsAtividades.Cell(linhaAtividade, 2).Value =
                        atividade.Descricao;

                    linhaAtividade++;
                }
            }

            // =====================================================
            // ABA ATORES
            // =====================================================

            var wsAtores = workbook.Worksheets.Add("Atores");

            wsAtores.Cell(1, 1).Value = "Nome";
            wsAtores.Cell(1, 2).Value = "Gênero";
            wsAtores.Cell(1, 3).Value = "Idade";
            wsAtores.Cell(1, 4).Value = "Papel Social 1";
            wsAtores.Cell(1, 5).Value = "Papel Social 2";
            wsAtores.Cell(1, 6).Value = "Telefone";
            wsAtores.Cell(1, 7).Value = "Da Equipe";
            wsAtores.Cell(1, 8).Value = "ROPE";
            wsAtores.Cell(1, 9).Value = "Lidera Opinião";
            wsAtores.Cell(1, 10).Value = "Mobiliza Comunidade";

            int linhaAtor = 2;

            foreach (var ator in atores)
            {
                wsAtores.Cell(linhaAtor, 1).Value = ator.Nome;
                wsAtores.Cell(linhaAtor, 2).Value = ator.Genero;
                wsAtores.Cell(linhaAtor, 3).Value = ator.Idade;
                wsAtores.Cell(linhaAtor, 4).Value = ator.PapelSocial1;
                wsAtores.Cell(linhaAtor, 5).Value = ator.PapelSocial2;
                wsAtores.Cell(linhaAtor, 6).Value = ator.Telefone;
                wsAtores.Cell(linhaAtor, 7).Value = ator.DaEquipe ? "Sim" : "Não";
                wsAtores.Cell(linhaAtor, 8).Value = ator.Rope ? "Sim" : "Não";
                wsAtores.Cell(linhaAtor, 9).Value = ator.Lopiniao ? "Sim" : "Não";
                wsAtores.Cell(linhaAtor, 10).Value = ator.Mcomunidade ? "Sim" : "Não";

                linhaAtor++;
            }

            var cabecalho = wsAtores.Range("A1:J1");

            cabecalho.Style.Font.Bold = true;
            cabecalho.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Ajusta largura automaticamente
            wsAtores.Columns().AdjustToContents();

            // =====================================================
            // AJUSTE FINAL
            // =====================================================

            ws.Columns().AdjustToContents();

            wsRecursos.Columns().AdjustToContents();

            wsAtividades.Columns().AdjustToContents();

            wsAtores.Columns().AdjustToContents();

            using var ms = new MemoryStream();

            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}