using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateEventDto
    {
        public int VenueId { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
    }

    public class UpdateEventDto : CreateEventDto
    {
        [Required] public string Status { get; set; } = "Upcoming";
    }

    public class CreateCategoryDto
    {
        [Required] public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
    }
}