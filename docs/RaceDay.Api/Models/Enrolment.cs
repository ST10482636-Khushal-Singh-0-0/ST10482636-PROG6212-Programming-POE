using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        public int CategoryId { get; set; }
        public EventCategory? Category { get; set; }

        public int ParticipantId { get; set; }
        public User? Participant { get; set; }

        public int RaceNumber { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Confirmed";

        public Result? Result { get; set; }
    }
}