namespace Empodera.Models;

public sealed class FirstContactReportViewModel
{
    public Guid? ComunidadeId { get; init; }
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
    public Guid? ComunidadeId { get; init; }
    public Guid? AtorId { get; init; }
    public DateTime? DataDe { get; init; }
    public DateTime? DataAte { get; init; }
    public IReadOnlyList<ActionReportRow> Rows { get; init; } = [];
    public int TotalActions => Rows.Sum(row => row.Actions);
    public int TotalQuantity => Rows.Sum(row => row.Quantity);
}

public sealed record ActionReportRow(string Axis, int Actions, int Quantity, int Actors);
