using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hotel_Management_System.Models;
using Microsoft.EntityFrameworkCore; // Added for .Include()
using System.Linq;

namespace Hotel_Management_System.Controllers
{
    public class FrontDeskController : Controller
    {
        private readonly HotelManagementDbContext _context;

        public FrontDeskController(HotelManagementDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "FrontDesk")] 
        public IActionResult Dashboard()
        {
            var bookings = _context.Bookings
                                   .Where(b => b.Status == "Pending")
                                   .ToList();

            Console.WriteLine($"Pending Bookings Count: {bookings.Count}"); 

            return View("Dashboard", bookings);
        }


        [Authorize(Roles = "FrontDesk")]
        public IActionResult Confirm(int bookingId)
        {

            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking != null && booking.Status == "Pending")
            {
                booking.Status = "Confirmed";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Booking confirmed successfully!";
            }
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "FrontDesk")]
        public IActionResult Cancel(int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking != null && booking.Status == "Confirmed")
            {
                booking.Status = "Pending";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Booking has been reverted to Pending status.";
            }
            return RedirectToAction("Dashboard");
        }
    }
}
