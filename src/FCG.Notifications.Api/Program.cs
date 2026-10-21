using FCG.Notifications.Api.Messaging;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddMessaging(builder.Configuration);

    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    // Este serviço só consome eventos. O único endpoint HTTP é o de saúde (probes do Docker e do Kubernetes).
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "fcg-notifications-api encerrada de forma inesperada durante a inicialização");
    // Código de saída diferente de zero: num container ou pod, é o que sinaliza a falha de subida.
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
