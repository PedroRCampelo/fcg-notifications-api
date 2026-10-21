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
        // O prefixo em maiúsculas e as setas fazem a linha se destacar no meio dos logs de infraestrutura.
        // O formato :l (literal) evita que o Serilog envolva os textos em aspas na mensagem renderizada.
        _logger.LogInformation(
            ">>>>>>>> E-MAIL ENVIADO ({Tipo}) <<<<<<<< Para: {Para:l} | Assunto: {Assunto:l} | Mensagem: {Corpo:l}",
            mensagem.Tipo, mensagem.Para, mensagem.Assunto, mensagem.Corpo);

        return Task.CompletedTask;
    }
}
