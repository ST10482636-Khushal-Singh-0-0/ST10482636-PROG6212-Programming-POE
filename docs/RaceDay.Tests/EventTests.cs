using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventTests
    {
        private DefaultHttpContext SetupMockSession(int userId, string role)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", userId);
            httpContext.Session.SetString("UserRole", role);
            return httpContext;
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201Created()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("CreateEventDb");
            var venue = new Venue { VenueName = "Moses Mabhida", City = "Durban", Province = "KZN", Latitude = -29.8m, Longitude = 31.0m };
            context.Venues.Add(venue);
            await context.SaveChangesAsync();

            var controller = new EventsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = SetupMockSession(1, "Organiser")
                }
            };

            var dto = new CreateEventDto
            {
                VenueId = venue.VenueId,
                Title = "Durban 10km Coastal Run",
                EventType = "Running Marathon",
                EventDate = DateTime.UtcNow.AddDays(30)
            };

            var result = await controller.CreateEvent(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdEvent = Assert.IsType<Event>(createdResult.Value);
            Assert.Equal("Durban 10km Coastal Run", createdEvent.Title);
        }
    }
}