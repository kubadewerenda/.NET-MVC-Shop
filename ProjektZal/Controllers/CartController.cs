using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjektZal.Models;
using System.Text.Json;

namespace ProjektZal.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
        }

        public class RemoveFromCartRequest
        {
            public int CartId { get; set; }
        }

        [HttpPost]
        public IActionResult ConfirmOrder()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Użytkownik nie jest zalogowany.");
            }

            int userId = int.Parse(userIdClaim.Value);


            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Koszyk jest pusty." });
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
            
                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price),
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    OrderItems = cartItems.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        PricePerUnit = c.Product.Price
                    }).ToList()
                };

              
                _context.Orders.Add(order);

                foreach (var item in cartItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                        {
                            throw new Exception($"Produkt {product.Name} ma niewystarczający stan magazynowy.");
                        }
                        product.Stock -= item.Quantity;
                    }
                }

    
                _context.Carts.RemoveRange(cartItems);

  
                _context.SaveChanges();

      
                transaction.Commit();

                return Json(new { success = true, message = "Zamówienie zostało dodane." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = $"Wystąpił błąd: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddToCartRequest request)
        {
            if (request.ProductId == 0)
            {
                return Json(new { success = false, message = "Nieprawidłowy productId." });
            }

 
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "Użytkownik nie jest zalogowany." });
            }

            int userId = int.Parse(userIdClaim.Value);

  
            var product = _context.Products.FirstOrDefault(p => p.Id == request.ProductId);
            if (product == null)
            {
                return Json(new { success = false, message = "Produkt nie istnieje." });
            }

    
            var cartItem = _context.Carts.FirstOrDefault(c => c.ProductId == request.ProductId && c.UserId == userId);
            if (cartItem != null)
            {
        
                cartItem.Quantity += 1;
            }
            else
            {
 
                _context.Carts.Add(new Cart
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    Quantity = 1
                });
            }


            _context.SaveChanges();

            var totalItems = _context.Carts.Where(c => c.UserId == userId).Sum(c => c.Quantity);


            return Json(new { success = true, totalItems });
        }


        [HttpPost]
        public IActionResult Remove([FromBody] RemoveFromCartRequest request)
        {
            if (request.CartId == 0)
            {
                return Json(new { success = false, message = "Nieprawidłowe ID koszyka." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Użytkownik nie jest zalogowany.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItem = _context.Carts.FirstOrDefault(c => c.Id == request.CartId && c.UserId == userId);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();

                var totalItems = _context.Carts.Where(c => c.UserId == userId).Sum(c => c.Quantity);

                return Json(new { success = true, totalItems });
            }

            return Json(new { success = false, message = "Nie znaleziono produktu w koszyku." });
        }

        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Użytkownik nie jest zalogowany.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            return View(cartItems);
        }
    }
}