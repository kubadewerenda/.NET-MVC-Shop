using Microsoft.AspNetCore.Mvc;
using ProjektZal.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProjektZal.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .ToList();
            return View(orders);
        }


        [HttpPost]
        public IActionResult Create(int userId)
        {
            var cartItems = _context.Carts.Include(c => c.Product).ToList();
            if (cartItems.Any())
            {
                var totalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price);

                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = totalPrice,
                    OrderDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                _context.Carts.RemoveRange(cartItems);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
