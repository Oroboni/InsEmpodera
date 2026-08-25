using System.Globalization;

namespace Empodera.Models;

public static class PersonalAssessmentScore
{
    public static int ParseImported(string? value, string metricName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException($"A pontuação de {metricName} é obrigatória.");

        var normalized = value.Trim();
        var parsed = double.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)
            ? invariant
            : double.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out var current)
                ? current
                : double.NaN;

        if (double.IsNaN(parsed) || parsed != Math.Truncate(parsed) || parsed is < 1 or > 5)
            throw new InvalidDataException($"A pontuação de {metricName} deve ser um inteiro entre 1 e 5.");

        return (int)parsed;
    }
}