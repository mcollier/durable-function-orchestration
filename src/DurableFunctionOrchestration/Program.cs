using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
//using DurableFunctionsMonitor.DotNetIsolated;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication((hostBuilderContext, workerAppBuilder) => {
        //workerAppBuilder.UseDurableFunctionsMonitor();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var travelReservationBaseUri = Environment.GetEnvironmentVariable("TRAVEL_RESERVATION_BASE_URI");
        if (string.IsNullOrEmpty(travelReservationBaseUri))
        {
            throw new InvalidOperationException("Environment variable TRAVEL_RESERVATION_BASE_URI is not set.");
        }

        // Configures a named Reservation APIS http . 
        //   .AddStandardResilienceHandler(); is another option however the policy handler was done using Polly directly.
        services.AddHttpClient("ReservationClient", client =>
        {
            client.BaseAddress = new Uri("https://web-travel-api-eastus.azurewebsites.net/");
        })
        .AddPolicyHandler(GetRetryPolicy());
    })
    .Build();
host.Run();


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Jitter https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
    var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(2), retryCount: 5);

    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(delay);
}
