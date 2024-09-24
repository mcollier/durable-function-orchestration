namespace DurableFunctionOrchestration.Models
{
    using System;

    namespace DurableFunctionOrchestration.Models
    {
        internal class ConfirmationRequest
        {
            public string Id { get; set; }
            public FlightDetails Flight { get; set; }
            public HotelDetails Hotel { get; set; }
        }

        internal class FlightDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public DateTime Departure { get; set; }
            public DateTime Arrival { get; set; }
        }

        internal class HotelDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public DateTime CheckIn { get; set; }
            public DateTime CheckOut { get; set; }
        }
    }
}
