using SmarterGros.Models;

namespace SmarterGros.Services
{
    /// <summary>
    /// 🔐 Interface لخدمة الترخيص
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// تهيئة الترخيص (يُستدعى عند أول تشغيل)
        /// </summary>
        Task<LicenseInfo> InitializeLicenseAsync();

        /// <summary>
        /// التحقق من حالة الترخيص
        /// </summary>
        Task<LicenseStatus> CheckLicenseAsync();

        /// <summary>
        /// تفعيل الترخيص بمفتاح
        /// </summary>
        Task<(bool Success, string Message)> ActivateLicenseAsync(string activationKey);

        /// <summary>
        /// الحصول على معلومات الترخيص الحالي
        /// </summary>
        Task<LicenseInfo?> GetCurrentLicenseAsync();

        /// <summary>
        /// عدد الأيام المتبقية
        /// </summary>
        Task<int> GetRemainingDaysAsync();
    }

    /// <summary>
    /// 📊 حالة الترخيص
    /// </summary>
    public enum LicenseStatus
    {
        Active,              // ✅ نشط
        TrialActive,         // 🆓 تجريبي نشط
        TrialExpiring,       // ⚠️ تجريبي قرب الانتهاء
        Expired,             // ❌ منتهي
        Invalid,             // 🚫 غير صالح
        NotActivated         // 🔓 لم يتم التفعيل
    }
}