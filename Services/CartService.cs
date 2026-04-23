//using System.Text.Json;
//using VinfastWeb.Models;

//namespace VinfastWeb.Services
//{
//    public class CartService
//    {
//        private readonly IHttpContextAccessor _ctx;
//        private const string CartKey = "cart";

//        public CartService(IHttpContextAccessor ctx) => _ctx = ctx;

//        private ISession Session => _ctx.HttpContext!.Session;

//        public List<CartItem> GetCart()
//        {
//            var json = Session.GetString(CartKey);
//            return string.IsNullOrEmpty(json)
//                ? new List<CartItem>()
//                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
//        }

//        public void SaveCart(List<CartItem> cart) =>
//            Session.SetString(CartKey, JsonSerializer.Serialize(cart));

//        public void AddItem(CartItem item)
//        {
//            var cart = GetCart();
//            var existing = cart.FirstOrDefault(c =>
//                c.ProductId == item.ProductId && c.ProductType == item.ProductType);
//            if (existing != null)
//                existing.Quantity += item.Quantity;
//            else
//                cart.Add(item);
//            SaveCart(cart);
//        }

//        public void RemoveItem(int productId, string productType)
//        {
//            var cart = GetCart();
//            cart.RemoveAll(c => c.ProductId == productId && c.ProductType == productType);
//            SaveCart(cart);
//        }

//        public void UpdateQuantity(int productId, string productType, int qty)
//        {
//            var cart = GetCart();
//            var item = cart.FirstOrDefault(c =>
//                c.ProductId == productId && c.ProductType == productType);
//            if (item != null)
//            {
//                if (qty <= 0) cart.Remove(item);
//                else item.Quantity = qty;
//            }
//            SaveCart(cart);
//        }

//        public void ClearCart()
//        {
//            Session.Remove(CartKey);
//        }

//        public int Count => GetCart().Sum(c => c.Quantity);
//    }
//}