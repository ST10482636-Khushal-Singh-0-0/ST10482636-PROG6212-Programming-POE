using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentTests
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
        public async Task Enrol_Participant_AssignsValidRaceNumber()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("EnrolmentDb");
            var category = new EventCategory
            {
                CategoryId = 1,
                CategoryName = "21.1km Half Marathon",
                DistanceKm = 21.1m,
                EntryFee = 250,
                MaxParticipants = 100
            };
            context.EventCategories.Add(category);
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = SetupMockSession(5, "Participant")
                }
            };

            var result = await controller.Enrol(new CreateEnrolmentDto { CategoryId = 1 });

            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var enrolment = Assert.IsType<Enrolment>(createdResult.Value);
            Assert.True(enrolment.RaceNumber > 0);
        }

        [Fact]
        public async Task Enrol_ParticipantAlreadyEnrolled_Returns409Conflict()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("EnrolmentConflictDb");
            var category = new EventCategory
            {
                CategoryId = 2,
                CategoryName = "10km Classic",
                DistanceKm = 10,
                EntryFee = 150,
                MaxParticipants = 50
            };
            context.EventCategories.Add(category);
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = SetupMockSession(10, "Participant")
                }
            };

            await controller.Enrol(new CreateEnrolmentDto { CategoryId = 2 });
            var duplicateResult = await controller.Enrol(new CreateEnrolmentDto { CategoryId = 2 });

            Assert.IsType<ConflictObjectResult>(duplicateResult);
        }
    }
}