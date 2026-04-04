using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApiService _api;
        public ProductsController(ApiService api) => _api = api;

        // GET /Products hoặc /Products?category=pho_thong
        public async Task<IActionResult> Index(string? category)
        {
            var all = await _api.GetMotorbikesAsync();
            var filtered = string.IsNullOrEmpty(category)
                ? all
                : all.Where(m => m.Category == category).ToList();

            var label = category switch
            {
                "pho_thong" => "Xe Phổ Thông",
                "trung_cap" => "Xe Trung Cấp",
                "cao_cap" => "Xe Cao Cấp",
                _ => "Tất Cả Xe Máy"
            };

            return View(new ProductsViewModel
            {
                Motorbikes = filtered,
                Category = category,
                CategoryLabel = label
            });
        }

        // GET /Products/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _api.GetMotorbikeAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}