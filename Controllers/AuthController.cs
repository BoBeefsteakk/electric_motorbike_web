using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;

        public AuthController(ApiService api)
        {
            _api = api;
        }

        // GET /Auth/Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // POST /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Account) || string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
                return View(vm);
            }

            var (success, token, message) = await _api.LoginAsync(vm.Account, vm.Password);

            if (!success)
            {
                vm.ErrorMessage = message ?? "Đăng nhập thất bại";
                return View(vm);
            }

            HttpContext.Session.SetString("jwt_token", token ?? "");
            HttpContext.Session.SetString("account", vm.Account);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, vm.Account ?? ""),
                new Claim("jwt", token ?? "")
            };

            string? role = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    role = jwt.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Role ||
                        c.Type == "role" ||
                        c.Type == "roles" ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    )?.Value;
                }
                catch
                {
                    // Token không đọc được thì bỏ qua
                }
            }

            // fallback: account admin thì gán role admin
            if (string.IsNullOrWhiteSpace(role) &&
                string.Equals(vm.Account, "admin", StringComparison.OrdinalIgnoreCase))
            {
                role = "admin";
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // GET /Auth/Register
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Account) || string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
                return View(vm);
            }

            if (vm.Password != vm.ConfirmPassword)
            {
                vm.ErrorMessage = "Mật khẩu xác nhận không khớp";
                return View(vm);
            }

            var (success, message) = await _api.RegisterAsync(vm.Account, vm.Password);

            if (!success)
            {
                vm.ErrorMessage = message ?? "Đăng ký thất bại";
                return View(vm);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET /Auth/Forgot
        public IActionResult Forgot()
        {
            return View();
        }

        // POST /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}