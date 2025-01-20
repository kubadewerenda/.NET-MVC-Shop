using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjektZal.Models;
using System.Globalization;
using System.Linq;

namespace ProjektZal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("AdminDashboard");
        }

        // Panel admina
        public IActionResult AdminDashboard(string searchQuery)
        {
            // Lista użytkowników
            var users = _context.Users
                .Where(u => u.Role == "User");

            if (!string.IsNullOrEmpty(searchQuery))
            {
                users = users.Where(u => u.Name.Contains(searchQuery) || u.Email.Contains(searchQuery));
            }

            // Produkty
            var products = _context.Products.ToList();

            // Złożone zamówienia
            var orders = _context.Orders
                .Include(o => o.User) // Pobierz użytkownika
                .Include(o => o.OrderItems) // Pobierz pozycje zamówienia
                .ThenInclude(oi => oi.Product) // Pobierz produkty w zamówieniach
                .ToList();

            var categories = _context.Categories.ToList();
            Console.WriteLine($"Liczba kategorii: {categories.Count}");
            ViewBag.Categories = categories;

            var model = new AdminDashboardViewModel
            {
                Users = _context.Users.Where(u => u.Role == "User").ToList(),
                Products = _context.Products.ToList(),
                Orders = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ToList()
            };

            return View(model);
        }

        // Usuwanie użytkownika
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("AdminDashboard");
        }

        [HttpGet]
public IActionResult ViewUserDetails(int id)
{
    var user = _context.Users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        TempData["ErrorMessage"] = "Użytkownik nie istnieje.";
        return RedirectToAction("AdminDashboard");
    }

    ViewBag.UserDetails = user; // Przekazanie danych użytkownika do widoku
    return View("AdminDashboard");
}

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = newStatus; // Zmień status zamówienia
                _context.SaveChanges(); // Zapisz zmiany w bazie danych
            }

            return RedirectToAction("AdminDashboard"); // Odśwież panel administratora
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            try
            {
                product.Price = decimal.Parse(product.Price.ToString(CultureInfo.InvariantCulture));
            }
            catch (FormatException)
            {
                ModelState.AddModelError("Price", "Cena musi być podana w formacie dziesiętnym (np. 2.12).");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid.");

                // Pobierz kategorię na podstawie CategoryId
                var category = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                if (category == null)
                {
                    Console.WriteLine("Wybrana kategoria nie istnieje.");
                    ModelState.AddModelError("Category", "Wybrana kategoria nie istnieje.");
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Przypisz kategorię do produktu
                product.Category = category;

                // Dodaj produkt do bazy danych
                _context.Products.Add(product);
                _context.SaveChanges();

                return RedirectToAction("AdminDashboard");
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                price = product.Price,
                description = product.Description,
                stock = product.Stock,
                categoryId = product.CategoryId
            });
        }

        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Produkt nie istnieje.";
                    return RedirectToAction("AdminDashboard");
                }

                // Aktualizacja danych produktu
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Produkt został zaktualizowany.";
                return RedirectToAction("AdminDashboard");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Produkt nie istnieje.";
                return RedirectToAction("AdminDashboard");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Produkt został usunięty.";
            return RedirectToAction("AdminDashboard");
        }



    }

    // Model widoku AdminDashboard
    public class AdminDashboardViewModel
    {
        public List<User> Users { get; set; }
        public List<Product> Products { get; set; }
        public List<Order> Orders { get; set; }
    }
}
