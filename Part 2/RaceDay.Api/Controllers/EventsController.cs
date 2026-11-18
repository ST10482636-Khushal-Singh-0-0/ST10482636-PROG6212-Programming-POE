using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;
        private readonly IConfiguration _configuration;

        public EventsController(
            RaceDayDbContext context,
            IBlobStorageService blobService,
            IConfiguration configuration)
        {
            _context = context;
            _blobService = blobService;
            _configuration = configuration;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents([FromQuery] int? organiserId)
        {
            var query = _context.Events.AsQueryable();

            if (organiserId.HasValue)
            {
                query = query.Where(e => e.OrganiserId == organiserId.Value);
            }

            return await query.OrderBy(e => e.Date).ToListAsync();
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            return ev;
        }

        // POST: api/events (Standard JSON endpoint from Part 2)
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        // POST: api/events/with-banner (Part 3 Multipart Form + Azure Blob Upload)
        [HttpPost("with-banner")]
        public async Task<ActionResult<Event>> CreateEventWithBanner([FromForm] EventFormDto dto, IFormFile? bannerImage)
        {
            string? bannerUrl = null;

            if (bannerImage != null && bannerImage.Length > 0)
            {
                var container = _configuration["BlobContainers:EventBanners"] ?? "event-banners";
                bannerUrl = await _blobService.UploadFileAsync(bannerImage, container);
            }

            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType,
                OrganiserId = dto.OrganiserId,
                BannerImageUrl = bannerUrl
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        // PUT: api/events/5 (Standard JSON endpoint from Part 2)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.Date = updatedEvent.Date;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.Distance = updatedEvent.Distance;
            existingEvent.EventType = updatedEvent.EventType;

            await _context.SaveChangesAsync();
            return Ok(existingEvent);
        }

        // PUT: api/events/5/with-banner (Part 3 Multipart Form + Azure Blob Replacement)
        [HttpPut("{id}/with-banner")]
        public async Task<IActionResult> UpdateEventWithBanner(int id, [FromForm] EventFormDto dto, IFormFile? bannerImage)
        {
            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            if (bannerImage != null && bannerImage.Length > 0)
            {
                var container = _configuration["BlobContainers:EventBanners"] ?? "event-banners";
                await _blobService.DeleteFileAsync(existingEvent.BannerImageUrl, container);
                existingEvent.BannerImageUrl = await _blobService.UploadFileAsync(bannerImage, container);
            }

            existingEvent.Name = dto.Name;
            existingEvent.Description = dto.Description;
            existingEvent.Date = dto.Date;
            existingEvent.Location = dto.Location;
            existingEvent.Distance = dto.Distance;
            existingEvent.EventType = dto.EventType;

            await _context.SaveChangesAsync();
            return Ok(existingEvent);
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (!string.IsNullOrEmpty(ev.BannerImageUrl))
            {
                var container = _configuration["BlobContainers:EventBanners"] ?? "event-banners";
                await _blobService.DeleteFileAsync(ev.BannerImageUrl, container);
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}