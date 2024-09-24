using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
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
