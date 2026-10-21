using FCG.Notifications.Api.Notifications;
using FCG.Notifications.Tests.Support;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FCG.Notifications.Tests.Notifications;

public class LogEmailSenderTests
{
    [Fact]
    public async Task Enviar_Deve_Registrar_Uma_Linha_Evidente_Com_Destinatario_Assunto_E_Mensagem()
    {
        var logger = new ListLogger<LogEmailSender>();
        var sender = new LogEmailSender(logger);

        await sender.EnviarAsync(new MensagemDeEmail(TipoDeEmail.BoasVindas, "maria@teste.com", "Bem-vinda!", "Olá, Maria!"));

        var (nivel, mensagem) = Assert.Single(logger.Registros);
        Assert.Equal(LogLevel.Information, nivel);
        Assert.Contains(">>>>>>>> E-MAIL ENVIADO (BoasVindas) <<<<<<<<", mensagem);
        Assert.Contains("Para: maria@teste.com", mensagem);
        Assert.Contains("Assunto: Bem-vinda!", mensagem);
        Assert.Contains("Mensagem: Olá, Maria!", mensagem);
    }
}
