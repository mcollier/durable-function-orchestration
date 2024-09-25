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

            TaskOptions retryOptions = TaskOptions.FromRetryHandler(retryContext =>
            {
                // Don't retry anything that derives from ApplicationException
                if(retryContext.LastFailure.IsCausedBy<ApplicationException>())
                {
                    return false;
                }

                // Quit after N attempts
                return retryContext.LastAttemptNumber < 3;
            });

            var hotelReservationResult = await context.CallActivityAsync<HotelReservationResult>(nameof(HotelFunctions.RegistrationAsync), userId);
            
            if(hotelReservationResult.Status == "FAIL") {
                return "FAIL";
            }

            var flightReservationResult = await context.CallActivityAsync<FlightReservationResult>(nameof(FlightFunctions.FlightRegistrationAsync), userId);
            
            if(flightReservationResult.Status == "FAIL") {
                var cancelResponse = await context.CallActivityAsync<string>(nameof(HotelFunctions.CancelHotelReservationAsync), hotelReservationResult.Reservation?.Id);

                return "FAIL";
            }

            var confirmationRequest = GetConfirmationRequest(hotelReservationResult.Reservation, flightReservationResult.Reservation);

            var confirmationResult = await context.CallActivityAsync<string>(
                nameof(ConfirmationFunction.ConfirmationAsync), confirmationRequest);

            if(confirmationResult == "FAIL")
            {
                var flightCancelResponse = await context.CallActivityAsync<string>(nameof(FlightFunctions.CancelFlightReservationAsync), flightReservationResult.Reservation?.Id);
                var hotelCancelResponse = await context.CallActivityAsync<string>(nameof(HotelFunctions.CancelHotelReservationAsync), hotelReservationResult.Reservation?.Id);
                
                return "FAIL";
            }

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
