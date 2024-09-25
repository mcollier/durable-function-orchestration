namespace DurableFunctionOrchestration.Models
{
    public class HotelReservationResult
    {
        public HotelReservationRequest? Reservation { get; set; }

        public required string Status { get; set; }

    }
}
