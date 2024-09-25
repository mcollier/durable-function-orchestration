using DurableFunctionOrchestration.Models.DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DurableFunctionOrchestration.Activities
{
    internal class ConfirmationFunction
    {
        private static HttpClient _httpClient = null!;

        public ConfirmationFunction(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ReservationClient");
        }

        [Function(nameof(ConfirmationAsync))]
        public async Task<string> ConfirmationAsync([ActivityTrigger] ConfirmationRequest request, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(ConfirmationAsync));
            logger.LogInformation("Confirming flight and hotel registration.");

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reservation/confirmation", content);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get confirmation.");
                return response.Content.ToString();
            }

            logger.LogInformation("Hotel and Flight have been confirmed");

            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedContent = JsonSerializer.Deserialize<string>(responseContent);

            if (new Random().NextDouble() <= 0.7)
            {
                return "Failure";
            }

            return deserializedContent;
        }


    }
}
