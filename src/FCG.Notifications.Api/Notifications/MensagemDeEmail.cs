namespace FCG.Notifications.Api.Notifications;

public sealed record MensagemDeEmail(TipoDeEmail Tipo, string Para, string Assunto, string Corpo);
