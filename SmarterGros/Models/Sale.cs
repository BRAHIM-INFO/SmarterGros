namespace SmarterGros.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string SaleType { get; set; } = "تجزئة";
        public string Status { get; set; } = "مكتملة";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}