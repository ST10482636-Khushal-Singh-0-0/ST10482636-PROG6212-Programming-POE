using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [AuthorizeRole] // Any logged-in user
        public async Task<IActionResult> GetProfile()
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.CreatedAt
            });
        }

        [HttpPut("profile")]
        [AuthorizeRole]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully.", user.FullName, user.PhoneNumber });
        }
    }
}