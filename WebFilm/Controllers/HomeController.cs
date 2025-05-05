using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebFilm.Models;

namespace WebFilm.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7028/api/films");
            var content = await response.Content.ReadAsStringAsync();
            var filmList = JsonConvert.DeserializeObject<FilmListResponse>(content);
            var films = filmList?.Items ?? new List<FilmDto>();
            return View(films);
        }
    }
}
