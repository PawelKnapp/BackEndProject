using Microsoft.AspNetCore.Mvc;
using WebFilm.Models;
using Newtonsoft.Json;
using System.Text;

namespace WebFilm.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();

            var dto = new RegisterDto
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            };

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var loginDto = new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password
                };
                var loginJson = JsonConvert.SerializeObject(loginDto);
                var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
                var loginResponse = await client.PostAsync("https://localhost:7028/api/auth/login", loginContent);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var loginResult = await loginResponse.Content.ReadAsStringAsync();
                    var tokenObj = Newtonsoft.Json.Linq.JObject.Parse(loginResult);
                    var token = tokenObj["Token"]?.ToString() ?? tokenObj["token"]?.ToString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        HttpContext.Session.SetString("Username", model.Username);
                        HttpContext.Session.SetString("JWToken", token);
                        TempData["Success"] = "Rejestracja zakończona sukcesem. Zostałeś automatycznie zalogowany.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var loginError = loginResult;
                        TempData["Error"] = "Rejestracja zakończona sukcesem, ale nie udało się automatycznie zalogować: " + loginError;
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    var loginError = await loginResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = "Rejestracja zakończona sukcesem, ale nie udało się automatycznie zalogować: " + loginError;
                    return RedirectToAction("Login");
                }
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Nie udało się zarejestrować: " + errorMsg;
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło.");
                return View(dto);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(responseContent);

            HttpContext.Session.SetString("JWToken", tokenResponse.Token);
            HttpContext.Session.SetString("Username", dto.Username);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Index", "Home");
        }
    }
}
