using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 💰 Interface لخدمة المبيعات
    /// </summary>
    public interface ISaleService
    {
        // ═══════════════════════════════════════════════════
        // 📋 العمليات الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إنشاء فاتورة بيع جديدة
        /// </summary>
        Task<(bool Success, string Message, int? SaleId)> CreateSaleAsync(
            SaleCreateViewModel model);

        /// <summary>
        /// تعديل فاتورة (مسودة فقط)
        /// </summary>
        Task<(bool Success, string Message)> UpdateSaleAsync(
            int id, SaleCreateViewModel model);

        /// <summary>
        /// حذف فاتورة (مسودة فقط)
        /// </summary>
        Task<(bool Success, string Message)> DeleteSaleAsync(int id);

        /// <summary>
        /// إلغاء فاتورة (عكس التأثيرات)
        /// </summary>
        Task<(bool Success, string Message)> CancelSaleAsync(
            SaleCancelViewModel model);

        /// <summary>
        /// نسخ فاتورة
        /// </summary>
        Task<(bool Success, string Message, int? NewSaleId)> DuplicateSaleAsync(
            int sourceId);

        // ═══════════════════════════════════════════════════
        // 💳 إدارة الدفعات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تسجيل دفعة من عميل
        /// </summary>
        Task<(bool Success, string Message)> RegisterPaymentAsync(
            CustomerPaymentViewModel model);

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على قائمة الفواتير مع الفلترة
        /// </summary>
        Task<SaleListViewModel> GetSalesAsync(
            string? search = null,
            int? customerId = null,
            InvoiceStatus? status = null,
            PaymentType? paymentType = null,
            SalePriceType? priceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// الحصول على فاتورة بالتفاصيل
        /// </summary>
        Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int id);

        /// <summary>
        /// الحصول على ViewModel للتعديل
        /// </summary>
        Task<SaleCreateViewModel?> GetSaleForEditAsync(int id);

        // ═══════════════════════════════════════════════════
        // 📊 الإحصائيات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إحصائيات المبيعات
        /// </summary>
        Task<SaleStatsViewModel> GetStatsAsync();

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// توليد رقم فاتورة جديد (SAL-2026-00001)
        /// </summary>
        Task<string> GenerateInvoiceNumberAsync();

        /// <summary>
        /// التحقق من توفر المخزون
        /// </summary>
        Task<(bool Available, string Message)> CheckStockAvailabilityAsync(
            int productId, int quantity);

        /// <summary>
        /// التحقق من إمكانية التعديل
        /// </summary>
        Task<bool> CanEditAsync(int id);

        /// <summary>
        /// التحقق من إمكانية الحذف
        /// </summary>
        Task<bool> CanDeleteAsync(int id);

        /// <summary>
        /// التحقق من إمكانية الإلغاء
        /// </summary>
        Task<bool> CanCancelAsync(int id);
    }
}