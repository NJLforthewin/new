using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hotel_Management_System.Models;
using System.Threading.Tasks;
using System.Linq;
using static BCrypt.Net.BCrypt;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Hotel_Management_System.Helpers;

namespace Hotel_Management_System.Controllers
{
    public class AccessController(HotelManagementDbContext context, ILogger<AccessController> logger) : Controller
    {
        private readonly HotelManagementDbContext _context = context;
        private readonly ILogger<AccessController> _logger = logger;

        // GET: Access/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Access/Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(VMLogin modelLogin)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == modelLogin.Email);

                if (user != null)
                {
                    // Use PasswordHelper to verify password
                    bool isPasswordValid = PasswordHelper.VerifyPassword(modelLogin.Password, user.PasswordHash);

                    if (isPasswordValid)
                    {
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.NameIdentifier, user.Email),
                            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                            new(ClaimTypes.Role, user.Role)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            IsPersistent = modelLogin.RememberMe
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return user.Role switch
                        {
                            "Admin" => RedirectToAction("Dashboard", "Admin"),
                            "FrontDesk" => RedirectToAction("Dashboard", "FrontDesk"),
                            _ => RedirectToAction("Index", "Home")
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Invalid password for user: {Email}", modelLogin.Email);
                        ViewData["ValidateMessage"] = "Invalid email or password.";
                    }
                }
                else
                {
                    _logger.LogWarning("User not found: {Email}", modelLogin.Email);
                    ViewData["ValidateMessage"] = "Invalid email or password.";
                }
            }

            return View(modelLogin);
        }

        // Add the GET: Access/Register method in AccessController
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(VMRegister model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Hash the password before saving
                string hashedPassword = PasswordHelper.HashPassword(model.Password);

                // Check how many admins exist in the database
                int adminCount = _context.Users.Count(u => u.Role == "Admin");

                // Allow only 4 admins; others become guests
                string assignedRole = (adminCount < 4) ? "Admin" : "Guest";

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = hashedPassword, // Store hashed password
                    Role = assignedRole,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }


        // POST: Access/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
