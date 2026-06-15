namespace SmarterGros.Security
{
    /// <summary>
    /// جميع صلاحيات النظام
    /// كل صلاحية تُكتب بصيغة: "القسم.العملية"
    /// مثال: "Products.View" = صلاحية عرض المنتجات
    /// </summary>
    public static class Permissions
    {
        // ========================================
        // 📊 لوحة التحكم
        // ========================================
        public static class Dashboard
        {
            public const string View = "Dashboard.View";
        }

        // ========================================
        // 📦 المنتجات
        // ========================================
        public static class Products
        {
            public const string View = "Products.View";
            public const string Create = "Products.Create";
            public const string Edit = "Products.Edit";
            public const string Delete = "Products.Delete";
            public const string Export = "Products.Export";
        }

        // ========================================
        // 🏷️ الفئات
        // ========================================
        public static class Categories
        {
            public const string View = "Categories.View";
            public const string Create = "Categories.Create";
            public const string Edit = "Categories.Edit";
            public const string Delete = "Categories.Delete";
        }

        // ========================================
        // 🚚 الموردون
        // ========================================
        public static class Suppliers
        {
            public const string View = "Suppliers.View";
            public const string Create = "Suppliers.Create";
            public const string Edit = "Suppliers.Edit";
            public const string Delete = "Suppliers.Delete";
            public const string ManagePayments = "Suppliers.ManagePayments";
            public const string Export = "Suppliers.Export";
        }

        // ========================================
        // 👥 العملاء
        // ========================================
        public static class Customers
        {
            public const string View = "Customers.View";
            public const string Create = "Customers.Create";
            public const string Edit = "Customers.Edit";
            public const string Delete = "Customers.Delete";
            public const string ManagePayments = "Customers.ManagePayments";
            public const string Export = "Customers.Export";
        }

        // ========================================
        // 🛒 المشتريات
        // ========================================
        public static class Purchases
        {
            public const string View = "Purchases.View";
            public const string Create = "Purchases.Create";
            public const string Edit = "Purchases.Edit";
            public const string Delete = "Purchases.Delete";
            public const string Print = "Purchases.Print";
            public const string Export = "Purchases.Export";
        }

        // ========================================
        // 💰 المبيعات
        // ========================================
        public static class Sales
        {
            public const string View = "Sales.View";
            public const string Create = "Sales.Create";
            public const string Edit = "Sales.Edit";
            public const string Delete = "Sales.Delete";
            public const string Print = "Sales.Print";
            public const string Export = "Sales.Export";
        }

        // ========================================
        // 📊 حركات المخزون
        // ========================================
        public static class StockMovements
        {
            public const string View = "StockMovements.View";
            public const string Create = "StockMovements.Create";
            public const string Delete = "StockMovements.Delete";
            public const string Export = "StockMovements.Export";
        }

        // ========================================
        // 📈 التقارير والإحصائيات
        // ========================================
        public static class Reports
        {
            public const string ViewSalesReport = "Reports.ViewSalesReport";
            public const string ViewPurchasesReport = "Reports.ViewPurchasesReport";
            public const string ViewStockReport = "Reports.ViewStockReport";
            public const string ViewProfitReport = "Reports.ViewProfitReport";
            public const string ViewStatistics = "Reports.ViewStatistics";
            public const string ExportToPdf = "Reports.ExportToPdf";
            public const string ExportToExcel = "Reports.ExportToExcel";
        }

        // ========================================
        // ⚙️ إعدادات المؤسسة
        // ========================================
        public static class CompanySettings
        {
            public const string View = "CompanySettings.View";
            public const string Edit = "CompanySettings.Edit";
        }

        // ========================================
        // 👤 إدارة المستخدمين
        // ========================================
        public static class Users
        {
            public const string View = "Users.View";
            public const string Create = "Users.Create";
            public const string Edit = "Users.Edit";
            public const string Delete = "Users.Delete";
            public const string ChangePassword = "Users.ChangePassword";
            public const string ToggleActive = "Users.ToggleActive";
        }

        // ========================================
        // 🔐 إدارة الصلاحيات
        // ========================================
        public static class Roles
        {
            public const string View = "Roles.View";
            public const string ManagePermissions = "Roles.ManagePermissions";
        }

        // ========================================
        // 💾 قاعدة البيانات
        // ========================================
        public static class Database
        {
            public const string View = "Database.View";
            public const string Backup = "Database.Backup";
            public const string Restore = "Database.Restore";
            public const string Clear = "Database.Clear";
        }

        // ========================================
        // 📝 سجل النشاطات
        // ========================================
        public static class ActivityLogs
        {
            public const string View = "ActivityLogs.View";
            public const string Delete = "ActivityLogs.Delete";
            public const string Export = "ActivityLogs.Export";
        }

        // ========================================
        // 🔔 الإشعارات
        // ========================================
        public static class Notifications
        {
            public const string View = "Notifications.View";
            public const string SendToAll = "Notifications.SendToAll";
            public const string Delete = "Notifications.Delete";
        }

        // ========================================
        // 🎧 الدعم الفني
        // ========================================
        public static class Support
        {
            public const string View = "Support.View";
        }
    }
}