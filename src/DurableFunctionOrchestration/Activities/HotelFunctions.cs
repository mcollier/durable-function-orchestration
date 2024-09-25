using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace DurableFunctionOrchestration.Activities
{
    internal class HotelFunctions
    {
        private static HttpClient _httpClient = null!;

        public HotelFunctions(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ReservationClient");
        }

        [Function(nameof(RegistrationAsync))]
        public async Task<HotelReservationResult> RegistrationAsync([ActivityTrigger] string userId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(RegistrationAsync));
            logger.LogInformation("Creating hotel registration for user {userId}.", userId);

            var reservationRequest = GetReservationRequest();
            var json = JsonSerializer.Serialize(reservationRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reservation/hotel", content);

            switch (response.StatusCode) {
                case HttpStatusCode.Created:
                    logger.LogInformation("Hotel registration created for user {userId}.", userId);
                    
                    string responseData = await response.Content.ReadAsStringAsync();

                    return new HotelReservationResult {
                        Reservation = reservationRequest,
                        Status = "SUCCESS"
                    };

                case HttpStatusCode.NotFound:
                    logger.LogInformation("Hotel registration declined for user {userId}.", userId);
                    
                    return new HotelReservationResult {
                        Status = "FAIL"
                    };
                case HttpStatusCode.InternalServerError:
                    logger.LogError("Reservation Service responded with 500");

                    throw new Exception("Reservation Service responded with 500");
                default:
                    logger.LogError("Unexpected status code returned: {statusCode}", response.StatusCode);

                    throw new Exception("Service responded with unexpected status");
            }
        }

        [Function(nameof(CancelHotelReservationAsync))]
        public async Task<string> CancelHotelReservationAsync([ActivityTrigger] string reservationId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(CancelHotelReservationAsync));

            logger.LogInformation("Canceling hotel registration: {reservationId}.", reservationId);

            var response = await _httpClient.DeleteAsync($"/api/reservation/hotel/{reservationId}");

            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    logger.LogInformation("Canceled registration: {reservationId}.", reservationId);

                    string responseData = await response.Content.ReadAsStringAsync();

                    return "Success";

                case HttpStatusCode.NotFound:
                    logger.LogInformation("Hotel registration not found: {reservationId}.", reservationId);

                    return "Success";
                case HttpStatusCode.InternalServerError:
                    logger.LogError("Reservation Service responded with 500");

                    throw new Exception("Reservation Service responded with 500");
                default:
                    logger.LogError("Unexpected status code returned: {statusCode}", response.StatusCode);

                    throw new Exception("Service responded with unexpected status");
            }
        }

        private HotelReservationRequest GetReservationRequest()
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
