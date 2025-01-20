using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProjektZal.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cena produktu jest wymagana.")]
        [Range(0.01, 10000.00, ErrorMessage = "Cena musi być pomiędzy 0.01 a 10000.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Opis nie może być dłuższy niż 500 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Stan magazynowy jest wymagany.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stan magazynowy musi być większy lub równy 0.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }



}