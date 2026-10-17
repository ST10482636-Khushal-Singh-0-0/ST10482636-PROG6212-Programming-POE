using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateEnrolmentDto
    {
        [Required] public int CategoryId { get; set; }
    }

    public class CreateResultDto
    {
        [Required] public int EnrolmentId { get; set; }
        public int FinishTimeSeconds { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }
        public string Status { get; set; } = "Finished";
    }
}