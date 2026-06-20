using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 📊 Interface لخدمة لوحة التحكم
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// الحصول على بيانات لوحة التحكم الكاملة
        /// </summary>
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}