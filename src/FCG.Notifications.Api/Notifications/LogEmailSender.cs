namespace FCG.Notifications.Api.Notifications;

// Não existe provedor de e-mail nesta fase: "enviar" é registrar a mensagem no console.
public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;

    public LogEmailSender(ILogger<LogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancellationToken = default)
    {
        // O prefixo em maiúsculas e as barras fazem a linha se destacar no meio dos logs de infraestrutura.
        _logger.LogInformation(
            ">>>>>>>> E-MAIL ENVIADO ({Tipo}) <<<<<<<< Para: {Para} | Assunto: {Assunto} | Mensagem: {Corpo}",
            mensagem.Tipo, mensagem.Para, mensagem.Assunto, mensagem.Corpo);

        return Task.CompletedTask;
    }
}
