namespace ProjektZal.Models
{
    public class OrderItem
    {
        public int Id { get; set; } // Identyfikator w tabeli OrderItem

        public int OrderId { get; set; }
        public Order Order { get; set; } // Relacja do zamówienia

        public int ProductId { get; set; }
        public Product Product { get; set; } // Relacja do produktu

        public int Quantity { get; set; } // Ilość danego produktu w zamówieniu

        public decimal PricePerUnit { get; set; } // Cena za jednostkę
    }
}