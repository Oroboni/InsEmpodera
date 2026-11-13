using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Empodera.Services.Validacao;

// Troca as mensagens padrão em inglês ("The FkIdComunidade field is required.") por textos no idioma da tela,
// com nomes de campo legíveis ("Comunidade"). Vale para todos os formulários, inclusive a validação no navegador.
public sealed class ValidacaoAmigavelProvider : IValidationMetadataProvider, IDisplayMetadataProvider
{
    // Nome técnico da propriedade -> nome mostrado (pt-BR, en, es).
    private static readonly Dictionary<string, (string Pt, string En, string Es)> Nomes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["FkIdComunidade"] = ("Comunidade", "Community", "Comunidad"),
            ["ComunidadeId"] = ("Comunidade", "Community", "Comunidad"),
            ["FK_Id_Comunidade"] = ("Comunidade", "Community", "Comunidad"),
            ["FK_id_Atores"] = ("Ator", "Actor", "Actor"),
            ["Fk_Id_Ator"] = ("Ator", "Actor", "Actor"),
            ["AtorId"] = ("Ator", "Actor", "Actor"),
            ["FkIdPerfil"] = ("Perfil", "Profile", "Perfil"),
            ["FkIdEixo"] = ("Eixo", "Axis", "Eje"),
            ["EixosSelecionados"] = ("Eixos", "Axes", "Ejes"),
            ["Nome"] = ("Nome", "Name", "Nombre"),
            ["Descricao"] = ("Descrição", "Description", "Descripción"),
            ["Descricao_Acessibilidade"] = ("Acessibilidade", "Accessibility", "Accesibilidad"),
            ["Data"] = ("Data", "Date", "Fecha"),
            ["DtContato"] = ("Data do contato", "Contact date", "Fecha del contacto"),
            ["HoraContato"] = ("Hora do contato", "Contact time", "Hora del contacto"),
            ["DtNascimento"] = ("Data de nascimento", "Date of birth", "Fecha de nacimiento"),
            ["Idade"] = ("Idade", "Age", "Edad"),
            ["Genero"] = ("Gênero", "Gender", "Género"),
            ["Email"] = ("E-mail", "E-mail", "Correo electrónico"),
            ["Senha"] = ("Senha", "Password", "Contraseña"),
            ["Password"] = ("Senha", "Password", "Contraseña"),
            ["ConfirmPassword"] = ("Confirmação da senha", "Password confirmation", "Confirmación de la contraseña"),
            ["Telefone"] = ("Telefone", "Phone", "Teléfono"),
            ["PapelSocial1"] = ("Papel social", "Social role", "Rol social"),
            ["PapelSocial2"] = ("Papel social adicional", "Additional social role", "Rol social adicional"),
            ["Interesse"] = ("Interesse", "Interest", "Interés"),
            ["Local"] = ("Localização", "Location", "Ubicación"),
            ["LocalMapa"] = ("Localização no mapa", "Map location", "Ubicación en el mapa"),
            ["LocalSecundario"] = ("Endereço adicional", "Additional address", "Dirección adicional"),
            ["Localizacao"] = ("Localização", "Location", "Ubicación"),
            ["LContato"] = ("Local do contato", "Contact place", "Lugar del contacto"),
            ["Status"] = ("Status", "Status", "Estado"),
            ["Tipo"] = ("Tipo", "Type", "Tipo"),
            ["Quantidade"] = ("Quantidade", "Amount", "Cantidad"),
            ["Servicos"] = ("Serviços", "Services", "Servicios"),
            ["Complemento"] = ("Complemento", "Complement", "Complemento"),
            ["Endereco"] = ("Endereço", "Address", "Dirección"),
            ["Ocupacao"] = ("Ocupação", "Occupation", "Ocupación"),
            ["TipoRelacao"] = ("Tipo de relação", "Relationship", "Tipo de relación"),
        };

    public static string NomeDoCampo(string propriedade)
    {
        if (!Nomes.TryGetValue(propriedade, out var nome))
            return propriedade;
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "en" => nome.En,
            "es" => nome.Es,
            _ => nome.Pt
        };
    }

    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var propriedade = context.Key.Name;
        if (context.Key.MetadataKind != ModelMetadataKind.Property || propriedade is null
            || context.Attributes.OfType<DisplayAttribute>().Any() || !Nomes.ContainsKey(propriedade))
            return;
        // Func: lido a cada uso, então acompanha o idioma da requisição.
        context.DisplayMetadata.DisplayName = () => NomeDoCampo(propriedade);
    }

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            // Mensagem escrita pela equipe no próprio atributo continua valendo.
            if (!string.IsNullOrEmpty(atributo.ErrorMessage) || !string.IsNullOrEmpty(atributo.ErrorMessageResourceName))
                continue;
            var chave = atributo switch
            {
                RequiredAttribute => nameof(MensagensValidacao.Obrigatorio),
                RangeAttribute => nameof(MensagensValidacao.Intervalo),
                StringLengthAttribute s when s.MinimumLength > 0 => nameof(MensagensValidacao.TamanhoEntre),
                StringLengthAttribute or MaxLengthAttribute => nameof(MensagensValidacao.TamanhoMaximo),
                MinLengthAttribute => nameof(MensagensValidacao.TamanhoMinimo),
                EmailAddressAttribute => nameof(MensagensValidacao.Email),
                System.ComponentModel.DataAnnotations.CompareAttribute => nameof(MensagensValidacao.Comparar),
                _ => null
            };
            if (chave is null)
                continue;
            atributo.ErrorMessageResourceType = typeof(MensagensValidacao);
            atributo.ErrorMessageResourceName = chave;
        }
    }

    // Mensagens de conversão (ex.: "abc" num campo numérico) no idioma da tela.
    public static void Configurar(MvcOptions options)
    {
        options.ModelMetadataDetailsProviders.Add(new ValidacaoAmigavelProvider());
        var m = options.ModelBindingMessageProvider;
        m.SetAttemptedValueIsInvalidAccessor((valor, campo) => string.Format(MensagensValidacao.ValorInvalido, valor, NomeDoCampo(campo)));
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => string.Format(MensagensValidacao.ValorInvalidoSemCampo, valor));
        m.SetUnknownValueIsInvalidAccessor(campo => string.Format(MensagensValidacao.Obrigatorio, NomeDoCampo(campo)));
        m.SetNonPropertyUnknownValueIsInvalidAccessor(() => MensagensValidacao.CorpoInvalido);
        m.SetValueIsInvalidAccessor(valor => string.Format(MensagensValidacao.ValorInvalidoSemCampo, valor));
        m.SetValueMustBeANumberAccessor(campo => string.Format(MensagensValidacao.DeveSerNumero, NomeDoCampo(campo)));
        m.SetNonPropertyValueMustBeANumberAccessor(() => string.Format(MensagensValidacao.DeveSerNumero, "?"));
        m.SetMissingBindRequiredValueAccessor(campo => string.Format(MensagensValidacao.Obrigatorio, NomeDoCampo(campo)));
        m.SetMissingKeyOrValueAccessor(() => MensagensValidacao.CorpoInvalido);
        m.SetMissingRequestBodyRequiredValueAccessor(() => MensagensValidacao.CorpoInvalido);
        m.SetValueMustNotBeNullAccessor(campo => string.Format(MensagensValidacao.Obrigatorio, NomeDoCampo(campo)));
    }
}
