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
        public async Task<ActionResult<IEnumerable<object>>> GetEvents([FromQuery] int? organiserId)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .AsQueryable();

            if (organiserId.HasValue)
            {
                query = query.Where(e => e.OrganiserId == organiserId.Value);
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            return Ok(events.Select(e => new
            {
                e.EventId,
                Id = e.EventId,
                e.OrganiserId,
                e.VenueId,
                Location = e.Venue != null ? $"{e.Venue.VenueName}, {e.Venue.City}" : "TBA",
                e.Title,
                Name = e.Title,
                e.Description,
                e.EventType,
                e.EventDate,
                Date = e.EventDate,
                e.Status,
                e.BannerImageUrl,
                Categories = e.Categories
            }));
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            return Ok(new
            {
                ev.EventId,
                Id = ev.EventId,
                ev.OrganiserId,
                ev.VenueId,
                Location = ev.Venue != null ? $"{ev.Venue.VenueName}, {ev.Venue.City}" : "TBA",
                ev.Title,
                Name = ev.Title,
                ev.Description,
                ev.EventType,
                ev.EventDate,
                Date = ev.EventDate,
                ev.Status,
                ev.BannerImageUrl,
                Categories = ev.Categories
            });
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

            var resolvedVenueId = await ResolveVenueIdAsync(dto.VenueId, dto.Location);

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                EventDate = dto.EventDate,
                VenueId = resolvedVenueId,
                EventType = dto.EventType,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Upcoming" : dto.Status,
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

            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.VenueId = updatedEvent.VenueId;
            existingEvent.EventType = updatedEvent.EventType;
            existingEvent.Status = updatedEvent.Status;

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

            existingEvent.Title = dto.Title;
            existingEvent.Description = dto.Description;
            existingEvent.EventDate = dto.EventDate;
            existingEvent.VenueId = await ResolveVenueIdAsync(dto.VenueId, dto.Location, existingEvent.VenueId);
            existingEvent.EventType = dto.EventType;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                existingEvent.Status = dto.Status;

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

        private async Task<int> ResolveVenueIdAsync(int venueId, string? location, int fallbackVenueId = 0)
        {
            if (venueId > 0) return venueId;

            if (!string.IsNullOrWhiteSpace(location))
            {
                var existing = await _context.Venues
                    .FirstOrDefaultAsync(v => v.VenueName == location || v.City == location);
                if (existing != null) return existing.VenueId;

                var newVenue = new Venue
                {
                    VenueName = location,
                    City = location,
                    Province = "Gauteng",
                    Latitude = -26.2041m,
                    Longitude = 28.0473m
                };
                _context.Venues.Add(newVenue);
                await _context.SaveChangesAsync();
                return newVenue.VenueId;
            }

            if (fallbackVenueId > 0) return fallbackVenueId;

            var firstVenue = await _context.Venues.FirstOrDefaultAsync();
            return firstVenue?.VenueId ?? 1;
        }
    }
}