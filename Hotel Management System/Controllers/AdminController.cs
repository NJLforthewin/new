using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hotel_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hotel_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can access this controller
    public class AdminController : Controller
    {
        private readonly HotelManagementDbContext _context;

        public AdminController(HotelManagementDbContext context)
        {
            _context = context;
        }

        // ✅ Move Dashboard() inside the AdminController
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: List all users
        public IActionResult UserList()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // Promote a user to Admin (only if there are fewer than 4 admins)
        public IActionResult PromoteToAdmin(int userId)
        {
            int adminCount = _context.Users.Count(u => u.Role == "Admin");

            if (adminCount >= 4)
            {
                TempData["ErrorMessage"] = "Cannot have more than 4 admins!";
                return RedirectToAction("UserList");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null && user.Role != "Admin")
            {
                user.Role = "Admin";
                _context.SaveChanges();
            }

            return RedirectToAction("UserList");
        }

        // Demote an Admin to Guest (only if there are more than 1 admin)
        public IActionResult DemoteToGuest(int userId)
        {
            int adminCount = _context.Users.Count(u => u.Role == "Admin");

            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "At least one admin is required!";
                return RedirectToAction("UserList");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null && user.Role == "Admin")
            {
                user.Role = "Guest";
                _context.SaveChanges();
            }

            return RedirectToAction("UserList");
        }
    }
}
