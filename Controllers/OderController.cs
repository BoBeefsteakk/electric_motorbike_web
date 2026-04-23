using Microsoft.AspNetCore.Mvc;

namespace VinfastWeb.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Success()
        {
            return View();
        }
    }
}