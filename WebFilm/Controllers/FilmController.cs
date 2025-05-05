using Microsoft.AspNetCore.Mvc;
using WebFilm.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

namespace WebFilm.Controllers
{
    public class FilmController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FilmController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var filmResponse = await client.GetAsync($"https://localhost:7028/api/films/{id}");
            if (!filmResponse.IsSuccessStatusCode)
                return NotFound();

            var filmContent = await filmResponse.Content.ReadAsStringAsync();
            var film = JsonConvert.DeserializeObject<FilmDto>(filmContent);

            var reviewsResponse = await client.GetAsync($"https://localhost:7028/api/reviews?filmId={id}");
            var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
            var reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(reviewsContent) ?? new List<ReviewDto>();

            var model = new FilmDetailsViewModel
            {
                Film = film,
                Reviews = reviews
            };

            var token = HttpContext.Session.GetString("JWToken");
            int? currentUserId = null;
            string currentUserRole = null;

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var idClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                    currentUserId = parsedId;

                var roleClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                if (roleClaim != null)
                    currentUserRole = roleClaim.Value;
            }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserRole = currentUserRole;

            return View(model);
        }

        [HttpGet]
        public IActionResult AddReview(int filmId)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            var model = new AddReviewDto { FilmId = filmId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(AddReviewDto dto)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(dto);

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/reviews", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Nie udało się dodać recenzji.");
                return View(dto);
            }

            return RedirectToAction("Details", new { id = dto.FilmId });
        }

        [HttpGet]
        public async Task<IActionResult> EditReview(int id, int filmId)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7028/api/reviews/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Details", new { id = filmId });

            var content = await response.Content.ReadAsStringAsync();
            var review = JsonConvert.DeserializeObject<ReviewDto>(content);

            var model = new EditReviewDto
            {
                Rating = review.Rating,
                Content = review.Content,
                FilmId = review.FilmId,
                ReviewId = review.Id
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditReview(EditReviewDto dto)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(dto);

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7028/api/reviews/{dto.ReviewId}", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Nie udało się zaktualizować recenzji.");
                return View(dto);
            }

            return RedirectToAction("Details", new { id = dto.FilmId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id, int filmId)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"https://localhost:7028/api/reviews/{id}");

            return RedirectToAction("Details", new { id = filmId });
        }
    }
}
