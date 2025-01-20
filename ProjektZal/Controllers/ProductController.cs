using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjektZal.Models;
using System.Linq;

namespace ProjektZal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

    
        public IActionResult ManageProducts()
        {
            var products = _context.Products.ToList();
            ViewBag.Categories = _context.Categories.ToList(); 
            return View(products);
        }


        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var category = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "Wybrana kategoria nie istnieje.");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View("ManageProducts");
                }

                product.Category = category;
                _context.Products.Add(product);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            ViewBag.Categories = _context.Categories.ToList();
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
    }
}
