using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebFilm.Models;
using System.Text;
using System.Net.Http.Headers;

namespace WebFilm.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string sortBy = "Title", string sortOrder = "asc")
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            ViewBag.CurrentUserRole = currentUserRole;

            var client = _httpClientFactory.CreateClient();
            var url = $"https://localhost:7028/api/films?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortOrder={sortOrder}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var filmList = JsonConvert.DeserializeObject<FilmListResponse>(content);

            ViewBag.CurrentPage = filmList?.Page ?? 1;
            ViewBag.TotalPages = (int)Math.Ceiling((filmList?.TotalItems ?? 0) / (double)(filmList?.PageSize ?? 10));
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var films = filmList?.Items ?? new List<FilmDto>();
            return View(films);
        }

        [HttpGet]
        public IActionResult AddFilm()
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            if (currentUserRole != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFilm(FilmDto dto)
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            if (currentUserRole != "Admin")
                return Unauthorized();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/films", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Nie uda³o siê dodaæ filmu.");
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditFilm(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            if (currentUserRole != "Admin")
                return Unauthorized();

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7028/api/films/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var film = JsonConvert.DeserializeObject<FilmDto>(content);

            return View(film);
        }

        [HttpPost]
        public async Task<IActionResult> EditFilm(FilmDto dto)
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            if (currentUserRole != "Admin")
                return Unauthorized();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7028/api/films/{dto.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Nie uda³o siê zaktualizowaæ filmu.");
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            string currentUserRole = null;
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }
            if (currentUserRole != "Admin")
                return Unauthorized();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"https://localhost:7028/api/films/{id}");

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
