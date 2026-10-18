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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> CaptureResult([FromBody] CreateResultDto dto)
        {
            var enrolment = await _context.Enrolments.FindAsync(dto.EnrolmentId);
            if (enrolment == null) return NotFound(new { message = "Enrolment ID not found." });

            bool resultExists = await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);
            if (resultExists) return Conflict(new { message = "Result already captured for this enrolment." });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSeconds = dto.FinishTimeSeconds,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                Status = dto.Status
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .OrderBy(r => r.OverallPosition)
                .Select(r => new
                {
                    r.OverallPosition,
                    r.CategoryPosition,
                    ParticipantName = r.Enrolment!.Participant!.FullName,
                    r.Enrolment.RaceNumber,
                    Category = r.Enrolment.Category!.CategoryName,
                    r.FinishTimeSeconds,
                    r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("my-history")]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> GetMyHistory()
        {
            int participantId = HttpContext.Session.GetInt32("UserId")!.Value;

            var history = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.ParticipantId == participantId)
                .Select(r => new
                {
                    EventTitle = r.Enrolment!.Category!.Event!.Title,
                    EventDate = r.Enrolment.Category.Event.EventDate,
                    Category = r.Enrolment.Category.CategoryName,
                    r.Enrolment.RaceNumber,
                    r.FinishTimeSeconds,
                    r.OverallPosition,
                    r.CategoryPosition,
                    r.Status
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}