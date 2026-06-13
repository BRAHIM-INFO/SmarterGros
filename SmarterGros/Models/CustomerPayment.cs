// Models/CustomerPayment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    public class CustomerPayment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        /// <summary>
        /// إذا كانت null → الدفعة مرتبطة بالدين الابتدائي
        /// إذا كانت موجودة → مرتبطة بفاتورة بيع محددة
        /// </summary>
        public int? SaleId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Customer? Customer { get; set; }
        public Sale? Sale { get; set; }
    }
}