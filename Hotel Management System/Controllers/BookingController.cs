using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hotel_Management_System.Models;
using System.Linq;

namespace Hotel_Management_System.Controllers
{
    public class BookingController : Controller
    {
        private readonly HotelManagementDbContext _context;

        public BookingController(HotelManagementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomId);
                if (room != null)
                {
                    booking.Status = "Pending"; // The room gets booked immediately
                    room.Status = "Booked"; // Change room status
                    _context.Bookings.Add(booking);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Room booked successfully!";
                    return RedirectToAction("DashboardBooking");
                }
                else
                {
                    TempData["ErrorMessage"] = "Room does not exist.";
                }
            }

            ViewBag.Rooms = _context.Rooms.ToList(); // Load all rooms, not just available ones
            return View("Create", booking);
        }
        



        // Booking dashboard (Admins/Front Desk Only)
        [Authorize(Roles = "Admin,FrontDesk")]
        public IActionResult DashboardBooking()
        {
            var bookings = _context.Bookings.ToList();
            return View("DashboardBooking", bookings);
        }
    }
}
