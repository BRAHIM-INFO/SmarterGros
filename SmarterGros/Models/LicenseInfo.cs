using System.ComponentModel.DataAnnotations;

namespace SmarterGros.Models
{
    /// <summary>
    /// 🔐 معلومات ترخيص النظام
    /// </summary>
    public class LicenseInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// Hardware ID للجهاز
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string HardwareId { get; set; } = string.Empty;

        /// <summary>
        /// مفتاح التفعيل
        /// </summary>
        [MaxLength(300)]
        public string? ActivationKey { get; set; }

        /// <summary>
        /// نوع الترخيص: Trial, Full, Yearly
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string LicenseType { get; set; } = "Trial";

        /// <summary>
        /// تاريخ التثبيت الأول
        /// </summary>
        public DateTime InstallationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاريخ التفعيل
        /// </summary>
        public DateTime? ActivationDate { get; set; }

        /// <summary>
        /// تاريخ انتهاء الصلاحية
        /// (null للترخيص الدائم)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// هل الترخيص مفعّل؟
        /// </summary>
        public bool IsActivated { get; set; } = false;

        /// <summary>
        /// آخر تاريخ تشغيل (لمنع التلاعب)
        /// </summary>
        public DateTime LastRunDate { get; set; } = DateTime.Now;

        /// <summary>
        /// عدد مرات التشغيل
        /// </summary>
        public int RunCount { get; set; } = 0;

        /// <summary>
        /// معلومات العميل (إن وُجدت)
        /// </summary>
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        [MaxLength(50)]
        public string? CustomerPhone { get; set; }

        [MaxLength(100)]
        public string? CustomerCity { get; set; }
    }
}