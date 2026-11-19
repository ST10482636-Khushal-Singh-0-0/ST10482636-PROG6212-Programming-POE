namespace RaceDay.Api.DTOs
{
    public class EventFormDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int VenueId { get; set; }
        public string? Location { get; set; }
        public string? Distance { get; set; }
        public string EventType { get; set; } = "Running Marathon";
        public string Status { get; set; } = "Upcoming";
        public int OrganiserId { get; set; }
    }
}