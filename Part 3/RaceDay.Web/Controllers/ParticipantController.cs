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
            var participantId = HttpContext.Session.GetInt32("UserId");

            var response = await client.PostAsJsonAsync("api/enrolments", new
            {
                EventId = eventId,
                CategoryId = categoryId,
                ParticipantId = participantId
            });

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "You are enrolled!" : "Could not complete enrolment.";

            return RedirectToAction(nameof(MyEnrolments));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            var client = CreateAuthorizedClient();
            var participantId = HttpContext.Session.GetInt32("UserId");
            var enrolments = await client.GetFromJsonAsync<List<EnrolmentViewModel>>($"api/enrolments/user/{participantId}") ?? new();
            return View(enrolments);
        }

        public async Task<IActionResult> MyResults()
        {
            var client = CreateAuthorizedClient();
            var participantId = HttpContext.Session.GetInt32("UserId");
            var results = await client.GetFromJsonAsync<List<ResultViewModel>>($"api/results/user/{participantId}") ?? new();
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