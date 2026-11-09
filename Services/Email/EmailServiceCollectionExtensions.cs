namespace Empodera.Services.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddGmailPasswordRecovery(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = services
            .AddOptions<GmailSmtpOptions>()
            .Bind(configuration.GetSection(GmailSmtpOptions.SectionName))
            .Validate(
                value => string.Equals(
                    value.User?.Trim(),
                    "empodera.ajuda@gmail.com",
                    StringComparison.OrdinalIgnoreCase),
                "Email:User deve ser empodera.ajuda@gmail.com.")
            .Validate(value => !string.IsNullOrWhiteSpace(value.FromName), "Email:FromName é obrigatório.");

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            options
                // Sem Email__Password a recuperação por e-mail fica desligada; com ela, a configuração precisa ser válida.
                .Validate(
                    value => !value.Configurado || value.Password.Replace(" ", string.Empty).Length >= 16,
                    "Defina Email__Password com uma senha de aplicativo válida do Gmail (ou deixe vazio para desligar a recuperação por e-mail).")
                .Validate(
                    value => !value.Configurado ||
                             (Uri.TryCreate(value.PublicBaseUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps),
                    "Defina Email__PublicBaseUrl com a URL HTTPS pública do Empodera.")
                .ValidateOnStart();
        }

        services.AddSingleton<GmailSmtpPasswordResetEmailSender>();
        services.AddSingleton<IPasswordResetEmailSender>(serviceProvider =>
            serviceProvider.GetRequiredService<GmailSmtpPasswordResetEmailSender>());
        services.AddHostedService(serviceProvider =>
            serviceProvider.GetRequiredService<GmailSmtpPasswordResetEmailSender>());
        return services;
    }
}
