using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DurableFunctionOrchestration.Activities
{
    public class ConfirmationFunctions
    {
        private static HttpClient _httpClient = null!;

        public ConfirmationFunctions(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ReservationClient");
        }

        [Function(nameof(ConfirmationAsync))]
        public async Task<string> ConfirmationAsync([ActivityTrigger] ConfirmationRequest request, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger<ConfirmationFunctions>();
            logger.LogInformation("Confirming flight and hotel registration.");

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reservation/confirmation", content);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get confirmation.");
                return response.Content?.ToString() ?? "Failed to get confirmation.";
            }

            logger.LogInformation("Hotel and Flight have been confirmed");

            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedContent = JsonSerializer.Deserialize<string>(responseContent);

            if (new Random().NextDouble() <= 0.7)
            {
                return "Failure";
            }

            return deserializedContent ?? "Failed to get confirmation.";
        }
    }
}
