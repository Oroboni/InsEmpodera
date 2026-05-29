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

            var wsAtores =
                workbook.Worksheets.Add("Atores");

            int linhaAtor = 1;

            foreach (var ator in atores)
            {
                wsAtores.Cell(linhaAtor, 1).Value =
                    "Nome";

                wsAtores.Cell(linhaAtor, 2).Value =
                    ator.Nome;

                linhaAtor++;

                wsAtores.Cell(linhaAtor, 1).Value =
                    "Gênero";

                wsAtores.Cell(linhaAtor, 2).Value =
                    ator.Genero;

                linhaAtor++;

                wsAtores.Cell(linhaAtor, 1).Value =
                    "Idade";

                wsAtores.Cell(linhaAtor, 2).Value =
                    ator.Idade;

                linhaAtor++;

                wsAtores.Cell(linhaAtor, 1).Value =
                    "Telefone";

                wsAtores.Cell(linhaAtor, 2).Value =
                    ator.Telefone;

                linhaAtor++;

                // =========================================
                // RECURSOS DO ATOR
                // =========================================

                wsAtores.Cell(linhaAtor, 1).Value =
                    "Recursos";

                linhaAtor++;

                if (ator.Redes != null &&
                    ator.Redes.Any())
                {
                    foreach (var rede in ator.Redes)
                    {
                        wsAtores.Cell(linhaAtor, 2).Value =
                            rede.Nome;

                        wsAtores.Cell(linhaAtor, 3).Value =
                            rede.Tipo;

                        linhaAtor++;
                    }
                }
                else
                {
                    wsAtores.Cell(linhaAtor, 2).Value =
                        "Nenhum recurso";

                    linhaAtor++;
                }

                // =========================================
                // VULNERABILIDADES
                // =========================================

                wsAtores.Cell(linhaAtor, 1).Value =
                    "Vulnerabilidades";

                linhaAtor++;

                if (ator.Avaliacoes != null &&
                    ator.Avaliacoes.Any())
                {
                    foreach (var av in ator.Avaliacoes)
                    {
                        wsAtores.Cell(linhaAtor, 2).Value =
                            $"Crimes: {av.CCrimes}";

                        wsAtores.Cell(linhaAtor, 3).Value =
                            $"Saúde: {av.Saude}";

                        wsAtores.Cell(linhaAtor, 4).Value =
                            $"Moradia: {av.Moradia}";

                        linhaAtor++;
                    }
                }
                else
                {
                    wsAtores.Cell(linhaAtor, 2).Value =
                        "Nenhuma vulnerabilidade";

                    linhaAtor++;
                }

                linhaAtor += 2;
            }

            var estiloAtores =
                wsAtores.Range($"A1:A{linhaAtor}");

            estiloAtores.Style.Font.Bold = true;

            estiloAtores.Style.Fill.BackgroundColor =
                XLColor.LightBlue;

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