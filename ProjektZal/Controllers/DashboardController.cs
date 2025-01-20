using Microsoft.AspNetCore.Mvc;
using ProjektZal.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProjektZal.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult UserDashboard(int? categoryId)
        {
 
            var categories = _context.Categories.ToList();

 
            var products = categoryId.HasValue
                ? _context.Products.Where(p => p.CategoryId == categoryId).ToList()
                : _context.Products.ToList();

   
            var model = new DashboardViewModel
            {
                Categories = categories,
                Products = products
            };

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
    }
}