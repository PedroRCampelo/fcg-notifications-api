using FCG.Contracts;
using FCG.Notifications.Api.Notifications;
using Xunit;

namespace FCG.Notifications.Tests.Notifications;

public class ComposicaoDeEmailsPagamentoTests
{
    private static PaymentProcessedEvent Evento(PaymentStatus status, string titulo = "Hades", Guid? userId = null) =>
        new(Guid.NewGuid(), userId ?? Guid.NewGuid(), Guid.NewGuid(), titulo, status);

    [Fact]
    public void Pagamento_Aprovado_Deve_Gerar_Email_De_Confirmacao_Com_O_Titulo_Do_Jogo()
    {
        var usuario = Guid.NewGuid();

        var email = ComposicaoDeEmails.ConfirmacaoDeCompra(Evento(PaymentStatus.Approved, "Cyberpunk 2077", usuario));

        Assert.NotNull(email);
        Assert.Equal(TipoDeEmail.ConfirmacaoDeCompra, email.Tipo);
        Assert.Contains("Cyberpunk 2077", email.Assunto);
        Assert.Contains("Cyberpunk 2077", email.Corpo);
        Assert.Contains(usuario.ToString(), email.Para);
    }

    [Fact]
    public void Pagamento_Recusado_Nao_Deve_Gerar_Email()
    {
        Assert.Null(ComposicaoDeEmails.ConfirmacaoDeCompra(Evento(PaymentStatus.Rejected)));
    }

    [Fact]
    public void Titulo_Ausente_Deve_Usar_Um_Tratamento_Generico()
    {
        var email = ComposicaoDeEmails.ConfirmacaoDeCompra(Evento(PaymentStatus.Approved, "  "));

        Assert.NotNull(email);
        Assert.Contains("seu jogo", email.Assunto);
    }

    [Fact]
    public void Evento_Sem_Usuario_Deve_Lancar_Evento_Invalido()
    {
        Assert.Throws<EventoInvalidoException>(() => ComposicaoDeEmails.ConfirmacaoDeCompra(Evento(PaymentStatus.Approved, userId: Guid.Empty)));
    }

    [Fact]
    public void Status_Desconhecido_Deve_Lancar_Evento_Invalido_Em_Vez_De_Enviar_Ou_Ignorar_Em_Silencio()
    {
        Assert.Throws<EventoInvalidoException>(() => ComposicaoDeEmails.ConfirmacaoDeCompra(Evento((PaymentStatus)99)));
    }
}
