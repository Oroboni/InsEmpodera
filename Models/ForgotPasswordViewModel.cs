using System.ComponentModel.DataAnnotations;

namespace Empodera.Models;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;
}
