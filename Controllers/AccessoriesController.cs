using VinfastWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;

namespace VinfastWeb.Controllers
{
    public class AccessoriesController : Controller
    {
        private readonly ApiService _api;

        public AccessoriesController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var all = await _api.GetAccessoriesAsync();
            return View(all ?? new List<Accessory>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var all = await _api.GetAccessoriesAsync();

            if (all == null)
                return NotFound();

            var item = all.FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }
    }
}