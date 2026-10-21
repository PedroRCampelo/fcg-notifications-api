using FCG.Contracts;
using FCG.Notifications.Api.Notifications;
using MassTransit;

namespace FCG.Notifications.Api.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(IEmailSender emailSender, ILogger<UserCreatedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation("UserCreatedEvent recebido para o usuário {UserId}", evento.UserId);

        var email = ComposicaoDeEmails.BoasVindas(evento);
        await _emailSender.EnviarAsync(email, context.CancellationToken);
    }
}
