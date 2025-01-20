using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjektZal.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, ErrorMessage = "Imię nie może być dłuższe niż 50 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Rola jest wymagana.")]
        public string Role { get; set; } = "User"; // Domyślna rola to "User"

        public ICollection<Cart> Carts { get; set; } = new List<Cart>(); // Inicjalizacja pustą kolekcją
        public ICollection<Order> Orders { get; set; } = new List<Order>(); // Inicjalizacja pustą kolekcją
    }
}
