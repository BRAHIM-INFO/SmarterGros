// ═══════════════════════════════════════════════════════════════════════
// 📁 LicenseService.cs - النظام الأصلي (ASP.NET Core MVC)
// ═══════════════════════════════════════════════════════════════════════
// ✅ النسخة النهائية المُصحَّحة باستخدام Base32 (Case-Insensitive)
// ✅ تعمل 100% مع أداة WPF لتوليد المفاتيح
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Services;
using System.Security.Cryptography;
using System.Text;

public class LicenseService : ILicenseService
{
    private readonly ApplicationDbContext _context;
    private readonly IHardwareIdService _hardwareIdService;

    // 🔑 المفتاح السري - يجب أن يكون موحداً مع أداة التوليد (WPF)
    private const string SECRET_KEY = "IbdaaSoft@SmarterGros@2026@SecureKey!";
    private const int TRIAL_DAYS = 7;

    public LicenseService(
        ApplicationDbContext context,
        IHardwareIdService hardwareIdService)
    {
        _context = context;
        _hardwareIdService = hardwareIdService;
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🚀 InitializeLicenseAsync - تهيئة الترخيص عند أول تشغيل
    // ═══════════════════════════════════════════════════════════════════
    public async Task<LicenseInfo> InitializeLicenseAsync()
    {
        var hardwareId = _hardwareIdService.GetHardwareId();
        var existing = await _context.LicenseInfos
            .FirstOrDefaultAsync(l => l.HardwareId == hardwareId);

        if (existing != null)
        {
            existing.LastRunDate = DateTime.Now;
            existing.RunCount++;
            await _context.SaveChangesAsync();
            return existing;
        }

        // ─── إنشاء ترخيص تجريبي جديد لمدة 7 أيام ───
        var newLicense = new LicenseInfo
        {
            HardwareId = hardwareId,
            LicenseType = "Trial",
            InstallationDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddDays(TRIAL_DAYS),
            IsActivated = false,
            LastRunDate = DateTime.Now,
            RunCount = 1
        };

        _context.LicenseInfos.Add(newLicense);
        await _context.SaveChangesAsync();
        return newLicense;
    }

    // ═══════════════════════════════════════════════════════════════════
    // ✅ CheckLicenseAsync - التحقق من حالة الترخيص
    // ═══════════════════════════════════════════════════════════════════
    public async Task<LicenseStatus> CheckLicenseAsync()
    {
        var license = await GetCurrentLicenseAsync();
        if (license == null)
        {
            await InitializeLicenseAsync();
            return LicenseStatus.TrialActive;
        }

        // 🛡️ منع التلاعب بالتاريخ
        if (DateTime.Now < license.LastRunDate.AddMinutes(-5))
            return LicenseStatus.Invalid;

        license.LastRunDate = DateTime.Now;
        license.RunCount++;
        await _context.SaveChangesAsync();

        // ─── ترخيص دائم (Full / Lifetime) ───
        if (license.IsActivated &&
            (license.LicenseType == "Full" || license.LicenseType == "Lifetime"))
            return LicenseStatus.Active;

        // ─── ترخيص محدود (Yearly / Monthly / Custom) ───
        if (license.IsActivated &&
            (license.LicenseType == "Yearly" ||
             license.LicenseType == "Monthly" ||
             license.LicenseType == "Custom"))
        {
            if (license.ExpiryDate.HasValue && DateTime.Now > license.ExpiryDate.Value)
                return LicenseStatus.Expired;
            return LicenseStatus.Active;
        }

        // ─── الترخيص التجريبي ───
        if (license.ExpiryDate.HasValue)
        {
            var daysRemaining = (license.ExpiryDate.Value - DateTime.Now).Days;
            if (daysRemaining < 0) return LicenseStatus.Expired;
            if (daysRemaining <= 2) return LicenseStatus.TrialExpiring;
            return LicenseStatus.TrialActive;
        }

        return LicenseStatus.NotActivated;
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🔑 ActivateLicenseAsync - تفعيل الترخيص بمفتاح
    // ═══════════════════════════════════════════════════════════════════
    public async Task<(bool Success, string Message)> ActivateLicenseAsync(string activationKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activationKey))
                return (false, "❌ يرجى إدخال مفتاح التفعيل");

            // ✅ تنظيف المفتاح: حذف المسافات وتحويله للأحرف الكبيرة
            // Base32 يحتوي فقط على A-Z و 2-7، لذا ToUpper آمن تماماً
            activationKey = activationKey.Trim().ToUpper().Replace(" ", "");

            // ✅ التحقق من صيغة المفتاح
            if (!IsValidKeyFormat(activationKey))
                return (false, "❌ صيغة المفتاح غير صحيحة\nالشكل الصحيح: SMARTERGROS-XXXX-XXXX-...");

            // ✅ فك تشفير المفتاح واستخراج البيانات
            var keyData = DecryptKey(activationKey);
            if (keyData == null)
                return (false, "❌ مفتاح التفعيل غير صالح أو تالف");

            // ✅ التحقق من تطابق Hardware ID
            var currentHardwareId = _hardwareIdService.GetHardwareId();
            if (!keyData.HardwareId.Equals(currentHardwareId, StringComparison.OrdinalIgnoreCase))
                return (false,
                    $"❌ هذا المفتاح غير صالح لهذا الجهاز\n\n" +
                    $"معرّف جهازك: {currentHardwareId}\n" +
                    $"معرّف المفتاح: {keyData.HardwareId}\n\n" +
                    $"يرجى التواصل مع المبرمج للحصول على مفتاح خاص بجهازك");

            // ✅ التحقق من تاريخ الانتهاء
            if (keyData.ExpiryDate.HasValue && DateTime.Now > keyData.ExpiryDate.Value)
                return (false, "❌ هذا المفتاح منتهي الصلاحية");

            // ✅ حفظ بيانات التفعيل في قاعدة البيانات
            var license = await GetCurrentLicenseAsync();
            if (license == null)
                license = await InitializeLicenseAsync();

            license.ActivationKey = activationKey;
            license.LicenseType = keyData.LicenseType;
            license.ActivationDate = DateTime.Now;
            license.ExpiryDate = keyData.ExpiryDate;
            license.IsActivated = true;
            license.CustomerName = keyData.CustomerName;

            await _context.SaveChangesAsync();

            // ─── رسالة النجاح حسب نوع الترخيص ───
            var successMessage = keyData.LicenseType switch
            {
                "Full" or "Lifetime"
                    => "✅ تم تفعيل النسخة الكاملة بنجاح!\n\nشكراً لاختياركم SmarterGros 🎉",
                "Yearly"
                    => $"✅ تم تفعيل الاشتراك السنوي بنجاح!\nصالح حتى: {keyData.ExpiryDate:yyyy/MM/dd}",
                "Monthly"
                    => $"✅ تم تفعيل الاشتراك الشهري بنجاح!\nصالح حتى: {keyData.ExpiryDate:yyyy/MM/dd}",
                "Custom"
                    => $"✅ تم التفعيل بنجاح!\nصالح حتى: {keyData.ExpiryDate:yyyy/MM/dd}",
                _ => "✅ تم التفعيل بنجاح!"
            };

            return (true, successMessage);
        }
        catch (Exception ex)
        {
            return (false, $"❌ حدث خطأ: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // 📊 GetCurrentLicenseAsync - جلب معلومات الترخيص الحالي
    // ═══════════════════════════════════════════════════════════════════
    public async Task<LicenseInfo?> GetCurrentLicenseAsync()
    {
        var hardwareId = _hardwareIdService.GetHardwareId();
        return await _context.LicenseInfos
            .FirstOrDefaultAsync(l => l.HardwareId == hardwareId);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ⏰ GetRemainingDaysAsync - الأيام المتبقية
    // ═══════════════════════════════════════════════════════════════════
    public async Task<int> GetRemainingDaysAsync()
    {
        var license = await GetCurrentLicenseAsync();
        if (license == null) return 0;

        // ترخيص دائم = أيام لا نهائية
        if (license.IsActivated &&
            (license.LicenseType == "Full" || license.LicenseType == "Lifetime"))
            return int.MaxValue;

        if (!license.ExpiryDate.HasValue) return 0;

        var days = (license.ExpiryDate.Value - DateTime.Now).Days;
        return days < 0 ? 0 : days;
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🔓 DecryptKey - فك تشفير المفتاح واستخراج البيانات
    // ═══════════════════════════════════════════════════════════════════
    // 🆕 يستخدم Base32 الآن (مع تسجيل تشخيصي للأخطاء)
    // ═══════════════════════════════════════════════════════════════════
    private KeyData? DecryptKey(string activationKey)
    {
        // ─── ملف التشخيص على سطح المكتب (للتطوير - يمكن حذفه لاحقاً) ───
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "license_debug.txt");

        var log = new StringBuilder();
        log.AppendLine("═══════ DECRYPT DEBUG ═══════");
        log.AppendLine($"Time: {DateTime.Now}");
        log.AppendLine($"Input Key: [{activationKey}]");
        log.AppendLine($"Input Length: {activationKey.Length}");

        try
        {
            // 1️⃣ إزالة البادئة "SMARTERGROS-"
            var keyPart = activationKey.Replace("SMARTERGROS-", "");
            log.AppendLine($"After removing prefix: [{keyPart}]");

            // 2️⃣ إزالة الشرطات للحصول على Base32 الأصلي
            var base32 = keyPart.Replace("-", "");
            log.AppendLine($"Base32 string: [{base32}]");
            log.AppendLine($"Base32 length: {base32.Length}");

            // 3️⃣ فك التشفير (Base32 → bytes → AES Decrypt)
            log.AppendLine("Attempting to decrypt...");
            var decrypted = Decrypt(base32);
            log.AppendLine($"✅ Decrypted: [{decrypted}]");

            // 4️⃣ تحليل البيانات: HardwareId|LicenseType|ExpiryDate|CustomerName
            var parts = decrypted.Split('|');
            log.AppendLine($"Parts count: {parts.Length}");
            for (int i = 0; i < parts.Length; i++)
                log.AppendLine($"  [{i}] = [{parts[i]}]");

            if (parts.Length < 3)
            {
                log.AppendLine("❌ Parts < 3, returning null");
                File.WriteAllText(logPath, log.ToString());
                return null;
            }

            var keyData = new KeyData
            {
                HardwareId = parts[0],
                LicenseType = parts[1],
                ExpiryDate = string.IsNullOrEmpty(parts[2]) ? null : DateTime.Parse(parts[2]),
                CustomerName = parts.Length > 3 ? parts[3] : null
            };

            log.AppendLine("✅ SUCCESS!");
            log.AppendLine($"HardwareId: {keyData.HardwareId}");
            log.AppendLine($"LicenseType: {keyData.LicenseType}");
            log.AppendLine($"ExpiryDate: {keyData.ExpiryDate}");
            log.AppendLine($"CustomerName: {keyData.CustomerName}");

            // ─── مقارنة مع الجهاز الحالي ───
            var currentHwId = _hardwareIdService.GetHardwareId();
            log.AppendLine($"\nCurrent Hardware ID: [{currentHwId}]");
            log.AppendLine($"Key Hardware ID:     [{keyData.HardwareId}]");
            log.AppendLine($"Match: {keyData.HardwareId.Equals(currentHwId, StringComparison.OrdinalIgnoreCase)}");

            File.WriteAllText(logPath, log.ToString());
            return keyData;
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Exception: {ex.GetType().Name}");
            log.AppendLine($"❌ Message: {ex.Message}");
            log.AppendLine($"❌ Stack:\n{ex.StackTrace}");
            File.WriteAllText(logPath, log.ToString());
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🔐 Encrypt - تشفير AES-256 + ترميز Base32
    // ═══════════════════════════════════════════════════════════════════════
    // 🔐 Encrypt - مع ضغط GZip لتقصير المفتاح
    // ═══════════════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════════
    // 🔐 Encrypt - بدون GZip
    // ═══════════════════════════════════════════════════════════════════
    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(SECRET_KEY));
        aes.Key = key;
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // ✅ مباشرة Base32 بدون ضغط
        return Base32Encode(cipherBytes);
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🔓 Decrypt - بدون GZip
    // ═══════════════════════════════════════════════════════════════════
    public static string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(SECRET_KEY));
        aes.Key = key;
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor();

        // ✅ Base32 → bytes مباشرة
        var cipherBytes = Base32Decode(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        // ✅ بدون فك ضغط
        return Encoding.UTF8.GetString(plainBytes);
    }
     
    // ═══════════════════════════════════════════════════════════════════
    // ✅ IsValidKeyFormat - التحقق من صيغة المفتاح
    // ═══════════════════════════════════════════════════════════════════
    private bool IsValidKeyFormat(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        if (!key.StartsWith("SMARTERGROS-")) return false;

        var parts = key.Split('-');
        if (parts.Length < 3) return false;

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🛠️ GenerateActivationKey - توليد مفتاح تفعيل
    // ═══════════════════════════════════════════════════════════════════
    // يُستخدم في أداة التوليد (WPF) - نفس الكود يعمل في الطرفين
    // ═══════════════════════════════════════════════════════════════════
    public static string GenerateActivationKey(
        string hardwareId,
        string licenseType,
        DateTime? expiryDate = null,
        string? customerName = null)
    {
        // 1️⃣ بناء البيانات: HardwareId|LicenseType|ExpiryDate|CustomerName
        var data = $"{hardwareId}|{licenseType}|{expiryDate?.ToString("yyyy-MM-dd") ?? ""}|{customerName ?? ""}";

        // 2️⃣ تشفير AES → Base32
        var encrypted = Encrypt(data);

        // 3️⃣ تنسيق المفتاح بمجموعات من 4 أحرف مفصولة بشرطات
        var formatted = FormatKey(encrypted);

        return $"SMARTERGROS-{formatted}";
    }

    // ═══════════════════════════════════════════════════════════════════
    // 🔧 FormatKey - تنسيق النص بمجموعات من 4 أحرف
    // ═══════════════════════════════════════════════════════════════════
    // مثال: "ABCDEFGHIJKL" → "ABCD-EFGH-IJKL"
    // ═══════════════════════════════════════════════════════════════════
    private static string FormatKey(string input)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append('-');
            sb.Append(input[i]);
        }
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════
    // 📦 Base32 Encoding/Decoding (RFC 4648)
    // ═══════════════════════════════════════════════════════════════════
    // ⭐ ميزات Base32:
    //    - يحتوي فقط على A-Z و 2-7 (32 حرف)
    //    - لا يحتوي على أحرف صغيرة → آمن مع ToUpper()
    //    - لا يحتوي على رموز خاصة (/ + =) → آمن للتنسيق
    //    - مقروء وسهل النسخ
    //    - Case-insensitive بطبيعته
    // ═══════════════════════════════════════════════════════════════════

    private const string BASE32_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// تحويل bytes إلى نص Base32
    /// </summary>
    public static string Base32Encode(byte[] data)
    {
        if (data == null || data.Length == 0) return "";

        var sb = new StringBuilder((data.Length * 8 + 4) / 5);
        int bits = 0;
        int value = 0;

        foreach (byte b in data)
        {
            value = (value << 8) | b;
            bits += 8;

            // كل 5 bits = حرف واحد في Base32
            while (bits >= 5)
            {
                sb.Append(BASE32_ALPHABET[(value >> (bits - 5)) & 0x1F]);
                bits -= 5;
            }
        }

        // معالجة البتات المتبقية (padding)
        if (bits > 0)
        {
            sb.Append(BASE32_ALPHABET[(value << (5 - bits)) & 0x1F]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// تحويل نص Base32 إلى bytes
    /// </summary>
    public static byte[] Base32Decode(string input)
    {
        if (string.IsNullOrEmpty(input)) return Array.Empty<byte>();

        input = input.ToUpper().TrimEnd('=');
        var output = new List<byte>(input.Length * 5 / 8);

        int bits = 0;
        int value = 0;

        foreach (char c in input)
        {
            int index = BASE32_ALPHABET.IndexOf(c);
            if (index < 0) continue; // تجاهل أي حرف غير صالح

            value = (value << 5) | index;
            bits += 5;

            // كل 8 bits = byte واحد
            if (bits >= 8)
            {
                output.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }

        return output.ToArray();
    }
}

// ═══════════════════════════════════════════════════════════════════════
// 📦 KeyData - بيانات المفتاح المُستخرَجة
// ═══════════════════════════════════════════════════════════════════════
internal class KeyData
{
    public string HardwareId { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string? CustomerName { get; set; }
}