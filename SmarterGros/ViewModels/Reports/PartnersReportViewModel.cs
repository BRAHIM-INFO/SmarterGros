using SmarterGros.Models;

namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 👥 ViewModel لتقرير الموردين والعملاء
    /// </summary>
    public class PartnersReportViewModel
    {
        public string ReportType { get; set; } = "customers"; // customers, suppliers
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-90);
        public DateTime DateTo { get; set; } = DateTime.Today;

        // ملخص العملاء
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalCustomersDebt { get; set; }
        public int CustomersWithDebt { get; set; }

        // ملخص الموردين
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public decimal TotalSuppliersDebt { get; set; }
        public int SuppliersWithDebt { get; set; }

        // التفاصيل
        public List<CustomerReportData> Customers { get; set; } = new();
        public List<SupplierReportData> Suppliers { get; set; } = new();

        // أعمار الديون (Aging)
        public List<DebtAgingData> CustomerDebtAging { get; set; } = new();
        public List<DebtAgingData> SupplierDebtAging { get; set; } = new();
    }

    public class CustomerReportData
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
        public int InvoicesCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal CurrentDebt { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int DaysSinceLastPurchase { get; set; }
    }

    public class SupplierReportData
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
        public int InvoicesCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal CurrentDebt { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }

    public class DebtAgingData
    {
        public string Period { get; set; } = string.Empty; // 0-30, 31-60, 61-90, 90+
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public string Color { get; set; } = "#28a745";
    }
}