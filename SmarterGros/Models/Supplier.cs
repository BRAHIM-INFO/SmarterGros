// Models/Supplier.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        // ===== المعلومات الأساسية =====
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? BusinessActivity { get; set; }

        // ===== معلومات الاتصال =====
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? Phone2 { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        // ===== المعلومات التجارية والضريبية =====
        /// <summary>RC - السجل التجاري</summary>
        [MaxLength(50)]
        public string? RC { get; set; }

        /// <summary>NIF - الرقم الجبائي</summary>
        [MaxLength(50)]
        public string? NIF { get; set; }

        /// <summary>AI - المادة الجبائية</summary>
        [MaxLength(50)]
        public string? AI { get; set; }

        /// <summary>NIS - رقم الإحصاء</summary>
        [MaxLength(50)]
        public string? NIS { get; set; }

        // ===== المعلومات البنكية =====
        [MaxLength(50)]
        public string? BankAccount { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        // ===== الدين الابتدائي =====
        /// <summary>
        /// الدين الابتدائي - ديون سابقة قبل بدء استخدام البرنامج
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialDebt { get; set; } = 0;

        // ===== الحالة =====
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ===== Navigation Properties =====
        public ICollection<Purchase> Purchases { get; set; }
            = new List<Purchase>();

        public ICollection<SupplierPayment> Payments { get; set; }
            = new List<SupplierPayment>();

        // ===== Computed Properties (غير محفوظة في DB) =====
        [NotMapped]
        public decimal TotalPurchases
            => Purchases?.Sum(p => p.TotalAmount) ?? 0;

        [NotMapped]
        public decimal TotalPaid
            => Payments?.Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal TotalDebt
            => InitialDebt + TotalPurchases - TotalPaid;

        [NotMapped]
        public string AvatarLetter
            => string.IsNullOrEmpty(Name) ? "؟"
               : Name.Trim().Substring(0, 1).ToUpper();

        [NotMapped]
        public string DisplayPhone
            => Phone ?? Phone2 ?? "---";
    }
}

//namespace SmarterGros.Models
//{
//    public class Supplier
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string? BusinessActivity { get; set; }
//        public string? Phone { get; set; }
//        public string? Phone2 { get; set; }
//        public string? Email { get; set; }
//        public string? Address { get; set; }
//        public string? City { get; set; }
//        public string? RC { get; set; }
//        public string? NIF { get; set; }
//        public string? BankAccount { get; set; }
//        public string? BankName { get; set; }
//        public bool IsActive { get; set; } = true;
//        public DateTime CreatedAt { get; set; } = DateTime.Now;
//        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
//    }
//}