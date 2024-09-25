using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
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
        public async Task<FlightReservationResult> FlightRegistrationAsync([ActivityTrigger] string userId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(FlightRegistrationAsync));
            logger.LogInformation("Creating flight registration for user {userId}.", userId);

            var request = GetReservationRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reservation/flight", content);

            return new FlightReservationResult
            {
                Status = "FAIL"
            };

            switch (response.StatusCode) {
                case HttpStatusCode.Created:
                    string responseData = await response.Content.ReadAsStringAsync();

                    logger.LogInformation("Flight registration created for user {userId}.", userId);
                    
                    return new FlightReservationResult {
                        Reservation = request,
                        Status = "SUCCESS"
                    };

                case HttpStatusCode.NotFound:
                    logger.LogError("Failed to create flight registration for user {userId}.", userId);
                    
                    return new FlightReservationResult {
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

        [Function(nameof(CancelFlightReservationAsync))]
        public async Task<string> CancelFlightReservationAsync([ActivityTrigger] string reservationId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(CancelFlightReservationAsync));

            logger.LogInformation("Canceling hotel registration: {reservationId}.", reservationId);

            var response = await _httpClient.DeleteAsync($"/api/reservation/flight/{reservationId}");

            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    logger.LogInformation("Canceled registration: {reservationId}.", reservationId);

                    string responseData = await response.Content.ReadAsStringAsync();

                    return "Success";

                case HttpStatusCode.NotFound:
                    logger.LogInformation("Flight registration not found: {reservationId}.", reservationId);

                    return "Success";
                case HttpStatusCode.InternalServerError:
                    logger.LogError("Reservation Service responded with 500");

                    throw new Exception("Reservation Service responded with 500");
                default:
                    logger.LogError("Unexpected status code returned: {statusCode}", response.StatusCode);

                    throw new Exception("Service responded with unexpected status");
            }
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
