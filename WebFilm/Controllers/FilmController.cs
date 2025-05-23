﻿using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Details(
            int id,
            int reviewPage = 1,
            int reviewPageSize = 5,
            string reviewSortBy = "date",
            string reviewSortOrder = "desc")
        {
            var client = _httpClientFactory.CreateClient();

            var filmResponse = await client.GetAsync($"https://localhost:7028/api/films/{id}");
            if (!filmResponse.IsSuccessStatusCode)
                return NotFound();

            var filmContent = await filmResponse.Content.ReadAsStringAsync();
            var film = JsonConvert.DeserializeObject<FilmDto>(filmContent);

            var reviewsUrl = $"https://localhost:7028/api/reviews?filmId={id}&page={reviewPage}&pageSize={reviewPageSize}&sortBy={reviewSortBy}&sortOrder={reviewSortOrder}";
            var reviewsResponse = await client.GetAsync(reviewsUrl);
            var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
            var reviewsList = JsonConvert.DeserializeObject<ReviewListResponse>(reviewsContent);

            var userIds = reviewsList?.Items.Select(r => r.UserId).Distinct().ToList() ?? new List<int>();
            List<UserDto> users = new();
            if (userIds.Any())
            {
                var usersResponse = await client.GetAsync($"https://localhost:7028/api/users?ids={string.Join(",", userIds)}");
                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersContent = await usersResponse.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<UserDto>>(usersContent);
                }
            }
            var userDict = users.ToDictionary(u => u.Id, u => u.Username);

            foreach (var review in reviewsList.Items)
            {
                review.AuthorUsername = userDict.ContainsKey(review.UserId) ? userDict[review.UserId] : "Nieznany użytkownik";
            }

            ViewBag.ReviewCurrentPage = reviewsList?.Page ?? 1;
            ViewBag.ReviewTotalPages = (int)Math.Ceiling((reviewsList?.TotalItems ?? 0) / (double)(reviewsList?.PageSize ?? 5));
            ViewBag.ReviewSortBy = reviewSortBy;
            ViewBag.ReviewSortOrder = reviewSortOrder;
            ViewBag.ReviewAverageRating = reviewsList?.AverageRating ?? 0;
            ViewBag.ReviewTotalCount = reviewsList?.TotalItems ?? 0;

            var model = new FilmDetailsViewModel
            {
                Film = film,
                Reviews = reviewsList?.Items ?? new List<ReviewDto>()
            };

            var token = HttpContext.Session.GetString("JWToken");
            int? currentUserId = null;
            string currentUserRole = null;

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewDto dto)
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
        [ValidateAntiForgeryToken]
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

            return Content("<script>history.go(-2);</script>", "text/html");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id, int filmId)
        {
            if (HttpContext.Session.GetString("JWToken") == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"https://localhost:7028/api/reviews/{id}");

            return Content("<script>history.go(-2);</script>", "text/html");
        }
    }
}
