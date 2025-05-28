using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
