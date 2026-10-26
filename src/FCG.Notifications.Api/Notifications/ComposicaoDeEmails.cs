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

    // Devolve null quando não há nada a enviar: só pagamento aprovado gera e-mail de confirmação.
    // O evento não traz o endereço de e-mail do usuário (só o UserId), então o destinatário fica
    // identificado pelo id.
    public static MensagemDeEmail? ConfirmacaoDeCompra(PaymentProcessedEvent evento)
    {
        if (evento.UserId == Guid.Empty)
            throw new EventoInvalidoException($"O pagamento do pedido {evento.OrderId} não identifica o usuário.");

        switch (evento.Status)
        {
            case PaymentStatus.Approved:
                var titulo = string.IsNullOrWhiteSpace(evento.Titulo) ? "seu jogo" : evento.Titulo.Trim();

                return new MensagemDeEmail(
                    TipoDeEmail.ConfirmacaoDeCompra,
                    $"usuário {evento.UserId}",
                    $"Compra confirmada: {titulo}",
                    $"Sua compra de {titulo} foi aprovada. Bom jogo!");

            case PaymentStatus.Rejected:
                return null;

            default:
                throw new EventoInvalidoException($"O pagamento do pedido {evento.OrderId} veio com um status desconhecido: {(int)evento.Status}.");
        }
    }
}
