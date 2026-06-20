namespace SmarterGros.Services
{
    /// <summary>
    /// 🖥️ Interface للحصول على Hardware ID
    /// </summary>
    public interface IHardwareIdService
    {
        /// <summary>
        /// الحصول على Hardware ID الفريد للجهاز
        /// </summary>
        string GetHardwareId();

        /// <summary>
        /// الحصول على Hardware ID بشكل مختصر للعرض
        /// </summary>
        string GetDisplayHardwareId();
    }
}