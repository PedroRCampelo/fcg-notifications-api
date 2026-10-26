using FCG.Notifications.Api.Notifications;

namespace FCG.Notifications.Tests.Support;

// Thread-safe: o harness do MassTransit pode consumir mensagens em paralelo.
public class FakeEmailSender : IEmailSender
{
    private readonly object _trava = new();
    private readonly List<MensagemDeEmail> _enviados = new();

    /// <summary>Cópia dos e-mails no momento da leitura (seguro para enumerar).</summary>
    public IReadOnlyList<MensagemDeEmail> Enviados
    {
        get { lock (_trava) return _enviados.ToList(); }
    }

    public Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancellationToken = default)
    {
        lock (_trava) _enviados.Add(mensagem);
        return Task.CompletedTask;
    }
}
