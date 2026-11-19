using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using System.Net.Http.Headers;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : BaseApiController
    {
        public OrganiserController(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<IActionResult> Dashboard()
        {
            var client = CreateAuthorizedClient();
            var organiserId = HttpContext.Session.GetInt32("UserId");
            List<EventViewModel> myEvents = new();

            try
            {
                var allEvents = await client.GetFromJsonAsync<List<EventViewModel>>("api/events") ?? new();
                myEvents = allEvents.Where(e => e.OrganiserId == organiserId).ToList();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to load dashboard events: {ex.Message}";
            }

            return View(myEvents);
        }

        [HttpGet]
        public IActionResult CreateEvent() => View(new EventViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateAuthorizedClient();
            using var formData = BuildEventMultipartContent(model);

            var response = await client.PostAsync("api/events/with-banner", formData);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create event.");
                return View(model);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var client = CreateAuthorizedClient();
            var ev = await client.GetFromJsonAsync<EventViewModel>($"api/events/{id}");
            if (ev == null) return NotFound();
            ev.Id = ev.EffectiveId;
            return View(ev);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(EventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateAuthorizedClient();
            using var formData = BuildEventMultipartContent(model);

            var response = await client.PutAsync($"api/events/{model.Id}/with-banner", formData);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update event.");
                return View(model);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var client = CreateAuthorizedClient();
            await client.DeleteAsync($"api/events/{id}");
            TempData["Success"] = "Event deleted.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId)
        {
            var client = CreateAuthorizedClient();
            var categories = await client.GetFromJsonAsync<List<CategoryViewModel>>($"api/categories/event/{eventId}") ?? new();
            ViewBag.EventId = eventId;
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            var client = CreateAuthorizedClient();
            await client.PostAsJsonAsync("api/categories", model);
            TempData["Success"] = "Category added!";
            return RedirectToAction(nameof(ManageCategories), new { eventId = model.EventId });
        }

        [HttpGet]
        public async Task<IActionResult> EventEnrolments(int eventId)
        {
            var client = CreateAuthorizedClient();
            var enrolments = await client.GetFromJsonAsync<List<EnrolmentViewModel>>($"api/enrolments/event/{eventId}") ?? new();
            ViewBag.EventId = eventId;
            return View(enrolments);
        }

        [HttpGet]
        public async Task<IActionResult> CaptureResults(int eventId)
        {
            var client = CreateAuthorizedClient();
            var enrolments = await client.GetFromJsonAsync<List<EnrolmentViewModel>>($"api/enrolments/event/{eventId}") ?? new();
            ViewBag.EventId = eventId;
            return View(enrolments);
        }

        [HttpPost]
        public async Task<IActionResult> PublishResult(int eventId, int enrolmentId, string finishTime, int position)
        {
            var client = CreateAuthorizedClient();
            var payload = new { EnrolmentId = enrolmentId, FinishTime = finishTime, Position = position };
            var response = await client.PostAsJsonAsync("api/results", payload);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "Result published!" : "Failed to publish result.";

            return RedirectToAction(nameof(CaptureResults), new { eventId });
        }

        private MultipartFormDataContent BuildEventMultipartContent(EventViewModel model)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var content = new MultipartFormDataContent
            {
                { new StringContent(model.Name), "Title" },
                { new StringContent(model.Description), "Description" },
                { new StringContent(model.Date.ToString("o")), "EventDate" },
                { new StringContent(model.Location), "Location" },
                { new StringContent(model.EventType), "EventType" },
                { new StringContent("Upcoming"), "Status" },
                { new StringContent(organiserId.ToString()), "OrganiserId" }
            };

            if (model.BannerImage != null && model.BannerImage.Length > 0)
            {
                var streamContent = new StreamContent(model.BannerImage.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.BannerImage.ContentType);
                content.Add(streamContent, "bannerImage", model.BannerImage.FileName);
            }

            return content;
        }
    }
}