using FCG.Contracts;
using FCG.Notifications.Api.Notifications;
using MassTransit;

namespace FCG.Notifications.Api.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(IEmailSender emailSender, ILogger<PaymentProcessedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation("PaymentProcessedEvent recebido para o pedido {OrderId} ({StatusDoPagamento})", evento.OrderId, evento.Status);

        var email = ComposicaoDeEmails.ConfirmacaoDeCompra(evento);

        if (email is null)
        {
            _logger.LogInformation("Pagamento do pedido {OrderId} recusado: nenhum e-mail é enviado", evento.OrderId);
            return;
        }

        await _emailSender.EnviarAsync(email, context.CancellationToken);
    }
}
