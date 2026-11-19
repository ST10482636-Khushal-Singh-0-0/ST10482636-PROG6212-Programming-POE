using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } = "Participant";
    }

    public class UserSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EffectiveId => Id > 0 ? Id : UserId;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class EventViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int EffectiveId => Id > 0 ? Id : EventId;

        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        [Required] public DateTime Date { get; set; } = DateTime.Now.AddDays(14);
        [Required] public string Location { get; set; } = string.Empty;
        public string? Distance { get; set; } = string.Empty;
        [Required] public string EventType { get; set; } = string.Empty;
        public int OrganiserId { get; set; }
        public string? BannerImageUrl { get; set; }
        public IFormFile? BannerImage { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new();
        public int TotalEnrolments { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int EffectiveId => CategoryId > 0 ? CategoryId : Id;
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string EffectiveName => !string.IsNullOrWhiteSpace(CategoryName) ? CategoryName : Name;
        public string? AgeGroup { get; set; }
        public string? Distance { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; } = 500;
    }

    public class EnrolmentViewModel
    {
        public int Id { get; set; }
        public int EnrolmentId { get; set; }
        public int EffectiveId => EnrolmentId > 0 ? EnrolmentId : Id;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int RaceNumber { get; set; }
        public string PaymentStatus { get; set; } = "Confirmed";
        public string Status { get; set; } = "Confirmed";
    }
    public class ResultViewModel
    {
        public int Id { get; set; }
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        [Required] public string FinishTime { get; set; } = string.Empty;
        [Required] public int Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}