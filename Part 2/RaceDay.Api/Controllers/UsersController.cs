using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;
        private readonly IConfiguration _configuration;

        public UsersController(
            RaceDayDbContext context,
            IBlobStorageService blobService,
            IConfiguration configuration)
        {
            _context = context;
            _blobService = blobService;
            _configuration = configuration;
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new
            {
                Id = user.UserId,
                user.UserId,
                user.FullName,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }

        // PUT: api/users/5/profile
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(
            int id,
            [FromForm] string fullName,
            [FromForm] string email,
            IFormFile? profilePicture)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Prevent duplicate email conflicts with other accounts
            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == email && u.UserId != id);
            if (emailInUse)
                return BadRequest("Email address is already in use by another account.");

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var container = _configuration["BlobContainers:ProfilePictures"] ?? "profile-pictures";
                await _blobService.DeleteFileAsync(user.ProfilePictureUrl, container);
                user.ProfilePictureUrl = await _blobService.UploadFileAsync(profilePicture, container);
            }

            user.FullName = fullName;
            user.Email = email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = user.UserId,
                user.UserId,
                user.FullName,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }
    }
}