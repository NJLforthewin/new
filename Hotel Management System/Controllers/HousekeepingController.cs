using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Management_System.Controllers
{
    [Authorize(Roles = "Housekeeping")] // Only Housekeeping can access
    public class HousekeepingController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
