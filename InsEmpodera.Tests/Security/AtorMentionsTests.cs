using Empodera.Services;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class AtorMentionsTests
{
    [Fact(DisplayName = "Menções — nome de ator vira link sem interpretar HTML do relato")]
    public void Render_EncodesDescriptionAndActorName()
    {
        var atorId = 4821;
        var html = Assert.IsType<HtmlString>(AtorMentions.Render(
            $"Olá <script>alert(1)</script> @[Nome anterior](ator:{atorId})",
            new Dictionary<int, string> { [atorId] = "Maria <Silva>" },
            id => $"/Atores/Resumo/{id}")).Value;

        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.Contains($"href=\"/Atores/Resumo/{atorId}\"", html);
        Assert.Contains("@Maria &lt;Silva&gt;", html);
        Assert.DoesNotContain("<script>", html);
    }
}
