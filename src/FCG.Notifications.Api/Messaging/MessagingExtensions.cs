using FCG.Notifications.Api.Consumers;
using FCG.Notifications.Api.Notifications;
using MassTransit;

namespace FCG.Notifications.Api.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = RabbitMqSettings.Carregar(configuration);

        services.AddSingleton<IEmailSender, LogEmailSender>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<UserCreatedConsumer>();

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbit.Host, (ushort)rabbit.Port, rabbit.VirtualHost, host =>
                {
                    host.Username(rabbit.Username);
                    host.Password(rabbit.Password);
                });

                // Fila com nome fixo e vindo de configuração: o publicador (UsersAPI) precisa
                // conhecer o mesmo nome para declará-la antes de este serviço subir.
                cfg.ReceiveEndpoint(rabbit.Queues.NotificationsUserCreated, endpoint =>
                {
                    endpoint.ConfigureConsumer<UserCreatedConsumer>(context);

                    endpoint.UseMessageRetry(retry =>
                    {
                        retry.Ignore<EventoInvalidoException>();
                        retry.Interval(3, TimeSpan.FromSeconds(2));
                    });
                });
            });
        });

        return services;
    }
}
