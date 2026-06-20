using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📊 ViewModel للتقارير المتقدمة للصندوق
    /// </summary>
    public class CashReportViewModel
    {
        // ═══════════════════════════════════════════════════
        // 🔍 معلومات الفترة
        // ═══════════════════════════════════════════════════

        public string Title { get; set; } = string.Empty;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string ReportType { get; set; } = "daily"; // daily, weekly, monthly, yearly, custom

        // ═══════════════════════════════════════════════════
        // 💰 المعلومات الأساسية
        // ═══════════════════════════════════════════════════

        public CashRegister? CashRegister { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 الإجماليات
        // ═══════════════════════════════════════════════════

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount => TotalIncome - TotalExpense;

        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }
        public int TotalTransactions => IncomeCount + ExpenseCount;

        // ═══════════════════════════════════════════════════
        // 📈 إحصائيات حسب الفئة
        // ═══════════════════════════════════════════════════

        public List<CategoryReportItem> IncomeByCategory { get; set; } = new();
        public List<CategoryReportItem> ExpenseByCategory { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📅 إحصائيات حسب اليوم
        // ═══════════════════════════════════════════════════

        public List<DailyReportItem> DailyBreakdown { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 👥 إحصائيات حسب الطرف
        // ═══════════════════════════════════════════════════

        public List<PartyReportItem> TopSuppliers { get; set; } = new();
        public List<PartyReportItem> TopCustomers { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📜 الحركات (للعرض التفصيلي)
        // ═══════════════════════════════════════════════════

        public List<CashTransaction> Transactions { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 💳 إحصائيات حسب طريقة الدفع
        // ═══════════════════════════════════════════════════

        public List<PaymentMethodStats> PaymentMethodsStats { get; set; } = new();
    }

    /// <summary>
    /// 📂 عنصر تقرير حسب الفئة
    /// </summary>
    public class CategoryReportItem
    {
        public TransactionCategory Category { get; set; }
        public string CategoryName => Category.GetArabicName();
        public string Icon => Category.GetIcon();
        public string Color => Category.GetColor();
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// 📅 عنصر تقرير يومي
    /// </summary>
    public class DailyReportItem
    {
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net => Income - Expense;
        public int TransactionsCount { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    /// <summary>
    /// 👤 عنصر تقرير حسب الطرف (مورد/عميل)
    /// </summary>
    public class PartyReportItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionsCount { get; set; }
    }

    /// <summary>
    /// 💳 إحصائيات طريقة الدفع
    /// </summary>
    public class PaymentMethodStats
    {
        public PaymentMethod Method { get; set; }
        public string MethodName => Method.GetArabicName();
        public string Icon => Method.GetIcon();
        public string Color => Method.GetBadgeColor();
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}