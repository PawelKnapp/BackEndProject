using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WebFilm.Models;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Panel()
    {
        var token = HttpContext.Session.GetString("JWToken");
        var role = HttpContext.Session.GetString("UserRole");
        if (token == null || role != "Admin")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://localhost:7028/api/users");
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<List<UserViewModel>>(content);

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var token = HttpContext.Session.GetString("JWToken");
        var role = HttpContext.Session.GetString("UserRole");
        if (token == null || role != "Admin")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync($"https://localhost:7028/api/users/{id}");
        return RedirectToAction("Panel");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(int id, string newRole)
    {
        var token = HttpContext.Session.GetString("JWToken");
        var role = HttpContext.Session.GetString("UserRole");
        if (token == null || role != "Admin")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = new { NewRole = newRole };
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://localhost:7028/api/users/{id}/role", content);

        return RedirectToAction("Panel");
    }
}
