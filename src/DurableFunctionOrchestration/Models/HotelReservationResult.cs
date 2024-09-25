namespace DurableFunctionOrchestration.Models
{
    internal class HotelReservationResult
    {
        public HotelReservationRequest? Reservation { get; set; }

        public required string Status { get; set; }

    }
}
