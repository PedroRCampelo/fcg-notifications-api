namespace FCG.Notifications.Api.Notifications;

// Evento que não tem como virar e-mail. Repetir o processamento não resolve, então não há retentativa.
public sealed class EventoInvalidoException : Exception
{
    public EventoInvalidoException(string mensagem) : base(mensagem)
    {
    }
}
