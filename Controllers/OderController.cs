using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;

namespace VinfastWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly CartService _cart;
        public OrderController(CartService cart) => _cart = cart;

        [HttpPost]
        public IActionResult Checkout()
        {
            var items = _cart.GetCart();
            if (items == null || !items.Any())
                return Json(new { success = false, message = "Giỏ hàng trống" });

            // Tại đây bạn có thể thêm code lưu vào Database nếu cần
            // Sau khi "thanh toán thành công", hãy xóa giỏ hàng
            _cart.ClearCart();

            return Json(new { success = true });
        }

        public IActionResult Success()
        {
            return View(); // Tạo một View Success.cshtml để chúc mừng
        }
    }
}