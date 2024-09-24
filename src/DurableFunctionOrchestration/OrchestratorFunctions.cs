using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using DurableFunctionOrchestration.Models.DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public static class OrchestratorFunctions
    {
        [Function(nameof(OrchestratorFunctions))]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(OrchestratorFunctions));
            logger.LogInformation("Starting the orchestrator.");

            string? userId = context.GetInput<string>();

            logger.LogInformation(userId);

            var hotel = await context.CallActivityAsync<HotelReservationRequest>(nameof(HotelFunctions.RegistrationAsync), userId);
            var flight = await context.CallActivityAsync<FlightReservationRequest>(nameof(FlightFunctions.FlightRegistrationAsync), userId);
            var confirmationRequest = GetConfirmationRequest(hotel, flight);

            var confirmationResult = await context.CallActivityAsync<string>(
                nameof(ConfirmationFunction.ConfirmationAsync), confirmationRequest);

            return confirmationResult;
        }

        private static ConfirmationRequest GetConfirmationRequest(
            HotelReservationRequest hotel,
            FlightReservationRequest flight)
        {
            return new ConfirmationRequest
            {
                Id = "John Doe",
                Flight = new FlightDetails
                {
                    Id = flight.Id,
                    Arrival = flight.Arrival,
                    Departure = flight.Departure,
                    From = flight.From,
                    Name = flight.Name,
                    To = flight.To
                },
                Hotel = new HotelDetails
                {
                    Id = hotel.Id,
                    Address = hotel.Address,
                    CheckIn = hotel.CheckIn,
                    CheckOut = hotel.CheckOut,
                    Name = hotel.Name
                }
            };
        }
    }
}
