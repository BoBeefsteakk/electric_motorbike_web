using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApiService _api;

        public CarsController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var all = await _api.GetCarsAsync();

            if (all == null)
                all = new List<VinfastWeb.Models.Car>();

            var filtered = string.IsNullOrWhiteSpace(category)
                ? all
                : all.Where(x => x.Category == category).ToList();

            var label = category switch
            {
                "dong_co_dien" => "Xe Điện",
                "dong_co_xang" => "Xe Xăng",
                "dong_xe_dich_vu" => "Xe Dịch Vụ",
                _ => "Tất Cả Ô Tô"
            };

            return View(new CarsViewModel
            {
                Cars = filtered,
                Category = category,
                CategoryLabel = label
            });
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var all = await _api.GetCarsAsync();

            if (all == null)
                return NotFound();

            var item = all.FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }
    }
}