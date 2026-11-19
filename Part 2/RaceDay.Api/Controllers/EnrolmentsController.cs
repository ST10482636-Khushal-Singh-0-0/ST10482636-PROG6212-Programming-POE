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

        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue) return sessionUserId.Value;

            if (Request.Headers.TryGetValue("X-User-Id", out var idHeader) &&
                int.TryParse(idHeader, out int parsedId))
            {
                return parsedId;
            }

            throw new UnauthorizedAccessException("User ID not found in session or headers.");
        }

        [HttpPost]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            int participantId = GetCurrentUserId();

            var category = await _context.EventCategories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null) return NotFound(new { message = "Category not found." });

            bool alreadyEnrolled = category.Enrolments.Any(e => e.ParticipantId == participantId);
            if (alreadyEnrolled) return Conflict(new { message = "You are already enrolled in this category." });

            if (category.Enrolments.Count >= category.MaxParticipants)
                return BadRequest(new { message = "This category has reached maximum participant capacity." });

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
            int participantId = GetCurrentUserId();

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

        // GET: api/enrolments/event/5 (Organiser view of enrolled athletes)
        [HttpGet("event/{eventId}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.Category != null && e.Category.EventId == eventId)
                .OrderBy(e => e.RaceNumber)
                .Select(e => new
                {
                    e.EnrolmentId,
                    Id = e.EnrolmentId,
                    e.RaceNumber,
                    e.PaymentStatus,
                    e.EnrolmentDate,
                    ParticipantName = e.Participant != null ? e.Participant.FullName : "Athlete",
                    ParticipantEmail = e.Participant != null ? e.Participant.Email : "",
                    CategoryName = e.Category != null ? e.Category.CategoryName : "General"
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}