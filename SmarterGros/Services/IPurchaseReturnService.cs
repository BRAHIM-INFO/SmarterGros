using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 🔄 Interface لخدمة مرتجعات المشتريات
    /// </summary>
    public interface IPurchaseReturnService
    {
        // ═══════════════════════════════════════════════════
        // 📋 العمليات الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إنشاء مرتجع شراء جديد
        /// </summary>
        Task<(bool Success, string Message, int? ReturnId)> CreateReturnAsync(
            PurchaseReturnCreateViewModel model);

        /// <summary>
        /// إلغاء مرتجع (عكس التأثيرات)
        /// </summary>
        Task<(bool Success, string Message)> CancelReturnAsync(
            int returnId, string cancellationReason);

        /// <summary>
        /// حذف مرتجع (للأدمن فقط)
        /// </summary>
        Task<(bool Success, string Message)> DeleteReturnAsync(int returnId);

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على قائمة المرتجعات
        /// </summary>
        Task<PurchaseReturnListViewModel> GetReturnsAsync(
            string? search = null,
            int? supplierId = null,
            ReturnRefundMethod? refundMethod = null,
            bool? isCancelled = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// الحصول على مرتجع بالتفاصيل
        /// </summary>
        Task<PurchaseReturnDetailsViewModel?> GetReturnDetailsAsync(int returnId);

        /// <summary>
        /// تجهيز ViewModel لإنشاء مرتجع من فاتورة
        /// </summary>
        Task<PurchaseReturnCreateViewModel?> GetReturnFormForPurchaseAsync(int purchaseId);

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// توليد رقم مرتجع جديد (RET-2025-00001)
        /// </summary>
        Task<string> GenerateReturnNumberAsync();

        /// <summary>
        /// التحقق من إمكانية إنشاء مرتجع لفاتورة
        /// </summary>
        Task<(bool CanReturn, string Reason)> CanCreateReturnForPurchaseAsync(int purchaseId);

        /// <summary>
        /// التحقق من إمكانية إلغاء المرتجع
        /// </summary>
        Task<bool> CanCancelReturnAsync(int returnId);
    }
}