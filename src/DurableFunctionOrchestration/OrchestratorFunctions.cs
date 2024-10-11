using DurableFunctionOrchestration.Activities;
using DurableFunctionOrchestration.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public static class OrchestratorFunctions
    {
        [Function(nameof(RunOrchestrator))]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(RunOrchestrator));
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
                var hotelReservationResult = await context.CallActivityAsync<HotelReservationResult>(nameof(HotelFunctions.RegistrationAsync), userId);

                if (hotelReservationResult.Status == "FAIL")
                {
                    return "FAIL";
                }

                var flightReservationResult = await context.CallActivityAsync<FlightReservationResult>(nameof(FlightFunctions.FlightRegistrationAsync), userId);

                if (flightReservationResult.Status == "FAIL")
                {
                    var cancelResponse = await context.CallActivityAsync<string>(nameof(HotelFunctions.CancelHotelReservationAsync), hotelReservationResult.Reservation?.Id);

                    return "FAIL";
                }

                if (hotelReservationResult.Reservation == null || flightReservationResult.Reservation == null)
                {
                    throw new InvalidOperationException("Reservation details cannot be null.");
                }

                var confirmationRequest = GetConfirmationRequest(hotelReservationResult.Reservation, flightReservationResult.Reservation);

                var confirmationState = await context.CallActivityAsync<string>(
                    nameof(ConfirmationFunctions.ConfirmationAsync), confirmationRequest);

                if (confirmationState.Contains("fail", StringComparison.OrdinalIgnoreCase))
                {
                    context.SetCustomStatus("Pending approval for cancellation");
                    var approvalStatus = await context.WaitForExternalEvent<ApprovalRequest>("Approval");
                    context.SetCustomStatus(null);
                    if (approvalStatus.Approved)
                    {
                        var flightCancelResponse = await context.CallActivityAsync<string>(nameof(FlightFunctions.CancelFlightReservationAsync), flightReservationResult.Reservation?.Id);
                        var hotelCancelResponse = await context.CallActivityAsync<string>(nameof(HotelFunctions.CancelHotelReservationAsync), hotelReservationResult.Reservation?.Id);

                        return "The flight and hotel were cancelled";
                    }
                    return "The flight was NOT cancelled";
                }

                return confirmationState;

            }
            catch (TaskFailedException e)
            {
                // TODO: Create a more descriptive error message.
                logger.LogError(e, "An error occurred.");
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
