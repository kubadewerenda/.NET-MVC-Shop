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
        [HttpGet]
        public IActionResult SearchUsers(string searchQuery)
        {
            var users = _context.Users
                .Where(u => u.Role == "User" && (string.IsNullOrEmpty(searchQuery) || u.Name.Contains(searchQuery) || u.Email.Contains(searchQuery)))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email
                })
                .ToList();

            return Json(users);
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
                TempData["SuccessMessage"] = "Użytkownik usunięty poprawnie.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można usunąć użytkownika.";
            }


            return RedirectToAction("AdminDashboard");
        }


        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = newStatus; // Zmień status zamówienia
                _context.SaveChanges(); // Zapisz zmiany w bazie danych
                TempData["SuccessMessage"] = "Status zamówienia został zaktualizowany.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie zaktualizowano zamówienia.";
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
                TempData["SuccessMessage"] = "Produkt został dodany.";

                return RedirectToAction("AdminDashboard");
            }
            TempData["ErrorMessage"] = "Nie dodano produktu.";
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

        [HttpGet]
        public IActionResult SearchProducts(string searchQuery)
        {
            var products = _context.Products
                .Where(p => string.IsNullOrEmpty(searchQuery) || p.Name.Contains(searchQuery))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description,
                    p.Stock
                })
                .ToList();

            return Json(products);
        }

        [HttpGet]
        public IActionResult SearchOrders(string searchQuery)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => string.IsNullOrEmpty(searchQuery) ||
                            o.User.Name.Contains(searchQuery) ||
                            o.User.Email.Contains(searchQuery) ||
                            o.Id.ToString().Contains(searchQuery))
                .Select(o => new
                {
                    o.Id,
                    UserName = o.User.Name,
                    UserEmail = o.User.Email,
                    OrderDate = o.OrderDate.ToShortDateString(),
                    o.Status,
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        oi.Product.Name,
                        oi.Quantity
                    })
                })
                .ToList();

            return Json(orders);
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

        [HttpGet]
        public IActionResult GetOrdersStats()
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-7);

                // Pobierz tylko daty zamówień
                var orders = _context.Orders
                    .Where(o => o.OrderDate >= startDate)
                    .Select(o => o.OrderDate.Date) // Tylko data (bez czasu)
                    .ToList() // Pobierz dane do pamięci
                    .GroupBy(date => date) // Grupowanie po dacie
                    .Select(g => new
                    {
                        OrderDate = g.Key.ToString("yyyy-MM-dd"), // Przekształcenie daty na string
                        OrderCount = g.Count() // Liczba zamówień dla każdej daty
                    })
                    .OrderBy(e => e.OrderDate) // Sortowanie po dacie
                    .ToList();

                return Json(orders); // Zwrócenie danych w formacie JSON
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd w GetOrdersStats: {ex.Message}");
                return StatusCode(500, new { error = "Wystąpił błąd podczas przetwarzania danych." });
            }
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
