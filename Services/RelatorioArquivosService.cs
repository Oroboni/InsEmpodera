using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Empodera.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Empodera.Services;

// Arquivos (Excel e PDF) dos relatórios. Os números vêm prontos do RelatoriosService.
public class RelatorioArquivosService
{
    private readonly RelatorioPdf _pdf;
    private readonly string _corPlanilha;

    static RelatorioArquivosService()
    {
        // Licença gratuita do QuestPDF (projeto acadêmico/sem fins lucrativos).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public RelatorioArquivosService(IWebHostEnvironment ambiente, IConfiguration configuracao)
    {
        _pdf = new RelatorioPdf(PaletaRelatorio.DoTema(configuracao["Tema"]), ambiente.WebRootPath);
        // Cabeçalho das planilhas na cor do tema.
        _corPlanilha = _pdf.Cor.Principal;
    }

    private static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "";

    // ── Excel ─────────────────────────────────────────────────────────────

    public byte[] AcoesExcel(ActionsReportViewModel m)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Ações");
        var linha = Cabecalho(ws, "Relatório de Ações", Filtros(m));
        linha = Tabela(ws, linha, ["Eixo", "Nome da ação", "Data da ação", "Atendidos", "Atendimentos", "Atores envolvidos"],
            m.Details.Select(d => new object[] { d.Axes, d.ActionName, d.Date.Date, d.Attended, d.Services, d.ActorsInvolved }));
        ws.Cell(linha, 1).Value = "Total (cada pessoa conta uma vez)";
        ws.Cell(linha, 4).Value = m.TotalAttended;
        ws.Cell(linha, 5).Value = m.TotalQuantity;
        ws.Cell(linha, 6).Value = m.TotalActors;
        ws.Row(linha).Style.Font.Bold = true;

        var resumo = wb.Worksheets.Add("Por eixo");
        var l2 = Cabecalho(resumo, "Ações por eixo", Filtros(m));
        Tabela(resumo, l2, ["Eixo", "Ações", "Atendidos", "Atendimentos", "Atores envolvidos"],
            m.Rows.Select(r => new object[] { r.Axis, r.Actions, r.Attended, r.Quantity, r.Actors }));
        return Salvar(wb);
    }

    public byte[] RscExcel(RscReportViewModel m)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("RSC");
        var linha = Cabecalho(ws, "Rede Subjetiva Comunitária (RSC)", [$"Comunidade: {m.ComunidadeNome}", $"Nós: {m.Nos}"]);
        static object X(bool marcado) => marcado ? 1 : "";
        linha = Tabela(ws, linha,
            ["Ator 1", "Ator 2", "Relação", "Gênero", "Papel Social 1", "Papel Social 2", "EQ", "RO", "RLO", "PA", "CO", "RRC"],
            m.Rows.Select(r => new object[]
            {
                r.Ator1, r.Ator2, r.Relacao, r.Genero, r.PapelSocial1 ?? "", r.PapelSocial2 ?? "",
                X(r.Eq), X(r.Ro), X(r.Rlo), X(r.Pa), X(r.Co), X(r.Rrc)
            }));
        object[] totais = ["Total", "", "", "", "", "", m.TotalEq, m.TotalRo, m.TotalRlo, m.TotalPa, m.TotalCo, m.TotalRrc];
        for (var c = 0; c < totais.Length; c++)
            ws.Cell(linha, c + 1).Value = XLCellValue.FromObject(totais[c]);
        ws.Row(linha).Style.Font.Bold = true;

        linha += 2;
        ws.Cell(linha++, 1).Value = "Ator 1: de quem sai a rede subjetiva. Ator 2: quem faz parte dela (demais colunas são do Ator 2).";
        foreach (var legenda in new[]
                 {
                     "EQ = Equipe", "RO = Rede Operativa (inclui Operador par)", "RLO = Rede de Líderes de Opinião",
                     "PA = Parceiro", "CO = Comunidade (mora na comunidade)", "RRC = Rede de Recursos Comunitários (recurso relacional)"
                 })
            ws.Cell(linha++, 1).Value = legenda;
        return Salvar(wb);
    }

    public byte[] AvaliacoesExcel(PersonalAssessmentReportViewModel m)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Avaliações");
        var linha = Cabecalho(ws, "Avaliações Pessoais", [$"Ator: {m.AtorNome}"]);
        var cabecalhos = new List<string> { "Dimensão" };
        cabecalhos.AddRange(m.Selecionadas.Select(a => Data(a.DtCriacao)));
        var valores = m.Selecionadas.Select(PersonalAssessmentReportViewModel.Valores).ToList();
        Tabela(ws, linha, cabecalhos.ToArray(),
            PersonalAssessmentReportViewModel.Dimensoes.Select((dimensao, i) =>
                new object[] { dimensao }.Concat(valores.Select(v => (object)v[i])).ToArray()));
        return Salvar(wb);
    }

    private static IEnumerable<string> Filtros(ActionsReportViewModel m)
    {
        yield return $"Comunidade: {m.ComunidadeNome}";
        yield return $"Eixo: {m.EixoNome ?? "Todos"}";
        if (m.DataDe.HasValue || m.DataAte.HasValue)
            yield return $"Período: {Data(m.DataDe)} a {Data(m.DataAte)}";
    }

    private static int Cabecalho(IXLWorksheet ws, string titulo, IEnumerable<string> linhas)
    {
        ws.Cell(1, 1).Value = titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        var linha = 2;
        foreach (var texto in linhas)
            ws.Cell(linha++, 1).Value = texto;
        ws.Cell(linha++, 1).Value = $"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}";
        return linha + 1;
    }

    private int Tabela(IXLWorksheet ws, int linha, string[] cabecalhos, IEnumerable<object[]> linhas)
    {
        for (var c = 0; c < cabecalhos.Length; c++)
            ws.Cell(linha, c + 1).Value = cabecalhos[c];
        var cab = ws.Range(linha, 1, linha, cabecalhos.Length);
        cab.Style.Font.Bold = true;
        cab.Style.Font.FontColor = XLColor.White;
        cab.Style.Fill.BackgroundColor = XLColor.FromHtml(_corPlanilha);
        linha++;
        foreach (var valores in linhas)
        {
            for (var c = 0; c < valores.Length; c++)
            {
                var celula = ws.Cell(linha, c + 1);
                celula.Value = XLCellValue.FromObject(valores[c]);
                if (valores[c] is DateTime)
                    celula.Style.DateFormat.Format = "dd/MM/yyyy";
            }
            linha++;
        }
        ws.Columns().AdjustToContents();
        return linha;
    }

    private static byte[] Salvar(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── PDF ───────────────────────────────────────────────────────────────

    private static List<(string, string)> Informacoes(ActionsReportViewModel m)
    {
        var lista = new List<(string, string)> { ("Comunidade", m.ComunidadeNome ?? "-"), ("Eixo", m.EixoNome ?? "Todos") };
        if (m.DataDe.HasValue || m.DataAte.HasValue)
            lista.Add(("Período", $"{(m.DataDe.HasValue ? Data(m.DataDe) : "início")} a {(m.DataAte.HasValue ? Data(m.DataAte) : "hoje")}"));
        return lista;
    }

    public byte[] AcoesPdf(ActionsReportViewModel m) => _pdf.Gerar("Relatório de Ações", Informacoes(m), col =>
    {
        _pdf.Indicadores(col,
        [
            ("Ações", $"{m.TotalActions}"),
            ("Atendimentos", $"{m.TotalQuantity}"),
            ("Atendidos", $"{m.TotalAttended}"),
            ("Atores envolvidos", $"{m.TotalActors}")
        ]);
        _pdf.Secao(col, "Resumo por eixo");
        _pdf.Tabela(col, ["Eixo", "Ações", "Atendidos", "Atendimentos", "Atores envolvidos"], [4, 1.4f, 1.6f, 1.8f, 2],
            m.Rows.Select(r => new[] { r.Axis, $"{r.Actions}", $"{r.Attended}", $"{r.Quantity}", $"{r.Actors}" }),
            primeiraColunaForte: true);
        _pdf.Secao(col, "Detalhamento das ações", $"{m.Details.Count} registro(s)");
        _pdf.Tabela(col, ["Eixo", "Nome da ação", "Data", "Atendidos", "Atendimentos", "Atores envolvidos"], [2.6f, 3.3f, 1.9f, 1.6f, 2.1f, 2.1f],
            m.Details.Select(d => new[] { d.Axes, d.ActionName, Data(d.Date), $"{d.Attended}", $"{d.Services}", $"{d.ActorsInvolved}" }),
            rodape: ["Total", "", "", $"{m.TotalAttended}", $"{m.TotalQuantity}", $"{m.TotalActors}"]);
        _pdf.Nota(col, "Atendidos: atores cadastrados na ação que não são da equipe. Atendimentos: quantidade informada na ação. "
            + "Atores envolvidos: todos os atores cadastrados na ação. Nos totais, cada pessoa conta uma vez; "
            + "uma ação com mais de um eixo aparece em cada um no resumo.");
    });

    public byte[] AvaliacoesPdf(PersonalAssessmentReportViewModel m)
    {
        var cores = PersonalAssessmentReportViewModel.Cores;
        var datas = m.Selecionadas.Select(a => Data(a.DtCriacao)).ToList();
        var informacoes = new List<(string, string)> { ("Ator", m.AtorNome ?? "-"), ("Avaliações comparadas", $"{datas.Count}") };
        if (datas.Count > 1)
            informacoes.Add(("Período", $"{datas[0]} a {datas[^1]}"));

        return _pdf.Gerar("Avaliações Pessoais", informacoes, col =>
        {
            _pdf.Secao(col, "Comparação das avaliações", "escala de 1 a 5");
            col.Item().Border(1).BorderColor("#DDE3DA").CornerRadius(8).Padding(10).Column(grafico =>
            {
                grafico.Item().AlignCenter().Width(330).Height(240).Svg(RadarSvg(m.Selecionadas));
                // Legenda: um quadrado com a cor de cada data (a fonte embutida não tem o caractere "■").
                grafico.Item().PaddingTop(6).AlignCenter().Row(legenda =>
                {
                    legenda.Spacing(16);
                    for (var i = 0; i < datas.Count; i++)
                    {
                        var cor = cores[i % cores.Length];
                        var data = datas[i];
                        legenda.AutoItem().Row(item =>
                        {
                            item.Spacing(5);
                            item.AutoItem().AlignMiddle().Width(10).Height(10).CornerRadius(5).Background(cor);
                            item.AutoItem().Text(data).Medium();
                        });
                    }
                });
            });

            var valores = m.Selecionadas.Select(PersonalAssessmentReportViewModel.Valores).ToList();
            var comVariacao = valores.Count > 1;
            var cabecalhos = new List<string> { "Dimensão" };
            cabecalhos.AddRange(datas);
            if (comVariacao) cabecalhos.Add("Variação");
            var larguras = new List<float> { 4 };
            larguras.AddRange(datas.Select(_ => 1.6f));
            if (comVariacao) larguras.Add(1.6f);

            static string Variacao(int diferenca) => diferenca > 0 ? $"+{diferenca}" : $"{diferenca}";
            _pdf.Secao(col, "Valores por dimensão");
            _pdf.Tabela(col, cabecalhos.ToArray(), larguras.ToArray(),
                PersonalAssessmentReportViewModel.Dimensoes.Select((dimensao, i) =>
                {
                    var linha = new List<string> { dimensao };
                    linha.AddRange(valores.Select(v => $"{v[i]}"));
                    if (comVariacao) linha.Add(Variacao(valores[^1][i] - valores[0][i]));
                    return linha.ToArray();
                }),
                corCabecalho: datas.Select((_, i) => (i + 1, cores[i % cores.Length])).ToDictionary(x => x.Item1, x => x.Item2),
                primeiraColunaForte: true);
            if (comVariacao)
                _pdf.Nota(col, "Variação: diferença entre a avaliação mais recente e a mais antiga escolhidas.");
        });
    }

    public byte[] ResumoComunidadePdf(CommunitySummaryViewModel m) =>
        _pdf.Gerar("Relatório Geral da Comunidade", [("Comunidade", m.ComunidadeNome ?? "-"), ("Data de referência", Data(DateTime.Today))], col =>
        {
            _pdf.Secao(col, "Rede da comunidade");
            _pdf.Indicadores(col,
            [
                ("Atores (nós)", $"{m.Atores}"), ("Recursos", $"{m.Recursos}"), ("Vulnerabilidades", $"{m.Vulnerabilidades}")
            ], porLinha: 3);
            _pdf.Secao(col, "Atuação");
            _pdf.Indicadores(col,
            [
                ("Atividades realizadas", $"{m.Atividades}"), ("Atendidos", $"{m.Atendidos}"),
                ("Atendimentos", $"{m.Atendimentos}"), ("Atores envolvidos", $"{m.AtoresEnvolvidos}")
            ]);
            _pdf.Secao(col, "Resumo");
            _pdf.Tabela(col, ["Indicador", "Quantidade"], [5, 2],
            [
                ["Atores (nós)", $"{m.Atores}"],
                ["Recursos", $"{m.Recursos}"],
                ["Vulnerabilidades", $"{m.Vulnerabilidades}"],
                ["Atividades realizadas", $"{m.Atividades}"],
                ["Atendidos", $"{m.Atendidos}"],
                ["Atendimentos", $"{m.Atendimentos}"],
                ["Atores envolvidos", $"{m.AtoresEnvolvidos}"]
            ], primeiraColunaForte: true);
            _pdf.Nota(col, "Atendidos: atores das ações que não são da equipe. Atendimentos: soma das quantidades das ações. "
                + "Atores envolvidos: todos os atores cadastrados nas ações.");
        });

    // Gráfico radar (escala 1 a 5) das avaliações escolhidas, sobrepostas e com uma cor por data.
    public static string RadarSvg(IReadOnlyList<AvaliacaoPessoal> avaliacoes)
    {
        const double largura = 520, altura = 420, cx = 260, cy = 212, raio = 165;
        var dimensoes = PersonalAssessmentReportViewModel.Dimensoes;
        var n = dimensoes.Length;
        string P(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
        (double X, double Y) Ponto(int i, double valor)
        {
            var angulo = -Math.PI / 2 + 2 * Math.PI * i / n;
            return (cx + raio * valor / 5 * Math.Cos(angulo), cy + raio * valor / 5 * Math.Sin(angulo));
        }

        var svg = new StringBuilder($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {P(largura)} {P(altura)}\">");
        for (var nivel = 1; nivel <= 5; nivel++)
        {
            var pontos = string.Join(" ", Enumerable.Range(0, n).Select(i => Ponto(i, nivel)).Select(p => $"{P(p.X)},{P(p.Y)}"));
            svg.Append($"<polygon points=\"{pontos}\" fill=\"none\" stroke=\"#DDE3DA\" stroke-width=\"1\"/>");
            svg.Append($"<text x=\"{P(cx + 3)}\" y=\"{P(cy - raio * nivel / 5 - 2)}\" font-family=\"Poppins\" font-size=\"9\" fill=\"#5B6670\">{nivel}</text>");
        }
        for (var i = 0; i < n; i++)
        {
            var (x, y) = Ponto(i, 5);
            svg.Append($"<line x1=\"{P(cx)}\" y1=\"{P(cy)}\" x2=\"{P(x)}\" y2=\"{P(y)}\" stroke=\"#D9E0D6\" stroke-width=\"1\"/>");
            var (lx, ly) = Ponto(i, 5.9);
            var ancora = Math.Abs(lx - cx) < 8 ? "middle" : lx > cx ? "start" : "end";
            svg.Append($"<text x=\"{P(lx)}\" y=\"{P(ly + 4)}\" font-family=\"Poppins\" font-size=\"14\" font-weight=\"500\" fill=\"#273338\" text-anchor=\"{ancora}\">{dimensoes[i]}</text>");
        }
        for (var a = 0; a < avaliacoes.Count; a++)
        {
            var cor = PersonalAssessmentReportViewModel.Cores[a % PersonalAssessmentReportViewModel.Cores.Length];
            var valores = PersonalAssessmentReportViewModel.Valores(avaliacoes[a]);
            var pontos = string.Join(" ", valores.Select((v, i) => Ponto(i, v)).Select(p => $"{P(p.X)},{P(p.Y)}"));
            svg.Append($"<polygon points=\"{pontos}\" fill=\"{cor}\" fill-opacity=\"0.14\" stroke=\"{cor}\" stroke-width=\"2.2\" stroke-linejoin=\"round\"/>");
            foreach (var (x, y) in valores.Select((v, i) => Ponto(i, v)))
                svg.Append($"<circle cx=\"{P(x)}\" cy=\"{P(y)}\" r=\"3.4\" fill=\"#FFFFFF\" stroke=\"{cor}\" stroke-width=\"2\"/>");
        }
        return svg.Append("</svg>").ToString();
    }
}
