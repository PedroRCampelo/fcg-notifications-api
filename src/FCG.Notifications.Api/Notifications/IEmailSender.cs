namespace FCG.Notifications.Api.Notifications;

public interface IEmailSender
{
    Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancellationToken = default);
}
