// Models/SupplierPayment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    public class SupplierPayment
    {
        public int Id { get; set; }

        // ===== العلاقات =====
        public int SupplierId { get; set; }

        /// <summary>
        /// إذا كانت null → الدفعة مرتبطة بالدين الابتدائي
        /// إذا كانت موجودة → مرتبطة بفاتورة شراء محددة
        /// </summary>
        public int? PurchaseId { get; set; }

        // ===== بيانات الدفعة =====
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ===== Navigation Properties =====
        public Supplier? Supplier { get; set; }
        public Purchase? Purchase { get; set; }
    }
}

//// Models/SupplierPayment.cs
//using SmarterGros.Models;

//public class SupplierPayment
//{
//    public int Id { get; set; }
//    public int SupplierId { get; set; }
//    public int? PurchaseId { get; set; }
//    public decimal Amount { get; set; }
//    public DateTime PaymentDate { get; set; }
//    public DateTime CreatedAt { get; set; } = DateTime.Now;

//    // Navigation
//    public Supplier? Supplier { get; set; }
//    public Purchase? Purchase { get; set; }
//}