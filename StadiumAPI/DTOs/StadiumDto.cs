namespace StadiumReservationApi.DTOs
{
    public class StadiumDto
    {
        public List<int>? FeatureIds { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int CategoryId { get; set; }
        public BookingPolicyDto? BookingPolicy { get; set; }
    }
}