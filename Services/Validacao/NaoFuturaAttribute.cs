using System.ComponentModel.DataAnnotations;

namespace Empodera.Services.Validacao;

// Data que já aconteceu: hoje ou antes (diários de campo, contatos, nascimentos...).
// No formulário, o campo também recebe max = hoje (ver as views).
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NaoFuturaAttribute : ValidationAttribute
{
    public NaoFuturaAttribute()
    {
        ErrorMessageResourceType = typeof(MensagensValidacao);
        ErrorMessageResourceName = nameof(MensagensValidacao.DataFutura);
    }

    public override bool IsValid(object? value) => value switch
    {
        DateTime data => data.Date <= DateTime.Today,
        DateOnly data => data <= DateOnly.FromDateTime(DateTime.Today),
        _ => true
    };
}
