using FCG.Notifications.Api.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FCG.Notifications.Tests.Messaging;

public class RabbitMqSettingsTests
{
    private static IConfiguration Configuracao(Dictionary<string, string?> valores) =>
        new ConfigurationBuilder().AddInMemoryCollection(valores).Build();

    private static Dictionary<string, string?> Completa() => new()
    {
        ["RabbitMq:Host"] = "rabbitmq",
        ["RabbitMq:Username"] = "fcg",
        ["RabbitMq:Password"] = "segredo",
        ["RabbitMq:Queues:NotificationsUserCreated"] = "notifications.user-created",
        ["RabbitMq:Queues:NotificationsPaymentProcessed"] = "notifications.payment-processed"
    };

    [Fact]
    public void Carregar_Deve_Ler_Host_Credenciais_E_Nome_Da_Fila()
    {
        var settings = RabbitMqSettings.Carregar(Configuracao(Completa()));

        Assert.Equal("rabbitmq", settings.Host);
        Assert.Equal(5672, settings.Port);
        Assert.Equal("fcg", settings.Username);
        Assert.Equal("notifications.user-created", settings.Queues.NotificationsUserCreated);
        Assert.Equal("notifications.payment-processed", settings.Queues.NotificationsPaymentProcessed);
    }

    [Fact]
    public void Carregar_Sem_Credenciais_Deve_Falhar_Apontando_As_Variaveis()
    {
        var valores = Completa();
        valores.Remove("RabbitMq:Password");

        var erro = Assert.Throws<InvalidOperationException>(() => RabbitMqSettings.Carregar(Configuracao(valores)));

        Assert.Contains("RabbitMq__Password", erro.Message);
    }

    [Theory]
    [InlineData("RabbitMq:Queues:NotificationsUserCreated", "RabbitMq__Queues__NotificationsUserCreated")]
    [InlineData("RabbitMq:Queues:NotificationsPaymentProcessed", "RabbitMq__Queues__NotificationsPaymentProcessed")]
    public void Carregar_Sem_Nome_De_Fila_Deve_Falhar_Apontando_A_Variavel(string chave, string variavel)
    {
        var valores = Completa();
        valores.Remove(chave);

        var erro = Assert.Throws<InvalidOperationException>(() => RabbitMqSettings.Carregar(Configuracao(valores)));

        Assert.Contains(variavel, erro.Message);
    }
}
