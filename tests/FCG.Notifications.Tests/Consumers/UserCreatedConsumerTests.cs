using FCG.Contracts;
using FCG.Notifications.Api.Consumers;
using FCG.Notifications.Api.Notifications;
using FCG.Notifications.Tests.Support;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FCG.Notifications.Tests.Consumers;

public class UserCreatedConsumerTests
{
    private static async Task<(ServiceProvider Provider, ITestHarness Harness, FakeEmailSender Sender)> IniciarAsync()
    {
        var sender = new FakeEmailSender();

        var provider = new ServiceCollection()
            .AddSingleton<IEmailSender>(sender)
            .AddLogging()
            .AddMassTransitTestHarness(bus => bus.AddConsumer<UserCreatedConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        return (provider, harness, sender);
    }

    [Fact]
    public async Task Deve_Enviar_O_Email_De_Boas_Vindas_Quando_Receber_UserCreatedEvent()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;
        var userId = Guid.NewGuid();

        await harness.Bus.Publish(new UserCreatedEvent(userId, "Maria Silva", "maria@teste.com"));

        Assert.True(await harness.Consumed.Any<UserCreatedEvent>());
        var email = Assert.Single(sender.Enviados);
        Assert.Equal("maria@teste.com", email.Para);
        Assert.Equal(TipoDeEmail.BoasVindas, email.Tipo);
        Assert.Contains("Maria Silva", email.Corpo);
    }

    [Fact]
    public async Task Cada_Evento_Deve_Gerar_Exatamente_Um_Email()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;

        await harness.Bus.Publish(new UserCreatedEvent(Guid.NewGuid(), "Um", "um@teste.com"));
        await harness.Bus.Publish(new UserCreatedEvent(Guid.NewGuid(), "Dois", "dois@teste.com"));

        Assert.True(await harness.Consumed.Any<UserCreatedEvent>(x => x.Context.Message.Nome == "Um"));
        Assert.True(await harness.Consumed.Any<UserCreatedEvent>(x => x.Context.Message.Nome == "Dois"));

        // A ordem de consumo não é garantida, então compara como conjunto.
        Assert.Equal(new[] { "dois@teste.com", "um@teste.com" }, sender.Enviados.Select(e => e.Para).Order());
    }

    [Fact]
    public async Task Evento_Sem_Email_Deve_Falhar_E_Nao_Enviar_Nada()
    {
        var (provider, harness, sender) = await IniciarAsync();
        await using var _ = provider;

        await harness.Bus.Publish(new UserCreatedEvent(Guid.NewGuid(), "Sem Email", ""));

        Assert.True(await harness.Published.Any<Fault<UserCreatedEvent>>());
        Assert.Empty(sender.Enviados);
    }
}
