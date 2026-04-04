using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;
        public AuthController(ApiService api) => _api = api;

        // GET /Auth/Login
        public IActionResult Login() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View(new LoginViewModel());

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

            // Lưu token vào session
            HttpContext.Session.SetString("jwt_token", token ?? "");
            HttpContext.Session.SetString("account", vm.Account);

            // Cookie auth
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, vm.Account),
                new("jwt", token ?? "")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        // GET /Auth/Register
        public IActionResult Register() => View(new RegisterViewModel());

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
        public IActionResult Forgot() => View();

        // POST /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}