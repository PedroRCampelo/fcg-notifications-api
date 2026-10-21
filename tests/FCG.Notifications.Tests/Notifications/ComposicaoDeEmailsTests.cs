using FCG.Contracts;
using FCG.Notifications.Api.Notifications;
using Xunit;

namespace FCG.Notifications.Tests.Notifications;

public class ComposicaoDeEmailsTests
{
    private static UserCreatedEvent Evento(string nome = "Maria Silva", string email = "maria@teste.com") =>
        new(Guid.NewGuid(), nome, email);

    [Fact]
    public void BoasVindas_Deve_Enderecar_O_Email_Para_O_Usuario_Do_Evento()
    {
        var mensagem = ComposicaoDeEmails.BoasVindas(Evento(email: "maria@teste.com"));

        Assert.Equal("maria@teste.com", mensagem.Para);
        Assert.Equal(TipoDeEmail.BoasVindas, mensagem.Tipo);
    }

    [Fact]
    public void BoasVindas_Deve_Chamar_O_Usuario_Pelo_Nome_No_Assunto_E_No_Corpo()
    {
        var mensagem = ComposicaoDeEmails.BoasVindas(Evento(nome: "Maria Silva"));

        Assert.Contains("Maria Silva", mensagem.Assunto);
        Assert.Contains("Maria Silva", mensagem.Corpo);
        Assert.Contains("FIAP Cloud Games", mensagem.Assunto);
    }

    [Fact]
    public void BoasVindas_Deve_Ignorar_Espacos_Nas_Pontas()
    {
        var mensagem = ComposicaoDeEmails.BoasVindas(Evento(nome: "  Maria  ", email: "  maria@teste.com "));

        Assert.Equal("maria@teste.com", mensagem.Para);
        Assert.Contains("Maria!", mensagem.Assunto);
    }

    [Fact]
    public void BoasVindas_Sem_Nome_Deve_Usar_Um_Tratamento_Generico()
    {
        var mensagem = ComposicaoDeEmails.BoasVindas(Evento(nome: "  "));

        Assert.Contains("jogador", mensagem.Assunto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BoasVindas_Sem_Email_De_Destino_Deve_Lancar_Evento_Invalido(string email)
    {
        Assert.Throws<EventoInvalidoException>(() => ComposicaoDeEmails.BoasVindas(Evento(email: email)));
    }
}
