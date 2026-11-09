namespace Empodera.Models;

public sealed class FirstContactReportViewModel
{
    public int? ComunidadeId { get; init; }
    public string Campo { get; init; } = "status";
    public int? Genero { get; init; }
    public string? EstruturaFamiliar { get; init; }
    public string? Condicao { get; init; }
    public string? Resultado { get; init; }
    public string? Resposta { get; init; }
    public string? Horario { get; init; }
    public string? ComoEstabeleceu { get; init; }
    public string? Escolaridade { get; init; }
    public DateTime? DataDe { get; init; }
    public DateTime? DataAte { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<ReportSeriesItem> Series { get; init; } = [];
}

public sealed record ReportSeriesItem(string Label, int Value, decimal Percentage);

public sealed class ActionsReportViewModel
{
    public int? ComunidadeId { get; init; }
    public string? ComunidadeNome { get; init; }
    public int? AtorId { get; init; }
    public int? EixoId { get; init; }
    public string? EixoNome { get; init; }
    public DateTime? DataDe { get; init; }
    public DateTime? DataAte { get; init; }
    public IReadOnlyList<ActionReportRow> Rows { get; init; } = [];
    public IReadOnlyList<ActionDetailRow> Details { get; init; } = [];
    public int TotalActions => Details.Count;
    public int TotalQuantity => Details.Sum(row => row.Services);
    // Pessoas distintas em todas as ações (a mesma pessoa em duas ações conta uma vez).
    public int TotalAttended { get; init; }
    public int TotalActors { get; init; }
}

// Atendidos: atores cadastrados na ação que não são da equipe. Atendimentos: quantidade informada na ação.
// Atores envolvidos: todos os atores cadastrados na ação (inclusive a equipe).
public sealed record ActionReportRow(string Axis, int Actions, int Quantity, int Actors, int Attended);

public sealed record ActionDetailRow(string Axes, string ActionName, DateTime Date, int Attended, int Services, int ActorsInvolved);

// Rede Subjetiva Comunitária: uma linha por vínculo da rede subjetiva de cada ator da comunidade.
public sealed record RscRow(
    string Ator1, string Ator2, string Relacao, string Genero, string? PapelSocial1, string? PapelSocial2,
    bool Eq, bool Ro, bool Rlo, bool Pa, bool Co, bool Rrc);

public sealed class RscReportViewModel
{
    public int? ComunidadeId { get; init; }
    public string? ComunidadeNome { get; init; }
    public IReadOnlyList<RscRow> Rows { get; init; } = [];
    public int Nos { get; init; }
    public int TotalEq => Rows.Count(r => r.Eq);
    public int TotalRo => Rows.Count(r => r.Ro);
    public int TotalRlo => Rows.Count(r => r.Rlo);
    public int TotalPa => Rows.Count(r => r.Pa);
    public int TotalCo => Rows.Count(r => r.Co);
    public int TotalRrc => Rows.Count(r => r.Rrc);
}

public sealed class PrimaryNetworkReportViewModel
{
    public int? ComunidadeId { get; init; }
    public int? AtorId { get; init; }
    public string? ComunidadeNome { get; init; }
    public int Amplitude { get; init; }
    public int AtoresNaComunidade { get; init; }
    // Amplitude / atores da comunidade.
    public decimal? Densidade { get; init; }
    // Grau (vínculos que saem + que chegam na rede primária) / (atores da comunidade - 1).
    public decimal? Centralidade { get; init; }
    public int Grau { get; init; }
    public IReadOnlyList<ReportSeriesItem> Relacoes { get; init; } = [];
    public IReadOnlyList<NetworkAxisShare> Eixos { get; init; } = [];
}

public sealed record NetworkAxisShare(string Axis, decimal Resources, decimal Vulnerabilities);

public sealed class PersonalAssessmentReportViewModel
{
    public int? ComunidadeId { get; init; }
    public int? AtorId { get; init; }
    public string? AtorNome { get; init; }
    public IReadOnlyList<AvaliacaoPessoal> Avaliacoes { get; init; } = [];
    public IReadOnlyList<AvaliacaoPessoal> Selecionadas { get; init; } = [];

    public static readonly string[] Dimensoes =
    [
        "Condições de crimes", "Substâncias", "Moradia", "Prevenção",
        "Assistência Básica", "Educação", "Saúde", "Ocupação", "Lazer"
    ];

    public static int[] Valores(AvaliacaoPessoal a) =>
        [a.CCrimes, a.Substancias, a.Moradia, a.Prevencao, a.AssBasica, a.Educacao, a.Saude, a.Ocupacao, a.Lazer];

    // Cores das avaliações sobrepostas (mesma ordem no gráfico da tela e no PDF).
    // Verde do EcoRedes, magenta do logo e tons de apoio da identidade visual.
    public static readonly string[] Cores = ["#2B5748", "#C2379F", "#E8A33D", "#026AA2", "#9CB080", "#805AD5", "#C05621", "#273338"];
}

public sealed class CommunitySummaryViewModel
{
    public int? ComunidadeId { get; init; }
    public string? ComunidadeNome { get; init; }
    public int Atores { get; init; }
    public int Recursos { get; init; }
    public int Vulnerabilidades { get; init; }
    public int Atividades { get; init; }
    public int Atendidos { get; init; }
    public int Atendimentos { get; init; }
    public int AtoresEnvolvidos { get; init; }
}

// Campo de filtro dos relatórios no padrão dos formulários de Atores (cartão com ícone e lista).
public sealed record RelatorioCampoFiltro(
    string Nome,
    string Rotulo,
    string Icone,
    IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Opcoes,
    string? OpcaoVazia = null,
    bool EnviarAoMudar = false,
    bool Inteiro = false,
    bool Desabilitado = false);
