using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Models;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    // ── CARS ──────────────────────────────────────────────────────────────────
    public class CarsController : Controller
    {
        private readonly ApiService _api;
        public CarsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? category)
        {
            var all = await _api.GetCarsAsync();
            var filtered = string.IsNullOrEmpty(category)
                ? all
                : all.Where(c => c.Category == category).ToList();

            var label = category switch
            {
                "dong_co_dien" => "Xe Điện",
                "dong_co_xang" => "Xe Xăng",
                "dong_xe_dich_vu" => "Xe Dịch Vụ",
                _ => "Tất Cả Ô Tô"
            };

            return View(new CarsViewModel { Cars = filtered, Category = category, CategoryLabel = label });
        }
    }

    // ── ACCESSORIES ───────────────────────────────────────────────────────────
    public class AccessoriesController : Controller
    {
        private readonly ApiService _api;
        public AccessoriesController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var items = await _api.GetAccessoriesAsync();
            return View(items);
        }
    }

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

    // ── CART ──────────────────────────────────────────────────────────────────
    public class CartController : Controller
    {
        private readonly CartService _cart;
        public CartController(CartService cart) => _cart = cart;

        public IActionResult Index()
        {
            var vm = new CartViewModel { Items = _cart.GetCart() };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Add([FromBody] CartItem item)
        {
            _cart.AddItem(item);
            return Json(new { success = true, count = _cart.Count });
        }

        [HttpPost]
        public IActionResult Remove([FromBody] RemoveItemRequest req)
        {
            _cart.RemoveItem(req.ProductId, req.ProductType);
            return Json(new { success = true, count = _cart.Count });
        }

        [HttpPost]
        public IActionResult UpdateQty([FromBody] UpdateQtyRequest req)
        {
            _cart.UpdateQuantity(req.ProductId, req.ProductType, req.Qty);
            var cart = _cart.GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == req.ProductId && c.ProductType == req.ProductType);
            var grandTotal = cart.Sum(c => c.Total);
            return Json(new
            {
                success = true,
                count = _cart.Count,
                itemTotal = item?.FormattedTotal ?? "0đ",
                grandTotal = grandTotal.ToString("N0").Replace(",", ".") + "đ"
            });
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cart.ClearCart();
            return Json(new { success = true });
        }

        public IActionResult Count() => Json(new { count = _cart.Count });
    }

    public record RemoveItemRequest(int ProductId, string ProductType);
    public record UpdateQtyRequest(int ProductId, string ProductType, int Qty);

    // ── SEARCH ────────────────────────────────────────────────────────────────
    public class SearchController : Controller
    {
        private readonly ApiService _api;
        public SearchController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new SearchViewModel { Query = "" });

            var lower = q.ToLower();
            var motorbikes = (await _api.GetMotorbikesAsync())
                .Where(m => m.Name.ToLower().Contains(lower)).ToList();
            var cars = (await _api.GetCarsAsync())
                .Where(c => c.Name.ToLower().Contains(lower)).ToList();
            var accessories = (await _api.GetAccessoriesAsync())
                .Where(a => a.Name.ToLower().Contains(lower)).ToList();

            return View(new SearchViewModel
            {
                Query = q,
                Motorbikes = motorbikes,
                Cars = cars,
                Accessories = accessories
            });
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