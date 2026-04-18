using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApiService _apiService;

        public SearchController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // =========================================================
        // 1. CHỨC NĂNG CŨ: TRẢ VỀ TRANG KẾT QUẢ ĐẦY ĐỦ KHI NHẤN ENTER
        // =========================================================
        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new SearchViewModel { Query = "" });

            var lower = q.ToLower();

            var motorbikes = (await _apiService.GetMotorbikesAsync())
                .Where(m => m.Name.ToLower().Contains(lower)).ToList();

            var cars = (await _apiService.GetCarsAsync())
                .Where(c => c.Name.ToLower().Contains(lower)).ToList();

            var accessories = (await _apiService.GetAccessoriesAsync())
                .Where(a => a.Name.ToLower().Contains(lower)).ToList();

            return View(new SearchViewModel
            {
                Query = q,
                Motorbikes = motorbikes,
                Cars = cars,
                Accessories = accessories
            });
        }

        // =========================================================
        // 2. CHỨC NĂNG MỚI: API GỢI Ý KHI ĐANG GÕ PHÍM (LIVE SEARCH)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string? q)
        {
            var motorbikes = await _apiService.GetMotorbikesAsync();
            var cars = await _apiService.GetCarsAsync();
            var accessories = await _apiService.GetAccessoriesAsync();

            var suggestions = new List<object>();

            // 1. KHI CHƯA GÕ GÌ: Trả về 5 sản phẩm gợi ý mặc định
            if (string.IsNullOrWhiteSpace(q))
            {
                if (motorbikes != null)
                    suggestions.AddRange(motorbikes.Take(3).Select(m => new { id = m.Id, name = m.Name, price = m.Price, imageUrl = m.ImageUrl, category = "motorbike" }));

                if (cars != null)
                    suggestions.AddRange(cars.Take(2).Select(c => new { id = c.Id, name = c.Name, price = c.Price, imageUrl = c.ImageUrl, category = "car" }));

                return Json(suggestions);
            }

            // 2. KHI CÓ GÕ CHỮ: Tìm kiếm theo tên
            var lower = q.ToLower();

            if (cars != null)
                suggestions.AddRange(cars.Where(c => c.Name.ToLower().Contains(lower))
                    .Select(c => new { id = c.Id, name = c.Name, price = c.Price, imageUrl = c.ImageUrl, category = "car" }));

            if (motorbikes != null)
                suggestions.AddRange(motorbikes.Where(m => m.Name.ToLower().Contains(lower))
                    .Select(m => new { id = m.Id, name = m.Name, price = m.Price, imageUrl = m.ImageUrl, category = "motorbike" }));

            if (accessories != null)
                suggestions.AddRange(accessories.Where(a => a.Name.ToLower().Contains(lower))
                    .Select(a => new { id = a.Id, name = a.Name, price = a.Price, imageUrl = a.ImageUrl, category = "accessory" }));

            // Chỉ lấy 5 kết quả đẹp nhất
            return Json(suggestions.Take(5));
        }
    }
}