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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.EventType,
                    e.EventDate,
                    e.Status,
                    Venue = e.Venue != null ? e.Venue.VenueName : null,
                    City = e.Venue != null ? e.Venue.City : null,
                    CategoryCount = e.Categories.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            return Ok(ev);
        }

        [HttpPost]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            int organiserId = HttpContext.Session.GetInt32("UserId")!.Value;

            var newEvent = new Event
            {
                OrganiserId = organiserId,
                VenueId = dto.VenueId,
                Title = dto.Title,
                Description = dto.Description,
                EventType = dto.EventType,
                EventDate = dto.EventDate,
                Status = "Upcoming"
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, newEvent);
        }

        [HttpPut("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            int organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
            var ev = await _context.Events.FindAsync(id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != organiserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });

            ev.VenueId = dto.VenueId;
            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.EventType = dto.EventType;
            ev.EventDate = dto.EventDate;
            ev.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        [HttpDelete("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
            var ev = await _context.Events.Include(e => e.Categories).ThenInclude(c => c.Enrolments).FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiserId) return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });

            bool hasEntries = ev.Categories.Any(c => c.Enrolments.Count > 0);
            if (hasEntries) return BadRequest(new { message = "Cannot delete event with registered participants." });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{eventId}/enrolments")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var roster = await _context.Enrolments
                .Where(en => en.Category!.EventId == eventId)
                .Select(en => new
                {
                    en.EnrolmentId,
                    en.RaceNumber,
                    ParticipantName = en.Participant!.FullName,
                    en.Participant.Email,
                    en.Category!.CategoryName,
                    en.EnrolmentDate
                })
                .ToListAsync();

            return Ok(roster);
        }
    }
}