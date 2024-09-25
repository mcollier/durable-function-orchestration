using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
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

            TaskOptions retryOptions = TaskOptions.FromRetryHandler(retryContext =>
            {
                // Don't retry anything that derives from ApplicationException
                if (retryContext.LastFailure.IsCausedBy<ApplicationException>())
                {
                    return false;
                }

                // Quit after N attempts
                return retryContext.LastAttemptNumber < 3;
            });

            try
            {
                var hotel = await context.CallActivityAsync<HotelReservationRequest>(nameof(HotelFunctions.RegistrationAsync), userId, retryOptions);
                var flight = await context.CallActivityAsync<FlightReservationRequest>(nameof(FlightFunctions.FlightRegistrationAsync), userId, retryOptions);
                var confirmationRequest = GetConfirmationRequest(hotel, flight);

                var confirmationResult = await context.CallActivityAsync<string>(
                    nameof(ConfirmationFunctions.ConfirmationAsync), confirmationRequest, retryOptions);

                return confirmationResult;
            }
            catch (TaskFailedException e)
            {
                logger.LogError("Task Failed", e);

                return "FAIL";
            }
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
