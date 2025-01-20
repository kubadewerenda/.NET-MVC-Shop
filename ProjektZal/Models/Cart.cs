namespace ProjektZal.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // Powiązanie z produktem
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Powiązanie z użytkownikiem
        public int UserId { get; set; } // Klucz obcy do tabeli User
        public User User { get; set; }

        // Ilość produktów
        public int Quantity { get; set; }
    }
}
