using System.Globalization;
using System.Resources;

namespace Empodera.Services.Validacao;

// Mensagens de validação no idioma da tela (Resources/MensagensValidacao*.resx).
// As propriedades são lidas pelos atributos de validação (ErrorMessageResourceType) a cada uso,
// então acompanham o idioma escolhido pelo usuário.
public static class MensagensValidacao
{
    private static readonly ResourceManager Recursos =
        new("InsEmpodera.Resources.MensagensValidacao", typeof(MensagensValidacao).Assembly);

    private static string Texto(string chave) =>
        Recursos.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;

    public static string Obrigatorio => Texto(nameof(Obrigatorio));
    public static string Intervalo => Texto(nameof(Intervalo));
    public static string TamanhoMaximo => Texto(nameof(TamanhoMaximo));
    public static string TamanhoEntre => Texto(nameof(TamanhoEntre));
    public static string TamanhoMinimo => Texto(nameof(TamanhoMinimo));
    public static string Email => Texto(nameof(Email));
    public static string Comparar => Texto(nameof(Comparar));
    public static string ValorInvalido => Texto(nameof(ValorInvalido));
    public static string ValorInvalidoSemCampo => Texto(nameof(ValorInvalidoSemCampo));
    public static string ValorObrigatorio => Texto(nameof(ValorObrigatorio));
    public static string DeveSerNumero => Texto(nameof(DeveSerNumero));
    public static string DataFutura => Texto(nameof(DataFutura));
    public static string CorpoInvalido => Texto(nameof(CorpoInvalido));
}
