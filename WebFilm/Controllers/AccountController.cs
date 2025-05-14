using Microsoft.AspNetCore.Mvc;
using WebFilm.Models;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            var response = await client.GetAsync($"https://localhost:7028/api/users/{userId}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            var userContent = await response.Content.ReadAsStringAsync();
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountSettingsViewModel>(userContent);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string NewEmail, string CurrentPassword)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login");

            if (!string.IsNullOrWhiteSpace(NewEmail))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(NewEmail);
                }
                catch
                {
                    TempData["ErrorChangeEmail"] = "Podaj poprawny adres e-mail zgodny ze strukturą adresu email.";
                    return RedirectToAction("Settings");
                }
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dto = new { NewEmail, CurrentPassword };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-email", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorChangeEmail"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Settings");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUsername(string NewUsername, string CurrentPassword)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dto = new { NewUsername, CurrentPassword };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-username", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var obj = Newtonsoft.Json.Linq.JObject.Parse(result);
                var newUsername = (string)obj["Username"];
                if (!string.IsNullOrEmpty(newUsername))
                    HttpContext.Session.SetString("Username", newUsername);

                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorChangeUsername"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Settings");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dto = new { CurrentPassword, NewPassword };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorChangePassword"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction("Settings");
            }
        }
    }
}
