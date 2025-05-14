using Microsoft.AspNetCore.Mvc;
using WebFilm.Models;
using Newtonsoft.Json;
using System.Linq;

public class ReviewController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReviewController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> MojeRecenzje(int? filmId = null, int page = 1, int pageSize = 5)
    {
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var reviewsResponse = await client.GetAsync("https://localhost:7028/api/reviews/user");
        var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
        var reviewsList = JsonConvert.DeserializeObject<ReviewListResponse>(reviewsContent) ?? new ReviewListResponse();

        var filmIds = reviewsList.Items.Select(r => r.FilmId).Distinct().ToList();

        List<FilmDto> films = new();
        if (filmIds.Any())
        {
            var filmsResponse = await client.GetAsync($"https://localhost:7028/api/films?ids={string.Join(",", filmIds)}");
            var filmsContent = await filmsResponse.Content.ReadAsStringAsync();
            var allFilms = JsonConvert.DeserializeObject<FilmListResponse>(filmsContent) ?? new FilmListResponse();
            films = allFilms.Items.Where(f => filmIds.Contains(f.Id)).ToList();
        }

        ViewBag.Films = films;

        var filteredReviews = filmId.HasValue
            ? reviewsList.Items.Where(r => r.FilmId == filmId.Value).ToList()
            : reviewsList.Items;

        int totalItems = filteredReviews.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var pagedReviews = filteredReviews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.SelectedFilmId = filmId;

        return View(pagedReviews);
    }
}
