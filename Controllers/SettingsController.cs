using Microsoft.AspNetCore.Mvc;
using VinfastWeb.Services;

namespace VinfastWeb.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ProfileService _profileService;

        public SettingsController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public IActionResult Index()
        {
            var model = _profileService.GetProfile();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var model = _profileService.GetProfile();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditProfile(VinfastWeb.ViewModels.UserProfileViewModel model)
        {
            _profileService.SaveProfile(model);

            return RedirectToAction("Index");
        }
    }
}