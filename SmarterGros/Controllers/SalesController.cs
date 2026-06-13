using Microsoft.AspNetCore.Mvc;

namespace SmarterGros.Controllers
{
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
