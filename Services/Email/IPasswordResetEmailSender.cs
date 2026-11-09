namespace Empodera.Services.Email;

public interface IPasswordResetEmailSender
{
    bool TryQueue(string recipientEmail, string resetUrl);

    // Falso quando o e-mail de recuperação ainda não foi configurado: a tela "Esqueci minha senha"
    // avisa para procurar o administrador em vez de prometer um e-mail que não vai chegar.
    bool Disponivel => true;
}
