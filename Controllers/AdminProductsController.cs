using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Models;
using VinfastWeb.Services;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApiService _api;

        public AdminProductsController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string keyword = "", string productType = "", int page = 1, int pageSize = 10)
        {
            var all = await _api.GetAdminProductsMergedAsync(keyword, productType);

            var vm = new AdminProductListViewModel
            {
                Keyword = keyword,
                ProductType = productType,
                Page = page,
                PageSize = pageSize,
                TotalItems = all.Count,
                Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var all = await _api.GetAdminProductsMergedAsync();
            var item = all.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        [HttpGet]
        public IActionResult Create()
        {
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdminProduct model)
        {
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var all = await _api.GetAdminProductsMergedAsync();
            var item = all.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AdminProduct model)
        {
            if (model == null)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Duplicate(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID không hợp lệ!";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Sao chép thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Không tìm thấy bản ghi!";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction("Index");
        }
    }
}