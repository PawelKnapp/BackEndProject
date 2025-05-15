using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebFilm.Models;

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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Nieprawidłowa nazwa użytkownika lub hasło.";
                return View(dto);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
            string token = tokenObj?.token ?? tokenObj?.Token;

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Błąd logowania: brak tokena w odpowiedzi.";
                return View(dto);
            }

            HttpContext.Session.SetString("JWToken", token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var usernameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (usernameClaim != null)
                HttpContext.Session.SetString("Username", usernameClaim.Value);

            var roleClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type == "role" ||
                c.Type == ClaimTypes.Role ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim != null)
                HttpContext.Session.SetString("UserRole", roleClaim.Value);

            return RedirectToAction("Index", "Home");
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
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                return View(model);
            }

            var loginDto = new LoginDto { Username = model.Username, Password = model.Password };
            var loginJson = JsonConvert.SerializeObject(loginDto);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("https://localhost:7028/api/auth/login", loginContent);

            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                var tokenObj = JsonConvert.DeserializeObject<dynamic>(loginResponseContent);
                string token = tokenObj?.token ?? tokenObj?.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("JWToken", token);

                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var usernameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    if (usernameClaim != null)
                        HttpContext.Session.SetString("Username", usernameClaim.Value);

                    var roleClaim = jwt.Claims.FirstOrDefault(c =>
                        c.Type == "role" ||
                        c.Type == ClaimTypes.Role ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                    if (roleClaim != null)
                        HttpContext.Session.SetString("UserRole", roleClaim.Value);

                    TempData["Success"] = "Rejestracja zakończona sukcesem. Zostałeś automatycznie zalogowany.";
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["Success"] = "Rejestracja zakończona sukcesem. Zaloguj się.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            var response = await client.GetAsync($"https://localhost:7028/api/users/{userId}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            var userContent = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<AccountSettingsViewModel>(userContent);

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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new { NewEmail, CurrentPassword };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-email", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                HttpContext.Session.Remove("UserRole");
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new { NewUsername, CurrentPassword };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-username", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<dynamic>(result);
                var newUsername = (string)obj?.Username;
                if (!string.IsNullOrEmpty(newUsername))
                    HttpContext.Session.SetString("Username", newUsername);

                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                HttpContext.Session.Remove("UserRole");
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new { CurrentPassword, NewPassword };
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7028/api/auth/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Remove("Username");
                HttpContext.Session.Remove("UserRole");
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
