using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 🛒 Interface لخدمة المشتريات
    /// تحتوي على كل عمليات المشتريات
    /// </summary>
    public interface IPurchaseService
    {
        // ═══════════════════════════════════════════════════
        // 📋 العمليات الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إنشاء فاتورة شراء جديدة
        /// </summary>
        Task<(bool Success, string Message, int? PurchaseId)> CreatePurchaseAsync(
            PurchaseCreateViewModel model);

        /// <summary>
        /// تعديل فاتورة موجودة (مسودة فقط)
        /// </summary>
        Task<(bool Success, string Message)> UpdatePurchaseAsync(
            int id, PurchaseCreateViewModel model);

        /// <summary>
        /// حذف فاتورة (مسودة فقط)
        /// </summary>
        Task<(bool Success, string Message)> DeletePurchaseAsync(int id);

        /// <summary>
        /// استلام الفاتورة (تأثير على المخزون والمورد)
        /// </summary>
        Task<(bool Success, string Message)> ReceivePurchaseAsync(
            PurchaseReceiveViewModel model);

        /// <summary>
        /// إلغاء فاتورة (عكس التأثيرات)
        /// </summary>
        Task<(bool Success, string Message)> CancelPurchaseAsync(
            PurchaseCancelViewModel model);

        /// <summary>
        /// نسخ فاتورة (Duplicate)
        /// </summary>
        Task<(bool Success, string Message, int? NewPurchaseId)> DuplicatePurchaseAsync(
            int sourceId);

        // ═══════════════════════════════════════════════════
        // 💳 إدارة الدفعات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تسجيل دفعة على فاتورة
        /// </summary>
        Task<(bool Success, string Message)> RegisterPaymentAsync(
            SupplierPaymentViewModel model);

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على قائمة الفواتير مع الفلترة
        /// </summary>
        Task<PurchaseListViewModel> GetPurchasesAsync(
            string? search = null,
            int? supplierId = null,
            InvoiceStatus? status = null,
            PaymentType? paymentType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// الحصول على فاتورة بالتفاصيل
        /// </summary>
        Task<PurchaseDetailsViewModel?> GetPurchaseDetailsAsync(int id);

        /// <summary>
        /// الحصول على ViewModel للتعديل
        /// </summary>
        Task<PurchaseCreateViewModel?> GetPurchaseForEditAsync(int id);

        /// <summary>
        /// الحصول على ViewModel للاستلام
        /// </summary>
        Task<PurchaseReceiveViewModel?> GetPurchaseForReceiveAsync(int id);

        // ═══════════════════════════════════════════════════
        // 📊 الإحصائيات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إحصائيات المشتريات
        /// </summary>
        Task<PurchaseStatsViewModel> GetStatsAsync();

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// توليد رقم فاتورة جديد (FACT-2025-00001)
        /// </summary>
        Task<string> GenerateInvoiceNumberAsync();

        /// <summary>
        /// التحقق من إمكانية تعديل الفاتورة
        /// </summary>
        Task<bool> CanEditAsync(int id);

        /// <summary>
        /// التحقق من إمكانية حذف الفاتورة
        /// </summary>
        Task<bool> CanDeleteAsync(int id);

        /// <summary>
        /// التحقق من إمكانية استلام الفاتورة
        /// </summary>
        Task<bool> CanReceiveAsync(int id);

        /// <summary>
        /// التحقق من إمكانية إلغاء الفاتورة
        /// </summary>
        Task<bool> CanCancelAsync(int id);
    }
}