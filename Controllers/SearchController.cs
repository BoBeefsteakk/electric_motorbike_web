using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Models;
using VinfastWeb.Services;

namespace VinfastWeb.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApiService _api;

        public SearchController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string q)
        {
            var all = await GetAll();

            if (string.IsNullOrWhiteSpace(q))
                return Json(all.Take(12).ToList());

            q = q.ToLower().Trim();

            var result = all
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.ToLower().Contains(q))
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSearchIndex()
        {
            try
            {
                var all = await GetAll();

                return Json(all.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    price = x.Price,
                    category = x.Category,
                    categoryLabel = x.Category == "motorbike" ? "xe may xe dien motorbike"
                                 : x.Category == "car" ? "o to oto car"
                                 : "phu kien accessory"
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = true,
                    message = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }

        private async Task<List<SearchItem>> GetAll()
        {
            var result = new List<SearchItem>();

            try
            {
                var motorbikes = await _api.GetMotorbikesAsync();
                if (motorbikes == null)
                    motorbikes = new List<Motorbike>();

                foreach (var x in motorbikes)
                {
                    result.Add(new SearchItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Category = "motorbike"
                    });
                }
            }
            catch { }

            try
            {
                var cars = await _api.GetCarsAsync();
                if (cars == null)
                    cars = new List<Car>();

                foreach (var x in cars)
                {
                    result.Add(new SearchItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Category = "car"
                    });
                }
            }
            catch { }

            try
            {
                var accessories = await _api.GetAccessoriesAsync();
                if (accessories == null)
                    accessories = new List<Accessory>();

                foreach (var x in accessories)
                {
                    result.Add(new SearchItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Category = "accessory"
                    });
                }
            }
            catch { }

            return result;
        }
    }

    public class SearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
    }
}