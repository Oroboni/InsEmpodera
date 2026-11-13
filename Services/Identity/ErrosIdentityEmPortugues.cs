using Microsoft.AspNetCore.Identity;

namespace Empodera.Services.Identity;

// Mensagens do Identity no idioma da pessoa (o padrão vinha em inglês na tela de usuários).
public sealed class ErrosIdentityEmPortugues : IdentityErrorDescriber
{
    private static string T(string pt, string en, string es) =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "en" => en,
            "es" => es,
            _ => pt
        };

    private static IdentityError E(string code, string pt, string en, string es) =>
        new() { Code = code, Description = T(pt, en, es) };

    public override IdentityError PasswordTooShort(int length) => E(nameof(PasswordTooShort),
        $"A senha deve ter pelo menos {length} caracteres.",
        $"The password must have at least {length} characters.",
        $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => E(nameof(PasswordRequiresUniqueChars),
        $"A senha deve ter pelo menos {uniqueChars} caracteres diferentes.",
        $"The password must have at least {uniqueChars} different characters.",
        $"La contraseña debe tener al menos {uniqueChars} caracteres distintos.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => E(nameof(PasswordRequiresNonAlphanumeric),
        "A senha deve ter pelo menos um símbolo (ex.: @, #, !).",
        "The password must have at least one symbol (e.g. @, #, !).",
        "La contraseña debe tener al menos un símbolo (ej.: @, #, !).");
    public override IdentityError PasswordRequiresDigit() => E(nameof(PasswordRequiresDigit),
        "A senha deve ter pelo menos um número.",
        "The password must have at least one number.",
        "La contraseña debe tener al menos un número.");
    public override IdentityError PasswordRequiresLower() => E(nameof(PasswordRequiresLower),
        "A senha deve ter pelo menos uma letra minúscula.",
        "The password must have at least one lowercase letter.",
        "La contraseña debe tener al menos una letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => E(nameof(PasswordRequiresUpper),
        "A senha deve ter pelo menos uma letra maiúscula.",
        "The password must have at least one uppercase letter.",
        "La contraseña debe tener al menos una letra mayúscula.");
    public override IdentityError DuplicateEmail(string email) => E(nameof(DuplicateEmail),
        "Este e-mail já está cadastrado.", "This e-mail is already registered.", "Este correo ya está registrado.");
    public override IdentityError DuplicateUserName(string userName) => E(nameof(DuplicateUserName),
        "Este e-mail já está cadastrado.", "This e-mail is already registered.", "Este correo ya está registrado.");
    public override IdentityError InvalidEmail(string? email) => E(nameof(InvalidEmail),
        "Informe um e-mail válido.", "Enter a valid e-mail.", "Ingrese un correo válido.");
    public override IdentityError InvalidUserName(string? userName) => E(nameof(InvalidUserName),
        "Informe um e-mail válido.", "Enter a valid e-mail.", "Ingrese un correo válido.");
    public override IdentityError PasswordMismatch() => E(nameof(PasswordMismatch),
        "Senha incorreta.", "Incorrect password.", "Contraseña incorrecta.");
    public override IdentityError InvalidToken() => E(nameof(InvalidToken),
        "O link expirou ou já foi usado. Peça um novo.", "The link has expired or was already used. Request a new one.", "El enlace expiró o ya fue usado. Solicite uno nuevo.");
    public override IdentityError DefaultError() => E(nameof(DefaultError),
        "Não foi possível concluir a ação. Tente de novo.", "The action could not be completed. Try again.", "No se pudo completar la acción. Inténtelo de nuevo.");
    public override IdentityError ConcurrencyFailure() => E(nameof(ConcurrencyFailure),
        "Outra pessoa alterou este cadastro. Recarregue a página.", "Someone else changed this record. Reload the page.", "Otra persona cambió este registro. Recargue la página.");
}
