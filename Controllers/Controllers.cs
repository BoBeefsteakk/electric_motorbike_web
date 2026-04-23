using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Models;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    // ── CARS ──────────────────────────────────────────────────────────────────
 

    // ── STORES ────────────────────────────────────────────────────────────────
    public class StoresController : Controller
    {
        private readonly ApiService _api;
        public StoresController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var stores = await _api.GetStoresAsync();
            return View(stores);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var store = await _api.GetStoreAsync(id);
            if (store == null) return NotFound();
            return View(store);
        }

        // JSON endpoint cho Store Detail page dùng JS fetch xe
        [Microsoft.AspNetCore.Mvc.HttpGet("/api/products-json")]
        public async Task<IActionResult> ProductsJson()
        {
            var items = await _api.GetMotorbikesAsync();
            return Json(items);
        }
    }

    // ── VOUCHERS ──────────────────────────────────────────────────────────────
    public class VouchersController : Controller
    {
        private readonly ApiService _api;
        public VouchersController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var vouchers = await _api.GetVouchersAsync();
            return View(vouchers);
        }
    }

   

    // ── PROFILE ───────────────────────────────────────────────────────────────
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Auth");
            return View();
        }
    }
}