namespace SmarterGros.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public string? Reason { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}