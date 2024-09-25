namespace DurableFunctionOrchestration.Models
{
    public class FlightReservationResult
    {
        public FlightReservationRequest? Reservation { get; set; }

        public required string Status { get; set; }

    }
}
