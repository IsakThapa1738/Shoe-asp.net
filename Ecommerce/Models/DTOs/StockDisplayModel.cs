namespace Ecommerce.Models.DTOs
{
    public class StockDisplayModel
    {
        public int Id { get; set; }
        public int ShoeId { get; set; }
        public int Quantity { get; set; }
        public string? ShoeName { get; set; }
    }
}