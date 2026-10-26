using FCG.Contracts;
using FCG.Notifications.Api.Consumers;
using FCG.Notifications.Api.Notifications;
using FCG.Notifications.Tests.Support;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FCG.Notifications.Tests.Consumers;

public class PaymentProcessedConsumerTests
{
    private static async Task<(ServiceProvider Provider, ITestHarness Harness, FakeEmailSender Sender)> IniciarAsync()
    {
        var sender = new FakeEmailSender();

        var provider = new ServiceCollection()
            .AddSingleton<IEmailSender>(sender)
            .AddLogging()
            .AddMassTransitTestHarness(bus => bus.AddConsumer<PaymentProcessedConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        return (provider, harness, sender);
    }

    private static PaymentProcessedEvent Evento(PaymentStatus status, string titulo = "Hades") =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), titulo, status);

    [Fact]
    public async Task Pagamento_Aprovado_Deve_Enviar_O_Email_De_Confirmacao_Da_Compra()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;
        var evento = Evento(PaymentStatus.Approved, "Hades");

        await harness.Bus.Publish(evento);

        Assert.True(await harness.Consumed.Any<PaymentProcessedEvent>());
        var email = Assert.Single(sender.Enviados);
        Assert.Equal(TipoDeEmail.ConfirmacaoDeCompra, email.Tipo);
        Assert.Contains("Hades", email.Corpo);
        Assert.Contains(evento.UserId.ToString(), email.Para);
    }

    [Fact]
    public async Task Pagamento_Recusado_Nao_Deve_Enviar_Nenhum_Email()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;

        await harness.Bus.Publish(Evento(PaymentStatus.Rejected));

        Assert.True(await harness.Consumed.Any<PaymentProcessedEvent>());
        Assert.Empty(sender.Enviados);
        Assert.False(await harness.Published.Any<Fault<PaymentProcessedEvent>>());
    }

    [Fact]
    public async Task Status_Desconhecido_Deve_Falhar_Sem_Enviar_Nada()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;

        await harness.Bus.Publish(Evento((PaymentStatus)99));

        Assert.True(await harness.Published.Any<Fault<PaymentProcessedEvent>>());
        Assert.Empty(sender.Enviados);
    }
}
