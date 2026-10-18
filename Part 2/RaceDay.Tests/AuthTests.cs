using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task Register_ValidParticipant_Returns201Created()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("RegisterSuccessDb");
            var controller = new AuthController(context);

            var registerDto = new RegisterDto
            {
                FullName = "Kagiso Mokoena",
                Email = "kagiso.test@example.com",
                Password = "SecurePassword123!",
                Role = "Participant"
            };

            var result = await controller.Register(registerDto);

            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("RegisterConflictDb");
            var controller = new AuthController(context);

            var dto = new RegisterDto
            {
                FullName = "Sarah Runner",
                Email = "sarah@example.com",
                Password = "Password123!",
                Role = "Participant"
            };

            await controller.Register(dto);
            var duplicateResult = await controller.Register(dto);

            Assert.IsType<ConflictObjectResult>(duplicateResult);
        }

        [Fact]
        public async Task Login_InvalidPassword_Returns401Unauthorized()
        {
            using var context = TestDatabaseFixture.CreateInMemoryDbContext("LoginFailDb");
            var controller = new AuthController(context);

            await controller.Register(new RegisterDto
            {
                FullName = "Organiser John",
                Email = "john@raceday.co.za",
                Password = "CorrectPassword123!",
                Role = "Organiser"
            });

            var result = await controller.Login(new LoginDto
            {
                Email = "john@raceday.co.za",
                Password = "WrongPassword!"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}