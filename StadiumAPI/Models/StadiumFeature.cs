namespace StadiumReservationApi.Models
{
    public class StadiumFeature
    {
        public int StadiumId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Stadium? Stadium { get; set; }

        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
    }
}