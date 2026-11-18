using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class AccountController : BaseApiController
    {
        public AccountController(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var user = await response.Content.ReadFromJsonAsync<UserSessionDto>();
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.EffectiveId);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);
                if (!string.IsNullOrEmpty(user.Token))
                    HttpContext.Session.SetString("JwtToken", user.Token);

                return string.Equals(user.Role, "Organiser", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction("Dashboard", "Organiser")
                    : RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Unable to process login.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.PostAsJsonAsync("api/auth/register", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Registration failed: {error}");
                return View(model);
            }

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}