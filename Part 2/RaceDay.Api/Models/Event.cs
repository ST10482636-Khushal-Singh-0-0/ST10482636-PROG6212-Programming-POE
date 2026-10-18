using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        public int OrganiserId { get; set; }
        public User? Organiser { get; set; }

        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required, MaxLength(30)]
        public string EventType { get; set; } = "Running Marathon";

        public DateTime EventDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Upcoming";

        public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
    }
}