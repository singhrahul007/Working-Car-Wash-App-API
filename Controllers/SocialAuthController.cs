using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    public class SocialAuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
