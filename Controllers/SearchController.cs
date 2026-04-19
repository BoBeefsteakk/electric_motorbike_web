using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var model = new SearchViewModel
            {
                Query = q ?? "",
                Motorbikes = new(),
                Cars = new(),
                Accessories = new()
            };

            if (string.IsNullOrWhiteSpace(q))
                return View(model);

            var keyword = q.Trim().ToLower();

            var motorbikes = await _apiService.GetMotorbikesAsync();
            var cars = await _apiService.GetCarsAsync();
            var accessories = await _apiService.GetAccessoriesAsync();

            model.Motorbikes = motorbikes?
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.ToLower().Contains(keyword))
                .ToList() ?? new();

            model.Cars = cars?
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.ToLower().Contains(keyword))
                .ToList() ?? new();

            model.Accessories = accessories?
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.ToLower().Contains(keyword))
                .ToList() ?? new();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string? q)
        {
            var motorbikes = await _apiService.GetMotorbikesAsync();
            var cars = await _apiService.GetCarsAsync();
            var accessories = await _apiService.GetAccessoriesAsync();

            var suggestions = new List<object>();

            if (string.IsNullOrWhiteSpace(q))
            {
                suggestions.AddRange(
                    (motorbikes ?? new())
                    .Take(3)
                    .Select(m => new
                    {
                        id = m.Id,
                        name = m.Name,
                        price = m.Price,
                        imageUrl = m.ImageUrl,
                        category = "motorbike"
                    }));

                suggestions.AddRange(
                    (cars ?? new())
                    .Take(2)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        price = c.Price,
                        imageUrl = c.ImageUrl,
                        category = "car"
                    }));

                return Json(suggestions);
            }

            var keyword = q.Trim().ToLower();

            suggestions.AddRange(
                (cars ?? new())
                .Where(c => !string.IsNullOrWhiteSpace(c.Name) &&
                            c.Name.ToLower().Contains(keyword))
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    price = c.Price,
                    imageUrl = c.ImageUrl,
                    category = "car"
                }));

            suggestions.AddRange(
                (motorbikes ?? new())
                .Where(m => !string.IsNullOrWhiteSpace(m.Name) &&
                            m.Name.ToLower().Contains(keyword))
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    price = m.Price,
                    imageUrl = m.ImageUrl,
                    category = "motorbike"
                }));

            suggestions.AddRange(
                (accessories ?? new())
                .Where(a => !string.IsNullOrWhiteSpace(a.Name) &&
                            a.Name.ToLower().Contains(keyword))
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    price = a.Price,
                    imageUrl = a.ImageUrl,
                    category = "accessory"
                }));

            return Json(suggestions.Take(8).ToList());
        }
    }
}