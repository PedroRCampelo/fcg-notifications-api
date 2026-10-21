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
        ["RabbitMq:Queues:NotificationsUserCreated"] = "notifications.user-created"
    };

    [Fact]
    public void Carregar_Deve_Ler_Host_Credenciais_E_Nome_Da_Fila()
    {
        var settings = RabbitMqSettings.Carregar(Configuracao(Completa()));

        Assert.Equal("rabbitmq", settings.Host);
        Assert.Equal(5672, settings.Port);
        Assert.Equal("fcg", settings.Username);
        Assert.Equal("notifications.user-created", settings.Queues.NotificationsUserCreated);
    }

    [Fact]
    public void Carregar_Sem_Credenciais_Deve_Falhar_Apontando_As_Variaveis()
    {
        var valores = Completa();
        valores.Remove("RabbitMq:Password");

        var erro = Assert.Throws<InvalidOperationException>(() => RabbitMqSettings.Carregar(Configuracao(valores)));

        Assert.Contains("RabbitMq__Password", erro.Message);
    }

    [Fact]
    public void Carregar_Sem_Nome_Da_Fila_Deve_Falhar_Apontando_A_Variavel()
    {
        var valores = Completa();
        valores.Remove("RabbitMq:Queues:NotificationsUserCreated");

        var erro = Assert.Throws<InvalidOperationException>(() => RabbitMqSettings.Carregar(Configuracao(valores)));

        Assert.Contains("RabbitMq__Queues__NotificationsUserCreated", erro.Message);
    }
}
