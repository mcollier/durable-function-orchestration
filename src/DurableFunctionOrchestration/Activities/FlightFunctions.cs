using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DurableFunctionOrchestration.Activities
{
    internal class FlightFunctions
    {
        private static HttpClient _httpClient = null!;

        public FlightFunctions(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ReservationClient");
        }

        [Function(nameof(FlightRegistrationAsync))]
        public async Task<FlightReservationRequest> FlightRegistrationAsync([ActivityTrigger] string userId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(FlightRegistrationAsync));
            logger.LogInformation("Creating flight registration for user {userId}.", userId);

            var request = GetReservationRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reservation/flight", content);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to create flight registration for user {userId}.", userId);
                return new();
            }

            logger.LogInformation("Flight registration created for user {userId}.", userId);

            return request;
        }

        private FlightReservationRequest GetReservationRequest()
        {
            // Create a ramdom FlightReservationRequest
            var random = new Random();
            var reservationRequest = new FlightReservationRequest
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John Doe Flight",
                To = "SEA",
                From = "MCO",
                Departure = DateTime.Now,
                Arrival = DateTime.Now.AddDays(random.Next(31, 60))
            };

            return reservationRequest;
        }
    }
}
