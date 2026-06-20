namespace SmarterGros.Security
{
    /// <summary>
    /// الصلاحيات الافتراضية لكل دور في النظام
    /// تُستخدم عند بدء النظام لأول مرة (في DbSeeder)
    /// </summary>
    public static class RolePermissions
    {
        /// <summary>
        /// أسماء الأدوار في النظام
        /// </summary>
        public static class Roles
        {
            public const string SystemAdmin = "مدير النظام";
            public const string StockManager = "مدير المخزن";
            public const string SalesManager = "مسؤول المبيعات";
            public const string PurchasesManager = "مسؤول المشتريات";
            public const string RegularUser = "مستخدم عادي";
        }

        /// <summary>
        /// الحصول على الصلاحيات الافتراضية لدور معين
        /// </summary>
        public static List<string> GetDefaultPermissionsForRole(string roleName)
        {
            return roleName switch
            {
                Roles.SystemAdmin => GetSystemAdminPermissions(),
                Roles.StockManager => GetStockManagerPermissions(),
                Roles.SalesManager => GetSalesManagerPermissions(),
                Roles.PurchasesManager => GetPurchasesManagerPermissions(),
                Roles.RegularUser => GetRegularUserPermissions(),
                _ => new List<string>()
            };
        }

        // ═══════════════════════════════════════════════════
        // 👑 مدير النظام - كل الصلاحيات
        // ═══════════════════════════════════════════════════
        private static List<string> GetSystemAdminPermissions()
        {
            // مدير النظام يحصل على كل الصلاحيات تلقائياً
            return PermissionsList.GetAllPermissionKeys();
        }

        // ═══════════════════════════════════════════════════
        // 📦 مدير المخزن
        // ═══════════════════════════════════════════════════
        private static List<string> GetStockManagerPermissions()
        {
            return new List<string>
            {
                // لوحة التحكم
                Permissions.Dashboard.View,

                // المنتجات (كامل)
                Permissions.Products.View,
                Permissions.Products.Create,
                Permissions.Products.Edit,
                Permissions.Products.Delete,
                Permissions.Products.Export,

                // الفئات (كامل)
                Permissions.Categories.View,
                Permissions.Categories.Create,
                Permissions.Categories.Edit,
                Permissions.Categories.Delete,

                // حركات المخزون (كامل)
                Permissions.StockMovements.View,
                Permissions.StockMovements.Create,
                Permissions.StockMovements.Delete,
                Permissions.StockMovements.Export,

                // المشتريات (عرض فقط)
                Permissions.Purchases.View,

                // مرتجعات المشتريات (عرض فقط) ✅ جديد
                Permissions.PurchaseReturns.View,

                // الصندوق - عرض فقط
                Permissions.CashRegister.View,
                Permissions.CashRegister.ViewReports,


               // مرتجعات البيع (عرض فقط) ✅ جديد
                Permissions.SaleReturns.View,

                // الموردون (عرض فقط)
                Permissions.Suppliers.View,

                // العملاء (عرض فقط)
                Permissions.Customers.View,

                // التقارير
                Permissions.Reports.ViewStatistics,
                Permissions.Reports.ViewStockReport,
                Permissions.Reports.ExportToPdf,
                Permissions.Reports.ExportToExcel,

                // الإشعارات (شخصية فقط)
                Permissions.Notifications.View,

                // الدعم
                Permissions.Support.View
            };
        }

        // ═══════════════════════════════════════════════════
        // 💰 مسؤول المبيعات
        // ═══════════════════════════════════════════════════
        private static List<string> GetSalesManagerPermissions()
        {
            return new List<string>
            {
                // لوحة التحكم
                Permissions.Dashboard.View,

                // المبيعات (كامل) ✅ محدّث
                Permissions.Sales.View,
                Permissions.Sales.Create,
                Permissions.Sales.Edit,
                Permissions.Sales.Delete,
                Permissions.Sales.Cancel,           // ✅ جديد
                Permissions.Sales.ManagePayments,   // ✅ جديد
                Permissions.Sales.Print,
                Permissions.Sales.Export,
                Permissions.Sales.Duplicate,        // ✅ جديد
                Permissions.Sales.QuickSale,        // ✅ جديد

                // مرتجعات البيع (كامل) ✅ جديد
                Permissions.SaleReturns.View,
                Permissions.SaleReturns.Create,
                Permissions.SaleReturns.Edit,
                Permissions.SaleReturns.Delete,
                Permissions.SaleReturns.Cancel,
                Permissions.SaleReturns.Print,
                Permissions.SaleReturns.Export,

                // العملاء (كامل)
                Permissions.Customers.View,
                Permissions.Customers.Create,
                Permissions.Customers.Edit,
                Permissions.Customers.Delete,
                Permissions.Customers.ManagePayments,
                Permissions.Customers.Export,

                // المنتجات (عرض فقط)
                Permissions.Products.View,

                // الفئات (عرض فقط)
                Permissions.Categories.View,

                // حركات المخزون (عرض فقط)
                Permissions.StockMovements.View,

                // الصندوق - صلاحيات للمبيعات
                Permissions.CashRegister.View,
                Permissions.CashRegister.AddTransaction,
                Permissions.CashRegister.ViewReports,

                // التقارير
                Permissions.Reports.ViewStatistics,
                Permissions.Reports.ViewSalesReport,
                Permissions.Reports.ViewProfitReport,
                Permissions.Reports.ExportToPdf,
                Permissions.Reports.ExportToExcel,

                // الإشعارات (شخصية فقط)
                Permissions.Notifications.View,

                // الدعم
                Permissions.Support.View
            };
        }

        // ═══════════════════════════════════════════════════
        // 🛒 مسؤول المشتريات
        // ═══════════════════════════════════════════════════
        private static List<string> GetPurchasesManagerPermissions()
        {
            return new List<string>
            {
                // لوحة التحكم
                Permissions.Dashboard.View,

                // المشتريات (كامل) ✅ محدّث
                Permissions.Purchases.View,
                Permissions.Purchases.Create,
                Permissions.Purchases.Edit,
                Permissions.Purchases.Delete,
                Permissions.Purchases.Receive,           // ✅ جديد
                Permissions.Purchases.Cancel,            // ✅ جديد
                Permissions.Purchases.ManagePayments,    // ✅ جديد
                Permissions.Purchases.Print,
                Permissions.Purchases.Export,
                Permissions.Purchases.Duplicate,         // ✅ جديد

                // مرتجعات المشتريات (كامل) ✅ جديد
                Permissions.PurchaseReturns.View,
                Permissions.PurchaseReturns.Create,
                Permissions.PurchaseReturns.Edit,
                Permissions.PurchaseReturns.Delete,
                Permissions.PurchaseReturns.Cancel,
                Permissions.PurchaseReturns.Print,
                Permissions.PurchaseReturns.Export,

                // الموردون (كامل)
                Permissions.Suppliers.View,
                Permissions.Suppliers.Create,
                Permissions.Suppliers.Edit,
                Permissions.Suppliers.Delete,
                Permissions.Suppliers.ManagePayments,
                Permissions.Suppliers.Export,

                 // الصندوق - صلاحيات محدودة
                Permissions.CashRegister.View,
                Permissions.CashRegister.AddTransaction,
                Permissions.CashRegister.ViewReports,


                // المنتجات (عرض + تعديل - لتحديث الأسعار)
                Permissions.Products.View,
                Permissions.Products.Edit,

                // الفئات (عرض فقط)
                Permissions.Categories.View,

                // حركات المخزون (عرض فقط)
                Permissions.StockMovements.View,

                // التقارير
                Permissions.Reports.ViewStatistics,
                Permissions.Reports.ViewPurchasesReport,
                Permissions.Reports.ExportToPdf,
                Permissions.Reports.ExportToExcel,

                // الإشعارات (شخصية فقط)
                Permissions.Notifications.View,

                // الدعم
                Permissions.Support.View
            };
        }

        // ═══════════════════════════════════════════════════
        // 👤 مستخدم عادي - عرض فقط
        // ═══════════════════════════════════════════════════
        private static List<string> GetRegularUserPermissions()
        {
            return new List<string>
            {
                // لوحة التحكم
                Permissions.Dashboard.View,

                // كل شيء عرض فقط
                Permissions.Products.View,
                Permissions.Categories.View,
                Permissions.Suppliers.View,
                Permissions.Customers.View,
                Permissions.Purchases.View,
                Permissions.PurchaseReturns.View,  // ✅ جديد
                Permissions.Sales.View,
                Permissions.SaleReturns.View,  // ✅ جديد
                Permissions.StockMovements.View,

                // الإحصائيات الأساسية
                Permissions.Reports.ViewStatistics,

                // الصندوق - عرض فقط
                Permissions.CashRegister.View,

                // الإشعارات (شخصية فقط)
                Permissions.Notifications.View,

                // الدعم
                Permissions.Support.View
            };
        }

        /// <summary>
        /// الحصول على جميع الأدوار المتاحة
        /// </summary>
        public static List<string> GetAllRoles()
        {
            return new List<string>
            {
                Roles.SystemAdmin,
                Roles.StockManager,
                Roles.SalesManager,
                Roles.PurchasesManager,
                Roles.RegularUser
            };
        }

        /// <summary>
        /// التحقق إذا كان الدور هو مدير النظام (له كل الصلاحيات تلقائياً)
        /// </summary>
        public static bool IsSystemAdmin(string roleName)
        {
            return roleName == Roles.SystemAdmin;
        }
    }
}