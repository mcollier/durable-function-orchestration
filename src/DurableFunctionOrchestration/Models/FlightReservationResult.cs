namespace DurableFunctionOrchestration.Models
{
    internal class FlightReservationResult
    {
        public FlightReservationRequest? Reservation { get; set; }

        public required string Status { get; set; }

    }
}
