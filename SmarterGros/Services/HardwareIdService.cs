// ═══════════════════════════════════════════════════════════════════════
// 📁 Services/HardwareIdService.cs
// ═══════════════════════════════════════════════════════════════════════
// ✅ نسخة موحَّدة - نفس الـ Hardware ID للعرض والتحقق
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using SmarterGros.Services;

namespace SmarterGros.Services;

//public interface IHardwareIdServices
//{
//    /// <summary>
//    /// الحصول على Hardware ID موحّد بصيغة: XXXX-XXXX-XXXX-XXXX
//    /// يُستخدم للعرض والتحقق معاً (موحَّد)
//    /// </summary>
//    string GetHardwareId();
//}

public class HardwareIdService : IHardwareIdService
{
    private static string? _cachedHardwareId;
    private static readonly object _lock = new();

    /// <summary>
    /// الحصول على Hardware ID الرسمي للنظام
    /// مثال: O0CF-2YMR-1E7D-ZOE1
    /// </summary>
    public string GetHardwareId()
    {
        // ✅ Cache للأداء (الـ Hardware ID لا يتغير)
        if (_cachedHardwareId != null)
            return _cachedHardwareId;

        lock (_lock)
        {
            if (_cachedHardwareId != null)
                return _cachedHardwareId;

            _cachedHardwareId = GenerateHardwareId();
            return _cachedHardwareId;
        }
    }

    
    private string GenerateHardwareId()
    {
        try
        {
            // 1️⃣ جمع معرّفات الجهاز
            var cpuId = GetCpuId();
            var motherboardId = GetMotherboardId();
            var diskId = GetDiskId();

            // 2️⃣ دمج المعرّفات
            var combined = $"{cpuId}|{motherboardId}|{diskId}|SmarterGros";

            // 3️⃣ SHA256 Hash
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);

            
            // 4️⃣ تحويل إلى Base32 وأخذ 16 حرف
            var base32 = ToBase32(hash).Substring(0, 44);
            return base32;
            // 5️⃣ تنسيق: XXXX-XXXX-XXXX-XXXX
            //return $"{base32.Substring(0, 4)}-{base32.Substring(4, 4)}-{base32.Substring(8, 4)}-{base32.Substring(12, 4)}";
        }
        catch (Exception)
        {
            return "ERROR-XXXX-XXXX-XXXX";
        }
    }

    // ✅ أضف هذه الدالة المفقودة
    public string GetDisplayHardwareId()
    {
        return GetHardwareId();  // نفس القيمة - موحَّد
    }

    // ═══════════════════════════════════════════════════
    // 🔧 Helpers - جمع معرّفات الجهاز
    // ═══════════════════════════════════════════════════

    private string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                var id = item["ProcessorId"]?.ToString();
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }
        }
        catch { }
        return "DEFAULT_CPU";
    }

    private string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var item in searcher.Get())
            {
                var sn = item["SerialNumber"]?.ToString();
                if (!string.IsNullOrWhiteSpace(sn))
                    return sn;
            }
        }
        catch { }
        return "DEFAULT_BOARD";
    }

    private string GetDiskId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT SerialNumber FROM Win32_DiskDrive WHERE Index = 0");
            foreach (var item in searcher.Get())
            {
                var sn = item["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(sn))
                    return sn;
            }
        }
        catch { }
        return "DEFAULT_DISK";
    }

    /// <summary>
    /// تحويل bytes إلى Base32
    /// </summary>
    private static string ToBase32(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var sb = new StringBuilder();
        int bits = 0;
        int value = 0;

        foreach (byte b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(alphabet[(value >> (bits - 5)) & 0x1F]);
                bits -= 5;
            }
        }

        if (bits > 0)
            sb.Append(alphabet[(value << (5 - bits)) & 0x1F]);

        return sb.ToString();
    }
}