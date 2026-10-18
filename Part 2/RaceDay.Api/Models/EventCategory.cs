using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class EventCategory
    {
        [Key]
        public int CategoryId { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}