using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjektZal.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ProjektZal.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        public IActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // Check if the email already exists
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    Console.WriteLine("Email already exists.");
                    ModelState.AddModelError("Email", "The email address is already in use.");
                    return View(model);
                }

                // Set default role to 'User'
                model.Role = "User";

                // Hash password before saving
                model.Password = _passwordHasher.HashPassword(model, model.Password);

                // Add user to the database
                _context.Users.Add(model);
                _context.SaveChanges();

                Console.WriteLine("User registered successfully.");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration.");
                return View(model);
            }
        }

        // GET: User/Login
        public IActionResult Login()
        {
            return View();
        }
       
        // POST: User/Login
        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
           
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
         
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return user.Role == "Admin" ?
                        RedirectToAction("Index", "Admin") :
                        RedirectToAction("UserDashboard", "User");
                }
            }

            ViewBag.Error = "Invalid email or password!";
            return View();
        }


        // GET: User/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
  
            var categories = _context.Categories.ToList();


            var products = _context.Products.ToList();


            var model = new DashboardViewModel
            {
                Categories = categories,
                Products = products
            };

  
            return View("~/Views/Dashboard/UserDashboard.cshtml", model);
        }


   
        [Authorize]
        public IActionResult Profile()
        {
            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
