using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Hotel_Management_System.Models;
using Hotel_Management_System.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Management_System.Controllers
{
    public class BookingController : Controller
    {
        private readonly HotelManagementDbContext _context;
        private readonly PayMongoService _payMongoService;

        public BookingController(HotelManagementDbContext context, PayMongoService payMongoService)
        {
            _context = context;
            _payMongoService = payMongoService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            var rooms = _context.Rooms.Where(r => r.Status == "Available").ToList();
            ViewBag.Rooms = rooms;
            ViewBag.RoomPrices = rooms.ToDictionary(r => r.RoomId.ToString(), r => r.PricePerNight);
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Available").ToList();
                ViewBag.RoomPrices = ((List<Room>)ViewBag.Rooms).ToDictionary(r => r.RoomId.ToString(), r => r.PricePerNight);
                return View(booking);
            }

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomId);
            if (room == null || room.Status != "Available")
            {
                ModelState.AddModelError("RoomId", "Room is unavailable or does not exist.");
                return View(booking);
            }

            if (booking.CheckInDate >= booking.CheckOutDate)
            {
                ModelState.AddModelError("", "Check-in date must be before check-out date.");
                return View(booking);
            }

            int totalDays = Math.Max(1, (booking.CheckOutDate - booking.CheckInDate).Days);
            booking.TotalPrice = room.PricePerNight * totalDays;
            booking.Status = "Pending";

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            string? paymentIntent = await _payMongoService.CreatePayment(booking.TotalPrice, "PHP", "gcash");

            if (string.IsNullOrEmpty(paymentIntent))
            {
                TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            return Redirect(paymentIntent);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Payment(int bookingId)
        {
            var booking = _context.Bookings.Include(b => b.Room).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            return View(booking);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int bookingId, string paymentMethod)
        {
            var booking = _context.Bookings.Include(b => b.Room).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            var paymentResponse = await _payMongoService.CreatePayment(booking.TotalPrice, "PHP", paymentMethod);

            if (paymentResponse == null)
            {
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Payment", new { bookingId });
            }

            booking.Status = "Paid";
            if (booking.Room != null)
            {
                booking.Room.Status = "Booked";
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment successful! Your booking is now confirmed.";
            return RedirectToAction("Confirmation", new { bookingId });
        }

        [HttpGet]
        public IActionResult Confirmation(int bookingId)
        {
            var booking = _context.Bookings.Include(b => b.Room).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            return View(booking);
        }

        [Authorize(Roles = "Admin, FrontDesk")]
        public IActionResult DashboardBooking()
        {
            var bookings = _context.Bookings.Include(b => b.Room).Where(b => b.Status == "Paid").ToList();
            return RedirectToAction("Dashboard", "FrontDesk");
        }

        [HttpPost]
        [Authorize(Roles = "Admin, FrontDesk")]
        public IActionResult ConfirmBooking(int bookingId)
        {
            var booking = _context.Bookings.Include(b => b.Room).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            if (booking.Room != null)
            {
                booking.Room.Status = "Booked";
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Booking confirmed!";
            return RedirectToAction("DashboardBooking");
        }

        [HttpPost]
        [Authorize(Roles = "Admin, FrontDesk")]
        public IActionResult CancelBooking(int bookingId)
        {
            var booking = _context.Bookings.Include(b => b.Room).FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found!";
                return RedirectToAction("DashboardBooking");
            }

            if (booking.Room != null)
            {
                booking.Room.Status = "Vacant";
            }

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Booking has been permanently canceled.";
            return RedirectToAction("DashboardBooking");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ResetBookings()
        {
            foreach (var booking in _context.Bookings.Include(b => b.Room))
            {
                if (booking.Room != null)
                {
                    booking.Room.Status = "Vacant";
                }
            }

            _context.Bookings.RemoveRange(_context.Bookings);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "All bookings have been deleted.";
            return RedirectToAction("DashboardBooking");
        }
    }
}
