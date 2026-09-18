using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace Empodera.Services;

public static class AtorMentions
{
    private static readonly Regex Marcador = new(
        @"@\[(?<nome>[^\]\r\n]{1,100})\]\(ator:(?<id>\d{1,10})\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    public static IHtmlContent Render(
        string? descricao,
        IReadOnlyDictionary<int, string> atores,
        Func<int, string> urlDoAtor)
    {
        var texto = descricao ?? string.Empty;
        var html = new StringBuilder();
        var inicio = 0;

        foreach (Match match in Marcador.Matches(texto))
        {
            html.Append(HtmlEncoder.Default.Encode(texto[inicio..match.Index]));
            if (int.TryParse(match.Groups["id"].Value, out var id) && atores.TryGetValue(id, out var nome))
            {
                html.Append("<a class=\"actor-mention\" href=\"")
                    .Append(HtmlEncoder.Default.Encode(urlDoAtor(id)))
                    .Append("\" title=\"Ver dados básicos do ator\">@")
                    .Append(HtmlEncoder.Default.Encode(nome))
                    .Append("</a>");
            }
            else
            {
                html.Append(HtmlEncoder.Default.Encode(match.Value));
            }
            inicio = match.Index + match.Length;
        }

        html.Append(HtmlEncoder.Default.Encode(texto[inicio..]));
        return new HtmlString(html.ToString().Replace("\r\n", "<br>").Replace("\n", "<br>"));
    }
}
