using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DurableFunctionsMonitor.DotNetIsolated;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication((hostBuilderContext, workerAppBuilder) => {
        workerAppBuilder.UseDurableFunctionsMonitor();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configures a named Reservaton APIS http . 
        services.AddHttpClient("ReservationClient", client =>
        {
            client.BaseAddress = new Uri("https://web-travel-api-eastus.azurewebsites.net/");
        })
        .AddStandardResilienceHandler();
    })
    .Build();

host.Run();
