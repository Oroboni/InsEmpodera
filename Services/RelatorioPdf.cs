using System.Globalization;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Empodera.Services;

// Cores do relatório: as mesmas do tema do sistema (wwwroot/css/tema.css), mais o magenta do logo.
public sealed record PaletaRelatorio(
    string Escuro, string Principal, string Medio, string Claro, string Suave, string Texto, string Destaque)
{
    public static readonly PaletaRelatorio Verde = new("#1E3F33", "#2B5748", "#618764", "#9CB080", "#EEF3EA", "#273338", "#C2379F");
    public static readonly PaletaRelatorio Roxo = new("#3A153F", "#7A3188", "#9B4DB0", "#CAA3E4", "#F3E9F7", "#2D2233", "#6BC0B7");

    // Mesma chave "Tema" do appsettings (verde = EcoRedes; sem tema = roxo, a identidade original).
    public static PaletaRelatorio DoTema(string? tema) =>
        string.Equals(tema, "verde", StringComparison.OrdinalIgnoreCase) ? Verde : Roxo;
}

// Layout dos PDFs com a identidade visual do EcoRedes: faixa no tema com logo e círculos, fonte Poppins,
// cartões de indicadores e tabelas no padrão das telas.
public sealed class RelatorioPdf
{
    private const string Cinza = "#5B6670";
    private const string Linha = "#DDE3DA";
    private static readonly object Trava = new();
    private static bool _fontesRegistradas;

    private readonly PaletaRelatorio _cor;
    private readonly byte[]? _logo;

    public RelatorioPdf(PaletaRelatorio paleta, string pastaWeb)
    {
        _cor = paleta;
        RegistrarFontes(pastaWeb);
        var logo = Path.Combine(pastaWeb, "images", "relatorios", "logo-ecoredes.png");
        _logo = File.Exists(logo) ? File.ReadAllBytes(logo) : null;
    }

    public PaletaRelatorio Cor => _cor;

    // Poppins (a fonte das telas) vem de wwwroot/fonts; sem ela, o QuestPDF usa a Lato embutida.
    private static void RegistrarFontes(string pastaWeb)
    {
        lock (Trava)
        {
            if (_fontesRegistradas) return;
            var pasta = Path.Combine(pastaWeb, "fonts", "Poppins");
            foreach (var arquivo in new[] { "Poppins-Regular.ttf", "Poppins-Medium.ttf", "Poppins-SemiBold.ttf" })
            {
                var caminho = Path.Combine(pasta, arquivo);
                if (File.Exists(caminho))
                {
                    using var fonte = File.OpenRead(caminho);
                    FontManager.RegisterFont(fonte);
                }
            }
            _fontesRegistradas = true;
        }
    }

    public byte[] Gerar(string titulo, IReadOnlyList<(string Rotulo, string Valor)> informacoes, Action<ColumnDescriptor> conteudo) =>
        Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Poppins", "Lato").FontSize(9).FontColor(_cor.Texto));

                page.Header().Column(col =>
                {
                    col.Item().ShowOnce().Element(c => FaixaCompleta(c, titulo));
                    col.Item().SkipOnce().Element(c => FaixaCompacta(c, titulo));
                });

                page.Content().PaddingHorizontal(36).PaddingTop(18).PaddingBottom(12).Column(col =>
                {
                    col.Spacing(10);
                    if (informacoes.Count > 0)
                        col.Item().ShowOnce().Element(c => Informacoes(c, informacoes));
                    conteudo(col);
                });

                page.Footer().DefaultTextStyle(x => x.FontSize(7.5f)).PaddingHorizontal(36).PaddingBottom(18).Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(_cor.Claro);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("EcoRedes").SemiBold().FontColor(_cor.Principal);
                            t.Span($"  ·  Sistema de Tratamento Comunitário ECO2  ·  gerado em {DateTime.Now:dd/MM/yyyy 'às' HH:mm}")
                                .FontColor(Cinza);
                        });
                        row.AutoItem().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontColor(Cinza));
                            t.Span("Página ");
                            t.CurrentPageNumber().SemiBold().FontColor(_cor.Principal);
                            t.Span(" de ");
                            t.TotalPages();
                        });
                    });
                });
            }))
            .WithMetadata(new DocumentMetadata { Title = $"{titulo} - EcoRedes", Author = "EcoRedes", Creator = "EcoRedes" })
            .GeneratePdf();

    // Primeira página: faixa no tema com logo, nome do sistema, título e os círculos da tela de login.
    private void FaixaCompleta(IContainer container, string titulo) =>
        container.Height(116).Layers(camadas =>
        {
            camadas.Layer().Background(_cor.Principal);
            camadas.Layer().Svg(Circulos(595, 116));
            camadas.PrimaryLayer().PaddingHorizontal(36).PaddingVertical(18).Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (_logo is not null)
                        row.ConstantItem(34).Height(34).Background(Colors.White).CornerRadius(17).Padding(4).Image(_logo).FitArea();
                    row.ConstantItem(10);
                    row.RelativeItem().AlignMiddle().Column(marca =>
                    {
                        marca.Item().Text("EcoRedes").FontSize(13).SemiBold().FontColor(Colors.White);
                        marca.Item().Text("Sistema de Tratamento Comunitário ECO2").FontSize(7.5f).FontColor(_cor.Claro);
                    });
                    row.AutoItem().AlignMiddle().Background(_cor.Escuro).CornerRadius(10).PaddingVertical(3).PaddingHorizontal(10)
                        .Text("RELATÓRIO").FontSize(7).SemiBold().LetterSpacing(0.1f).FontColor(_cor.Claro);
                });
                col.Item().PaddingTop(10).Text(titulo).FontSize(21).SemiBold().FontColor(Colors.White);
            });
        });

    // Demais páginas: faixa fina para o leitor saber de que relatório é a folha.
    private void FaixaCompacta(IContainer container, string titulo) =>
        container.Background(_cor.Principal).DefaultTextStyle(x => x.FontSize(9)).PaddingHorizontal(36).PaddingVertical(9).Row(row =>
        {
            row.RelativeItem().Text(t =>
            {
                t.Span("EcoRedes  ").SemiBold().FontColor(Colors.White);
                t.Span(titulo).FontColor(_cor.Claro);
            });
        });

    private void Informacoes(IContainer container, IReadOnlyList<(string Rotulo, string Valor)> informacoes) =>
        container.Row(row =>
        {
            row.Spacing(8);
            foreach (var (rotulo, valor) in informacoes)
                row.RelativeItem().Background(_cor.Suave).CornerRadius(6).BorderLeft(3).BorderColor(_cor.Medio)
                    .PaddingVertical(6).PaddingHorizontal(9).Column(c =>
                    {
                        c.Item().Text(rotulo.ToUpperInvariant()).FontSize(6.5f).SemiBold().LetterSpacing(0.08f).FontColor(_cor.Medio);
                        c.Item().Text(valor).FontSize(9.5f).Medium();
                    });
        });

    public void Secao(ColumnDescriptor col, string titulo, string? apoio = null) =>
        col.Item().PaddingTop(8).Row(row =>
        {
            row.ConstantItem(4).Height(16).Background(_cor.Destaque).CornerRadius(2);
            row.ConstantItem(8);
            row.RelativeItem().AlignMiddle().Text(t =>
            {
                t.Span(titulo).FontSize(12).SemiBold().FontColor(_cor.Principal);
                if (apoio is not null)
                    t.Span($"   {apoio}").FontSize(8).FontColor(Cinza);
            });
        });

    // Cartões de indicadores (número grande + rótulo), no estilo dos cartões das telas.
    public void Indicadores(ColumnDescriptor col, IReadOnlyList<(string Rotulo, string Valor)> indicadores, int porLinha = 4)
    {
        foreach (var linha in indicadores.Chunk(porLinha))
            col.Item().Row(row =>
            {
                row.Spacing(8);
                foreach (var (rotulo, valor) in linha)
                    row.RelativeItem().Border(1).BorderColor(Linha).CornerRadius(8).Background(Colors.White)
                        .Column(c =>
                        {
                            c.Item().Height(3).Background(_cor.Medio);
                            c.Item().PaddingHorizontal(10).PaddingTop(8).PaddingBottom(9).Column(dentro =>
                            {
                                dentro.Item().Text(valor).FontSize(20).SemiBold().FontColor(_cor.Principal);
                                dentro.Item().Text(rotulo).FontSize(8).FontColor(Cinza);
                            });
                        });
                for (var vazio = linha.Length; vazio < porLinha; vazio++)
                    row.RelativeItem();
            });
    }

    // corCabecalho: cor própria de uma coluna (ex.: a cor de cada data no gráfico das avaliações).
    public void Tabela(ColumnDescriptor col, string[] cabecalhos, float[] larguras, IEnumerable<string[]> linhas,
        string[]? rodape = null, IReadOnlyDictionary<int, string>? corCabecalho = null, bool primeiraColunaForte = false)
    {
        var dados = linhas.ToList();
        if (dados.Count == 0)
        {
            col.Item().Background(_cor.Suave).CornerRadius(6).Padding(12)
                .Text("Nenhum registro para os filtros escolhidos.").Italic().FontColor(Cinza);
            return;
        }

        static bool Numero(string valor) => valor.Length > 0 && valor.All(c => char.IsDigit(c) || c is ',' or '.' or '-' or '+');

        col.Item().Border(1).BorderColor(Linha).CornerRadius(6).Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                foreach (var largura in larguras)
                    c.RelativeColumn(largura);
            });
            t.Header(h =>
            {
                for (var i = 0; i < cabecalhos.Length; i++)
                {
                    var fundo = corCabecalho is not null && corCabecalho.TryGetValue(i, out var cor) ? cor : _cor.Principal;
                    h.Cell().Background(fundo).PaddingVertical(6).PaddingHorizontal(7)
                        .Text(cabecalhos[i]).FontSize(7.5f).SemiBold().FontColor(Colors.White);
                }
            });
            for (var i = 0; i < dados.Count; i++)
            {
                var fundo = i % 2 == 0 ? "#FFFFFF" : _cor.Suave;
                for (var c = 0; c < dados[i].Length; c++)
                {
                    var valor = dados[i][c];
                    var celula = t.Cell().Background(fundo).BorderBottom(0.5f).BorderColor(Linha).PaddingVertical(4.5f).PaddingHorizontal(7);
                    var texto = (Numero(valor) && c > 0 ? celula.AlignRight() : celula).Text(valor);
                    if (c == 0 && primeiraColunaForte)
                        texto.Medium();
                }
            }
            if (rodape is not null)
                for (var c = 0; c < rodape.Length; c++)
                {
                    var celula = t.Cell().Background(_cor.Claro).PaddingVertical(6).PaddingHorizontal(7);
                    (Numero(rodape[c]) && c > 0 ? celula.AlignRight() : celula).Text(rodape[c]).SemiBold().FontColor(_cor.Escuro);
                }
        });
    }

    public void Nota(ColumnDescriptor col, string texto) =>
        col.Item().PaddingTop(2).Text(texto).FontSize(7.5f).Italic().FontColor(Cinza);

    // Círculos vazados da tela de login, à direita da faixa.
    private string Circulos(double largura, double altura)
    {
        string P(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
        var circulos = new (double X, double Y, double R, double Opacidade)[]
        {
            (largura - 70, 18, 46, 0.22), (largura - 175, altura + 8, 58, 0.16), (largura - 16, altura - 10, 30, 0.2),
            (largura - 255, -6, 22, 0.12)
        };
        var corpo = string.Concat(circulos.Select(c =>
            $"<circle cx=\"{P(c.X)}\" cy=\"{P(c.Y)}\" r=\"{P(c.R)}\" fill=\"none\" stroke=\"{_cor.Claro}\" stroke-opacity=\"{P(c.Opacidade)}\" stroke-width=\"7\"/>"));
        // O recorte mantém os círculos dentro da faixa (sem invadir o conteúdo).
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {P(largura)} {P(altura)}\">"
            + $"<defs><clipPath id=\"faixa\"><rect width=\"{P(largura)}\" height=\"{P(altura)}\"/></clipPath></defs>"
            + $"<g clip-path=\"url(#faixa)\">{corpo}</g></svg>";
    }
}
