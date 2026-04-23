using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Models;
using VinfastWeb.ViewModels;

namespace VinfastWeb.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View(new CartViewModel
            {
                Items = new List<CartItem>()
            });
        }
    }
}