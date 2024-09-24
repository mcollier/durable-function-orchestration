namespace DurableFunctionOrchestration.Models
{
    internal class HotelReservationRequest
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public DateTime CheckIn { get; set; }

        public DateTime CheckOut { get; set; }
    }
}
