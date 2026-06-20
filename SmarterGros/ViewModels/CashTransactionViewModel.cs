using System.ComponentModel.DataAnnotations;
using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🔄 ViewModel لتسجيل/تعديل حركة في الصندوق
    /// </summary>
    public class CashTransactionViewModel
    {
        public int? Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الحركة الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الحركة (يُولَّد تلقائياً)
        /// </summary>
        public string? TransactionNumber { get; set; }

        [Required(ErrorMessage = "تاريخ الحركة مطلوب")]
        [Display(Name = "تاريخ الحركة")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// الصندوق
        /// </summary>
        [Required]
        public int CashRegisterId { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 نوع وفئة الحركة
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "نوع الحركة مطلوب")]
        [Display(Name = "نوع الحركة")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "فئة الحركة مطلوبة")]
        [Display(Name = "فئة الحركة")]
        public TransactionCategory Category { get; set; }

        [Required]
        [Display(Name = "طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // ═══════════════════════════════════════════════════
        // 💵 المبلغ
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من 0")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 الوصف والملاحظات
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "الوصف مطلوب")]
        [MaxLength(500)]
        [Display(Name = "الوصف")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══════════════════════════════════════════════════
        // 🚚 الطرف المقابل (اختياري)
        // ═══════════════════════════════════════════════════

        [Display(Name = "المورد")]
        public int? SupplierId { get; set; }

        [Display(Name = "العميل")]
        public int? CustomerId { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 المرجع (للحركات التلقائية)
        // ═══════════════════════════════════════════════════

        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        // ═══════════════════════════════════════════════════
        // 📜 الشيك / التحويل
        // ═══════════════════════════════════════════════════

        [MaxLength(50)]
        [Display(Name = "رقم الشيك")]
        public string? CheckNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "اسم البنك")]
        public string? BankName { get; set; }

        [Display(Name = "تاريخ استحقاق الشيك")]
        [DataType(DataType.Date)]
        public DateTime? CheckDueDate { get; set; }
    }

    /// <summary>
    /// 📋 ViewModel لقائمة الحركات (مع الفلاتر)
    /// </summary>
    public class CashTransactionListViewModel
    {
        public List<CashTransaction> Transactions { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();

        // الفلاتر
        public string? SearchTerm { get; set; }
        public TransactionType? Type { get; set; }
        public TransactionCategory? Category { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public int? SupplierId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsCancelled { get; set; }

        // الإحصائيات
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount => TotalIncome - TotalExpense;
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// ❌ ViewModel لإلغاء حركة
    /// </summary>
    public class CancelCashTransactionViewModel
    {
        [Required]
        public int TransactionId { get; set; }

        public string? TransactionNumber { get; set; }
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
        [MaxLength(500)]
        [Display(Name = "سبب الإلغاء")]
        public string CancellationReason { get; set; } = string.Empty;
    }
}