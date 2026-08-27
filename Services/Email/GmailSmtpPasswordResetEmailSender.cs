using System.Net;
using System.Net.Mail;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Empodera.Services.Email;

public sealed class GmailSmtpPasswordResetEmailSender : BackgroundService, IPasswordResetEmailSender
{
    private readonly Channel<PasswordResetEmail> _queue = Channel.CreateBounded<PasswordResetEmail>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    private readonly IOptionsMonitor<GmailSmtpOptions> _options;
    private readonly ILogger<GmailSmtpPasswordResetEmailSender> _logger;

    public GmailSmtpPasswordResetEmailSender(
        IOptionsMonitor<GmailSmtpOptions> options,
        ILogger<GmailSmtpPasswordResetEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public bool TryQueue(string recipientEmail, string resetUrl)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail) ||
            !Uri.TryCreate(resetUrl, UriKind.Absolute, out var resetUri) ||
            resetUri.Scheme != Uri.UriSchemeHttps)
            return false;

        return _queue.Writer.TryWrite(new PasswordResetEmail(recipientEmail.Trim(), resetUri.AbsoluteUri));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SendAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Não foi possível enviar um e-mail de recuperação pelo Gmail SMTP.");
            }
        }
    }

    private async Task SendAsync(PasswordResetEmail message, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        var gmailUser = options.User?.Trim();
        if (string.IsNullOrWhiteSpace(gmailUser) ||
            !string.Equals(gmailUser, "empodera.ajuda@gmail.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Email__User deve ser empodera.ajuda@gmail.com para o remetente oficial do sistema.");
        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException(
                "Defina Email__Password com a senha de aplicativo do Gmail no cofre de segredos do servidor.");

        var gmailAppPassword = options.Password.Replace(" ", string.Empty);
        var from = new MailAddress(gmailUser, options.FromName?.Trim() ?? "Instituto Empodera");
        var recipient = new MailAddress(message.RecipientEmail);
        using var mail = new MailMessage(from, recipient)
        {
            Subject = "Redefinição de senha — Empodera",
            IsBodyHtml = true,
            Body = CreateHtmlBody(message.ResetUrl)
        };
        mail.Headers.Add("Auto-Submitted", "auto-generated");
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            CreateTextBody(message.ResetUrl),
            null,
            "text/plain"));

        using var smtp = new SmtpClient(GmailSmtpOptions.GmailHost, GmailSmtpOptions.GmailStartTlsPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(gmailUser, gmailAppPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15_000
        };

        cancellationToken.ThrowIfCancellationRequested();
        await smtp.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("E-mail de recuperação enviado pelo Gmail SMTP.");
    }

    private static string CreateHtmlBody(string resetUrl)
    {
        var safeUrl = WebUtility.HtmlEncode(resetUrl);
        return $$"""
            <!doctype html>
            <html lang="pt-BR">
            <body style="margin:0;background:#f4f1f6;font-family:Arial,sans-serif;color:#2f2733">
              <div style="max-width:580px;margin:32px auto;background:#fff;border-radius:18px;overflow:hidden;border:1px solid #e7dfea">
                <div style="background:#7c4288;color:#fff;padding:26px 32px">
                  <h1 style="font-size:22px;margin:0">Redefinição de senha</h1>
                </div>
                <div style="padding:30px 32px;line-height:1.6">
                  <p>Recebemos uma solicitação para redefinir a senha da sua conta no Empodera.</p>
                  <p style="margin:28px 0;text-align:center">
                    <a href="{{safeUrl}}" style="display:inline-block;background:#6bc0b7;color:#173c39;text-decoration:none;font-weight:700;padding:13px 22px;border-radius:999px">Redefinir minha senha</a>
                  </p>
                  <p>Este link expira em <strong>30 minutos</strong> e só pode ser usado para essa conta.</p>
                  <p>Se você não solicitou a alteração, ignore este e-mail. Sua senha continuará igual.</p>
                  <hr style="border:0;border-top:1px solid #eee;margin:26px 0">
                  <p style="font-size:13px;color:#6f6872">O Instituto Empodera nunca solicitará sua senha por e-mail.</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private static string CreateTextBody(string resetUrl) => $$"""
        Redefinição de senha — Empodera

        Recebemos uma solicitação para redefinir a senha da sua conta.

        Abra o endereço abaixo em até 30 minutos:
        {{resetUrl}}

        Se você não solicitou a alteração, ignore este e-mail. Sua senha continuará igual.
        O Instituto Empodera nunca solicitará sua senha por e-mail.
        """;

    private sealed record PasswordResetEmail(string RecipientEmail, string ResetUrl);
}
