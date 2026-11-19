using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using System.Net.Http.Headers;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Participant")]
    public class ParticipantController : BaseApiController
    {
        public ParticipantController(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var client = CreateAuthorizedClient();

            var response = await client.PostAsJsonAsync("api/enrolments", new
            {
                CategoryId = categoryId
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You are enrolled! Your race number has been assigned.";
                return RedirectToAction(nameof(MyEnrolments));
            }

            var errBody = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Could not complete enrolment: {errBody}";
            return RedirectToAction("EventDetails", "Home", new { id = eventId });
        }

        public async Task<IActionResult> MyEnrolments()
        {
            var client = CreateAuthorizedClient();
            List<EnrolmentViewModel> enrolments = new();
            try
            {
                enrolments = await client.GetFromJsonAsync<List<EnrolmentViewModel>>("api/enrolments/my-entries") ?? new();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to load your enrolments: {ex.Message}";
            }
            return View(enrolments);
        }

        public async Task<IActionResult> MyResults()
        {
            var client = CreateAuthorizedClient();
            List<ResultViewModel> results = new();
            try
            {
                results = await client.GetFromJsonAsync<List<ResultViewModel>>("api/results/my-history") ?? new();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to load your race results: {ex.Message}";
            }
            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var client = CreateAuthorizedClient();
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await client.GetFromJsonAsync<ProfileViewModel>($"api/users/{userId}") ?? new ProfileViewModel
            {
                Id = userId ?? 0,
                FullName = HttpContext.Session.GetString("UserName") ?? "",
                Email = HttpContext.Session.GetString("UserEmail") ?? ""
            };
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateAuthorizedClient();
            var userId = HttpContext.Session.GetInt32("UserId");

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.FullName), "fullName" },
                { new StringContent(model.Email), "email" }
            };

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var streamContent = new StreamContent(model.ProfilePicture.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfilePicture.ContentType);
                content.Add(streamContent, "profilePicture", model.ProfilePicture.FileName);
            }

            var response = await client.PutAsync($"api/users/{userId}/profile", content);
            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("UserName", model.FullName);
                HttpContext.Session.SetString("UserEmail", model.Email);
                TempData["Success"] = "Profile updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update profile.";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}