namespace DurableFunctionOrchestration.Models
{
    public class FlightReservationRequest
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }

    }
}
