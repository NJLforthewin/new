using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hotel_Management_System.Models;
using System.Linq;

namespace Hotel_Management_System.Controllers
{
    [Authorize(Roles = "Admin,FrontDesk")] 
    public class RoomController : Controller
    {
        private readonly HotelManagementDbContext _context;

        public RoomController(HotelManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult Rooms()
        {
            var rooms = _context.Rooms.ToList(); 
            return View(rooms); 
        }

        public IActionResult AvailableRooms()
        {
            var availableRooms = _context.Rooms.Where(r => r.Status == "Available").ToList();
            return View(availableRooms);
        }


        public IActionResult BookedRooms()
        {
            var bookedRooms = _context.Rooms.Where(r => r.Status == "Booked").ToList();
            return View(bookedRooms);
        }

    }
}
