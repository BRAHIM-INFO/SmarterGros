using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 💰 Interface لخدمة الصندوق
    /// يحتوي على كل عمليات الصندوق المالية
    /// </summary>
    public interface ICashRegisterService
    {
        // ═══════════════════════════════════════════════════
        // 📊 لوحة المعلومات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على لوحة معلومات الصندوق
        /// </summary>
        Task<CashDashboardViewModel> GetDashboardAsync(int? cashRegisterId = null);

        /// <summary>
        /// الحصول على الصندوق الافتراضي
        /// </summary>
        Task<CashRegister?> GetDefaultRegisterAsync();

        /// <summary>
        /// الحصول على الرصيد الحالي للصندوق
        /// </summary>
        Task<decimal> GetCurrentBalanceAsync(int? cashRegisterId = null);

        // ═══════════════════════════════════════════════════
        // 💰 إدارة الحركات اليدوية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إضافة حركة جديدة (يدوية)
        /// </summary>
        Task<(bool Success, string Message, int? TransactionId)> AddTransactionAsync(
            CashTransactionViewModel model);

        /// <summary>
        /// إلغاء حركة موجودة
        /// </summary>
        Task<(bool Success, string Message)> CancelTransactionAsync(
            CancelCashTransactionViewModel model);

        /// <summary>
        /// حذف حركة نهائياً (للأدمن فقط - خطير)
        /// </summary>
        Task<(bool Success, string Message)> DeleteTransactionAsync(int transactionId);

        // ═══════════════════════════════════════════════════
        // 🔄 الحركات التلقائية (من أنظمة أخرى)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تسجيل دفعة مشتريات تلقائياً (صادر)
        /// يُستدعى من PurchaseService عند الدفع النقدي
        /// </summary>
        Task<bool> RecordPurchasePaymentAsync(
            int purchaseId,
            string invoiceNumber,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null);

        /// <summary>
        /// تسجيل استرداد من مرتجع مشتريات (وارد)
        /// يُستدعى من PurchaseReturnService عند الاسترداد النقدي
        /// </summary>
        Task<bool> RecordPurchaseRefundAsync(
            int returnId,
            string returnNumber,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null);

        /// <summary>
        /// تسجيل دفعة لمورد (صادر)
        /// يُستدعى عند تسجيل دفعة عامة للمورد
        /// </summary>
        Task<bool> RecordSupplierPaymentAsync(
            int paymentId,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null);

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على قائمة الحركات مع الفلترة
        /// </summary>
        Task<CashTransactionListViewModel> GetTransactionsAsync(
            int? cashRegisterId = null,
            string? search = null,
            TransactionType? type = null,
            TransactionCategory? category = null,
            PaymentMethod? paymentMethod = null,
            int? supplierId = null,
            int? customerId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? isCancelled = null,
            int page = 1,
            int pageSize = 50);

        /// <summary>
        /// الحصول على تفاصيل حركة
        /// </summary>
        Task<CashTransaction?> GetTransactionByIdAsync(int id);

        // ═══════════════════════════════════════════════════
        // 🔒 الجرد اليومي
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تجهيز نموذج الجرد اليومي
        /// </summary>
        Task<DailyClosureViewModel> PrepareDailyClosureAsync(
            int? cashRegisterId = null,
            DateTime? date = null);

        /// <summary>
        /// تنفيذ الجرد اليومي
        /// </summary>
        Task<(bool Success, string Message, int? ClosureId)> PerformDailyClosureAsync(
            DailyClosureViewModel model);

        /// <summary>
        /// الحصول على قائمة الجردات السابقة
        /// </summary>
        Task<DailyClosureListViewModel> GetClosuresAsync(
            int? cashRegisterId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? hasDifferenceOnly = null);

        /// <summary>
        /// الحصول على جرد محدد
        /// </summary>
        Task<DailyClosure?> GetClosureByIdAsync(int id);

        /// <summary>
        /// هل اليوم مغلق؟
        /// </summary>
        Task<bool> IsDayClosedAsync(int cashRegisterId, DateTime date);

        // ═══════════════════════════════════════════════════
        // 📊 التقارير
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// توليد تقرير حسب الفترة
        /// </summary>
        Task<CashReportViewModel> GenerateReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? cashRegisterId = null,
            string reportType = "custom");

        /// <summary>
        /// تقرير يومي
        /// </summary>
        Task<CashReportViewModel> GetDailyReportAsync(DateTime date, int? cashRegisterId = null);

        /// <summary>
        /// تقرير شهري
        /// </summary>
        Task<CashReportViewModel> GetMonthlyReportAsync(int year, int month, int? cashRegisterId = null);

        /// <summary>
        /// تقرير سنوي
        /// </summary>
        Task<CashReportViewModel> GetYearlyReportAsync(int year, int? cashRegisterId = null);

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// توليد رقم حركة جديد (TRX-2026-00001)
        /// </summary>
        Task<string> GenerateTransactionNumberAsync();

        /// <summary>
        /// تحديد الرصيد الافتتاحي (مرة واحدة فقط)
        /// </summary>
        Task<(bool Success, string Message)> SetOpeningBalanceAsync(
            int cashRegisterId,
            decimal openingBalance);
    }
}