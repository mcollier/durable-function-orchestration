using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DurableFunctionOrchestration.Activities
{
    public class HotelFunctions
    {
        private static HttpClient _httpClient = null!;

        public HotelFunctions(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ReservationClient");
        }

        [Function(nameof(RegistrationAsync))]
        public async Task<HotelReservationRequest> RegistrationAsync([ActivityTrigger] string userId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger<HotelFunctions>();
            logger.LogInformation("Creating hotel registration for user {userId}.", userId);

            var request = GetReservationRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // test retrys
            // content.Headers.Add("x-custom-status", "429");

            var response = await _httpClient.PostAsync("/api/reservation/hotel", content);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to create hotel registration for user {userId}.", userId);

                throw new HotelFunctionException("Failed to create hotel registration");
            }

            logger.LogInformation("Hotel registration created for user {userId}.", userId);

            return request;
        }

        private static HotelReservationRequest GetReservationRequest()
        {
            // Create a ramdom HotelReservationRequest
            var random = new Random();
            var reservationRequest = new HotelReservationRequest
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John Doe Hotel",
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(random.Next(31, 60)),
                Address = "123 Main St, Redmond, WA 98052",

            };
            return reservationRequest;
        }
    }
}
