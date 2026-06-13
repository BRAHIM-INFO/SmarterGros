namespace SmarterGros.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? QRCode { get; set; }
        public string? ImagePath { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Unit { get; set; }
        public string? Location { get; set; }
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Shelf { get; set; }
        public string? Level { get; set; }
        public string? Bin { get; set; }
        public decimal PurchasePriceHT { get; set; }
        public decimal TaxRate { get; set; } = 0;
        public decimal PurchasePriceTTC { get; set; }
        public decimal WholesalePriceHT { get; set; }
        public decimal WholesalePriceTTC { get; set; }
        public decimal WholesaleMargin { get; set; } = 10;
        public decimal SemiWholesalePriceHT { get; set; }
        public decimal SemiWholesalePriceTTC { get; set; }
        public decimal SemiWholesaleMargin { get; set; } = 15;
        public decimal RetailPriceHT { get; set; }
        public decimal RetailPriceTTC { get; set; }
        public decimal RetailMargin { get; set; } = 20;
        public int StockQuantity { get; set; } = 0;
        public int MinStockAlert { get; set; } = 0;
        public int PackagingQty { get; set; } = 1;
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? Specifications { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}