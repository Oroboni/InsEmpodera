namespace Empodera.Services.Email;

public interface IPasswordResetEmailSender
{
    bool TryQueue(string recipientEmail, string resetUrl);
}
