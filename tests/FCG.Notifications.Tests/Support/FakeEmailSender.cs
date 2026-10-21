using FCG.Notifications.Api.Notifications;

namespace FCG.Notifications.Tests.Support;

public class FakeEmailSender : IEmailSender
{
    public List<MensagemDeEmail> Enviados { get; } = new();

    public Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancellationToken = default)
    {
        Enviados.Add(mensagem);
        return Task.CompletedTask;
    }
}
