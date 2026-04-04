using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var motorbikes = await _api.GetMotorbikesAsync();
            var stores = await _api.GetStoresAsync();
            var vouchers = await _api.GetVouchersAsync();

            var vm = new HomeViewModel
            {
                FeaturedMotorbikes = motorbikes.Take(6).ToList(),
                Stores = stores,
                Vouchers = vouchers
            };
            return View(vm);
        }

        public IActionResult Error() => View();
    }
}