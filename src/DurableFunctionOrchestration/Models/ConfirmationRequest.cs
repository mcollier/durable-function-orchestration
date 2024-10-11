namespace DurableFunctionOrchestration.Models
{
    using System;

    public class ConfirmationRequest
    {
        public required string Id { get; set; }
        public required FlightDetails Flight { get; set; }
        public required HotelDetails Hotel { get; set; }
    }

    public class FlightDetails
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
    }

    public class HotelDetails
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
