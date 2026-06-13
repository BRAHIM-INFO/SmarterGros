// Models/Customer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

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

        [MaxLength(50)]
        public string? RC { get; set; }

        [MaxLength(50)]
        public string? NIF { get; set; }

        /// <summary>
        /// الدين الابتدائي - ديون سابقة قبل بدء استخدام البرنامج
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        /// <summary>
        /// الدين الابتدائي الصريح
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialDebt { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<CustomerPayment> Payments { get; set; }
            = new List<CustomerPayment>();

        // Computed Properties
        [NotMapped]
        public string AvatarLetter => string.IsNullOrEmpty(Name) ? "؟"
            : Name.Trim().Substring(0, 1).ToUpper();

        [NotMapped]
        public decimal TotalDebt => InitialDebt +
            (Sales?.Sum(s => s.TotalAmount) ?? 0) -
            (Payments?.Sum(p => p.Amount) ?? 0);
    }
} 