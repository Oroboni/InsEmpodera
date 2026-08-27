namespace Empodera.Services.Email;

public sealed class GmailSmtpOptions
{
    public const string SectionName = "Email";
    public const string GmailHost = "smtp.gmail.com";
    public const int GmailStartTlsPort = 587;

    public string User { get; set; } = "empodera.ajuda@gmail.com";
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "Instituto Empodera";
    public string PublicBaseUrl { get; set; } = string.Empty;
}
