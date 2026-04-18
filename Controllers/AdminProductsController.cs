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
        private readonly IWebHostEnvironment _env;

        public AdminProductsController(ApiService api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

        private string? GetToken()
            => HttpContext.Session.GetString("AdminToken")
            ?? User.FindFirst("token")?.Value;

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        private string GetModelErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return errors.Any()
                ? string.Join(" | ", errors)
                : "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
        }

        private void RemoveOptionalImageValidation()
        {
            ModelState.Remove("ImageFile");
            ModelState.Remove("PathImage");
            ModelState.Remove("ImageUrl");
        }

        public async Task<IActionResult> Index(string keyword = "", string productType = "", int page = 1, int pageSize = 10)
        {
            var all = await _api.GetAdminProductsAsync(keyword, productType);

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
            var item = await _api.GetAdminProductAsync(id);

            if (item == null)
            {
                TempData["Error"] = $"Không tìm thấy sản phẩm #{id}.";
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminProduct
            {
                CreateDate = DateTime.Today,
                UpdateDate = DateTime.Today,
                Quantity = 1,
                Unit = "chiếc",
                PathImage = ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProduct model, IFormFile? ImageFile)
        {
            RemoveOptionalImageValidation();

            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");

            if (model.Price <= 0)
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0.");

            if (model.ProductType?.Trim() != "Xe máy")
                model.CategoryGroup = "";

            if (model.Quantity <= 0)
                model.Quantity = 1;

            if (string.IsNullOrWhiteSpace(model.Unit))
                model.Unit = "chiếc";

            if (!ModelState.IsValid)
            {
                TempData["Error"] = GetModelErrors();
                return View(model);
            }

            var uploadedPath = await SaveImageAsync(ImageFile);
            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                model.PathImage = uploadedPath;
            }
            else
            {
                model.PathImage ??= "";
            }

            model.CreateDate ??= DateTime.Today;
            model.UpdateDate ??= DateTime.Today;

            var token = GetToken();
            var ok = await _api.CreateAdminProductAsync(model, token);

            if (ok)
            {
                TempData["Success"] = $"Đã tạo sản phẩm \"{model.ProductName}\" thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Tạo sản phẩm thất bại. Vui lòng thử lại.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _api.GetAdminProductAsync(id);

            if (item == null)
            {
                TempData["Error"] = $"Không tìm thấy sản phẩm #{id}.";
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminProduct model, IFormFile? ImageFile)
        {
            RemoveOptionalImageValidation();

            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");

            if (model.Price <= 0)
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0.");

            if (model.ProductType?.Trim() != "Xe máy")
                model.CategoryGroup = "";

            if (model.Quantity <= 0)
                model.Quantity = 1;

            if (string.IsNullOrWhiteSpace(model.Unit))
                model.Unit = "chiếc";

            if (!ModelState.IsValid)
            {
                TempData["Error"] = GetModelErrors();
                return View(model);
            }

            var oldItem = await _api.GetAdminProductAsync(model.ProductID);
            if (oldItem == null)
            {
                TempData["Error"] = $"Không tìm thấy sản phẩm #{model.ProductID}.";
                return RedirectToAction(nameof(Index));
            }

            var uploadedPath = await SaveImageAsync(ImageFile);
            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                model.PathImage = uploadedPath;
            }
            else if (string.IsNullOrWhiteSpace(model.PathImage))
            {
                model.PathImage = oldItem.PathImage ?? "";
            }

            model.CreateDate = oldItem.CreateDate;

            if (model.UpdateDate == null)
                model.UpdateDate = oldItem.UpdateDate ?? DateTime.Today;

            var token = GetToken();
            var ok = await _api.UpdateAdminProductAsync(model, token);

            if (ok)
            {
                TempData["Success"] = $"Đã cập nhật sản phẩm \"{model.ProductName}\" thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Cập nhật thất bại. Vui lòng thử lại.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Duplicate(int id, string keyword = "", string productType = "", int page = 1, int pageSize = 10)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID không hợp lệ!";
                return RedirectToAction(nameof(Index), new
                {
                    keyword,
                    productType,
                    page,
                    pageSize
                });
            }

            var token = GetToken();
            var original = await _api.GetAdminProductAsync(id);

            var ok = await _api.DuplicateAdminProductAsync(id, token);

            if (ok)
            {
                var name = original?.ProductName ?? $"#{id}";
                TempData["Success"] = $"Đã tạo bản sao của \"{name}\". Bản sao được đặt tên \"Bản sao_{name}\".";
            }
            else
            {
                TempData["Error"] = "Sao chép thất bại. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new
            {
                keyword,
                productType,
                page,
                pageSize
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string keyword = "", string productType = "", int page = 1, int pageSize = 10)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID không hợp lệ!";
                return RedirectToAction(nameof(Index), new
                {
                    keyword,
                    productType,
                    page,
                    pageSize
                });
            }

            var token = GetToken();
            var ok = await _api.DeleteAdminProductAsync(id, token);

            if (ok)
                TempData["Success"] = $"Đã xóa sản phẩm #{id} thành công!";
            else
                TempData["Error"] = $"Xóa sản phẩm #{id} thất bại. Vui lòng thử lại.";

            return RedirectToAction(nameof(Index), new
            {
                keyword,
                productType,
                page,
                pageSize
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            var token = GetToken();
            var ok = await _api.DeleteManyAdminProductsAsync(ids, token);

            if (ok)
                TempData["Success"] = $"Đã xóa {ids.Count} sản phẩm thành công!";
            else
                TempData["Error"] = "Xóa nhiều sản phẩm thất bại. Vui lòng thử lại.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDuplicate(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            var token = GetToken();
            var ok = await _api.DuplicateManyAdminProductsAsync(ids, token);

            if (ok)
                TempData["Success"] = $"Đã sao chép {ids.Count} sản phẩm thành công!";
            else
                TempData["Error"] = "Sao chép nhiều sản phẩm thất bại. Vui lòng thử lại.";

            return RedirectToAction(nameof(Index));
        }
    }
}