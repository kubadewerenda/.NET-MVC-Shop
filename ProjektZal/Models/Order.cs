namespace ProjektZal.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } // Relacja do użytkownika

        public decimal TotalPrice { get; set; } // Łączna cena zamówienia
        public DateTime OrderDate { get; set; } // Data zamówienia
        public string Status { get; set; } // Pending, Completed

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Relacja do OrderItems
    }
}
