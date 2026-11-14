using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Empodera.TagHelpers;

// Marca todo <script> das views com o nonce desta requisição: a política de segurança (CSP) só executa
// scripts com esse nonce, então um script injetado por um invasor não roda (sem 'unsafe-inline').
[HtmlTargetElement("script")]
public sealed class NonceScriptTagHelper : TagHelper
{
    public const string ChaveNonce = "Empodera.CspNonce";

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    // Roda depois dos tag helpers do ASP.NET (asp-append-version etc.).
    public override int Order => 1000;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext.HttpContext.Items[ChaveNonce] is string nonce)
            output.Attributes.SetAttribute("nonce", nonce);
    }
}
