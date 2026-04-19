using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;

namespace VinfastWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly CartService _cart;
        public OrderController(CartService cart) => _cart = cart;

        [HttpPost]
        public IActionResult CheckoutSelected([FromBody] CheckoutSelectedRequest request)
        {
            var cartItems = _cart.GetCart();

            if (cartItems == null || !cartItems.Any())
                return Json(new { success = false, message = "Giỏ hàng trống" });

            if (request == null || request.Items == null || !request.Items.Any())
                return Json(new { success = false, message = "Bạn chưa chọn sản phẩm nào" });

            var selectedItems = cartItems
                .Where(c => request.Items.Any(r =>
                    r.ProductId == c.ProductId &&
                    r.ProductType == c.ProductType))
                .ToList();

            if (!selectedItems.Any())
                return Json(new { success = false, message = "Không tìm thấy sản phẩm đã chọn trong giỏ hàng" });

            foreach (var cartItem in selectedItems)
            {
                var reqItem = request.Items.FirstOrDefault(r =>
                    r.ProductId == cartItem.ProductId &&
                    r.ProductType == cartItem.ProductType);

                if (reqItem != null && reqItem.Quantity > 0)
                {
                    cartItem.Quantity = reqItem.Quantity;
                }
            }

            // TODO: lưu selectedItems vào database nếu cần

            foreach (var item in selectedItems)
            {
                _cart.RemoveItem(item.ProductId, item.ProductType);
            }

            return Json(new { success = true });
        }

        public IActionResult Success()
        {
            return View();
        }
    }

    public class CheckoutSelectedRequest
    {
        public List<CheckoutSelectedItem> Items { get; set; } = new();
    }

    public class CheckoutSelectedItem
    {
        public int ProductId { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}