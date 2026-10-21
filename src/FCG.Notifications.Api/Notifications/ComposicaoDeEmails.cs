using FCG.Contracts;

namespace FCG.Notifications.Api.Notifications;

public static class ComposicaoDeEmails
{
    public static MensagemDeEmail BoasVindas(UserCreatedEvent evento)
    {
        if (string.IsNullOrWhiteSpace(evento.Email))
            throw new EventoInvalidoException($"O evento do usuário {evento.UserId} não tem e-mail de destino.");

        var nome = string.IsNullOrWhiteSpace(evento.Nome) ? "jogador" : evento.Nome.Trim();

        return new MensagemDeEmail(
            TipoDeEmail.BoasVindas,
            evento.Email.Trim(),
            $"Bem-vindo(a) à FIAP Cloud Games, {nome}!",
            $"Olá, {nome}! Sua conta na FIAP Cloud Games foi criada com sucesso. " +
            "Explore o catálogo e monte sua biblioteca de jogos.");
    }
}
