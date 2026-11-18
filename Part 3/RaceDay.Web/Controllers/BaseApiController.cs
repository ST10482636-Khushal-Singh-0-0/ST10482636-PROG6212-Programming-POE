using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace RaceDay.Web.Controllers
{
    public abstract class BaseApiController : Controller
    {
        protected readonly IHttpClientFactory _httpClientFactory;

        protected BaseApiController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!string.IsNullOrEmpty(role))
                client.DefaultRequestHeaders.Add("X-User-Role", role);

            if (userId.HasValue)
                client.DefaultRequestHeaders.Add("X-User-Id", userId.Value.ToString());

            return client;
        }
    }
}