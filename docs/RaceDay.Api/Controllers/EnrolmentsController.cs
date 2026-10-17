using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            var category = await _context.EventCategories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null) return NotFound(new { message = "Category not found." });

            // Check if user is already enrolled
            bool alreadyEnrolled = category.Enrolments.Any(e => e.ParticipantId == participantId);
            if (alreadyEnrolled) return Conflict(new { message = "You are already enrolled in this category." });

            // Check capacity limit
            if (category.Enrolments.Count >= category.MaxParticipants)
                return BadRequest(new { message = "This category has reached maximum participant capacity." });

            // Generate next available race number
            int nextRaceNumber = category.Enrolments.Count == 0
                ? (category.CategoryId * 1000) + 1
                : category.Enrolments.Max(e => e.RaceNumber) + 1;

            var enrolment = new Enrolment
            {
                CategoryId = dto.CategoryId,
                ParticipantId = participantId,
                RaceNumber = nextRaceNumber,
                PaymentStatus = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, enrolment);
        }

        [HttpGet("my-entries")]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> GetMyEntries()
        {
            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            var entries = await _context.Enrolments
                .Where(e => e.ParticipantId == participantId)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.RaceNumber,
                    e.PaymentStatus,
                    e.EnrolmentDate,
                    CategoryName = e.Category!.CategoryName,
                    Distance = e.Category.DistanceKm,
                    EventTitle = e.Category.Event!.Title,
                    EventDate = e.Category.Event.EventDate
                })
                .ToListAsync();

            return Ok(entries);
        }
    }
}