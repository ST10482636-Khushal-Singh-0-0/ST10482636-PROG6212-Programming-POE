using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class HomeController : BaseApiController
    {
        public HomeController(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<IActionResult> Index(string? search, string? eventType, string? location)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            List<EventViewModel> events = new();
            try
            {
                events = await client.GetFromJsonAsync<List<EventViewModel>>("api/events") ?? new();
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = $"Could not connect to RaceDay API ({client.BaseAddress}api/events): {ex.Message}";
            }

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(eventType))
                events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(location))
                events = events.Where(e => e.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Search = search;
            ViewBag.EventType = eventType;
            ViewBag.Location = location;
            return View(events);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var ev = await client.GetFromJsonAsync<EventViewModel>($"api/events/{id}");
            if (ev == null) return NotFound();

            return View(ev);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}