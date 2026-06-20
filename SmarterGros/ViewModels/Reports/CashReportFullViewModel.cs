using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 💵 ViewModel لتقرير الصندوق الشامل
    /// </summary>
    public class CashReportFullViewModel
    {
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DateTo { get; set; } = DateTime.Today;
        public int? CashRegisterId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        // الملخص
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetCashFlow => TotalIncome - TotalExpense;
        public int TotalTransactions { get; set; }
        public int IncomeTransactions { get; set; }
        public int ExpenseTransactions { get; set; }

        // الرسوم البيانية
        public List<DailyCashFlowData> DailyCashFlow { get; set; } = new();
        public List<CategoryCashData> IncomeByCategory { get; set; } = new();
        public List<CategoryCashData> ExpenseByCategory { get; set; } = new();
        public List<PaymentMethodData> ByPaymentMethod { get; set; } = new();

        // الحركات
        public List<CashTransaction> Transactions { get; set; } = new();

        // الجردات
        public List<DailyClosure> Closures { get; set; } = new();
        public int ClosuresCount { get; set; }
        public decimal TotalDifferences { get; set; }

        // Dropdowns
        public List<CashRegister> CashRegisters { get; set; } = new();
    }

    public class DailyCashFlowData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance { get; set; }
    }

    public class CategoryCashData
    {
        public TransactionCategory Category { get; set; }
        public string CategoryName => Category.GetArabicName();
        public string Icon => Category.GetIcon();
        public string Color => Category.GetColor();
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentMethodData
    {
        public PaymentMethod Method { get; set; }
        public string MethodName => Method.GetArabicName();
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}