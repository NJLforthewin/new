using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Management_System.Controllers
{
    [Authorize(Roles = "FrontDesk")] // Only Front Desk Staff can access
    public class FrontDeskController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
