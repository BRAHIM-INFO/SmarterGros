namespace SmarterGros.Security
{
    /// <summary>
    /// قائمة شاملة بجميع صلاحيات النظام
    /// مع أسماء عربية وتصنيفات لعرضها في واجهة الإدارة
    /// </summary>
    public static class PermissionsList
    {
        /// <summary>
        /// نموذج تفاصيل الصلاحية الواحدة
        /// </summary>
        public class PermissionInfo
        {
            public string Key { get; set; } = string.Empty;        // مفتاح الصلاحية (مثل: Products.View)
            public string DisplayName { get; set; } = string.Empty; // الاسم بالعربية
            public string Description { get; set; } = string.Empty; // وصف مختصر
        }

        /// <summary>
        /// نموذج مجموعة الصلاحيات (حسب القسم)
        /// </summary>
        public class PermissionGroup
        {
            public string GroupName { get; set; } = string.Empty;       // اسم القسم
            public string GroupIcon { get; set; } = string.Empty;       // أيقونة Font Awesome
            public string GroupColor { get; set; } = string.Empty;      // اللون
            public List<PermissionInfo> Permissions { get; set; } = new();
        }

        /// <summary>
        /// الحصول على جميع الصلاحيات منظمة حسب الأقسام
        /// </summary>
        public static List<PermissionGroup> GetAllPermissions()
        {
            return new List<PermissionGroup>
            {
                // ========== لوحة التحكم ==========
                new PermissionGroup
                {
                    GroupName = "لوحة التحكم",
                    GroupIcon = "fa-th-large",
                    GroupColor = "#0E4D3A",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Dashboard.View, DisplayName = "عرض لوحة التحكم", Description = "الوصول إلى الصفحة الرئيسية" }
                    }
                },

                // ========== المنتجات ==========
                new PermissionGroup
                {
                    GroupName = "المنتجات",
                    GroupIcon = "fa-box",
                    GroupColor = "#1976d2",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Products.View, DisplayName = "عرض المنتجات", Description = "عرض قائمة المنتجات وتفاصيلها" },
                        new() { Key = Permissions.Products.Create, DisplayName = "إضافة منتج", Description = "إضافة منتجات جديدة" },
                        new() { Key = Permissions.Products.Edit, DisplayName = "تعديل المنتجات", Description = "تعديل بيانات المنتجات" },
                        new() { Key = Permissions.Products.Delete, DisplayName = "حذف المنتجات", Description = "حذف المنتجات نهائياً" },
                        new() { Key = Permissions.Products.Export, DisplayName = "تصدير المنتجات", Description = "تصدير قائمة المنتجات" }
                    }
                },

                // ========== الفئات ==========
                new PermissionGroup
                {
                    GroupName = "الفئات",
                    GroupIcon = "fa-tags",
                    GroupColor = "#7b1fa2",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Categories.View, DisplayName = "عرض الفئات", Description = "عرض قائمة الفئات" },
                        new() { Key = Permissions.Categories.Create, DisplayName = "إضافة فئة", Description = "إضافة فئات جديدة" },
                        new() { Key = Permissions.Categories.Edit, DisplayName = "تعديل الفئات", Description = "تعديل بيانات الفئات" },
                        new() { Key = Permissions.Categories.Delete, DisplayName = "حذف الفئات", Description = "حذف الفئات نهائياً" }
                    }
                },

                // ========== الموردون ==========
                new PermissionGroup
                {
                    GroupName = "الموردون",
                    GroupIcon = "fa-truck",
                    GroupColor = "#f57c00",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Suppliers.View, DisplayName = "عرض الموردين", Description = "عرض قائمة الموردين" },
                        new() { Key = Permissions.Suppliers.Create, DisplayName = "إضافة مورد", Description = "إضافة موردين جدد" },
                        new() { Key = Permissions.Suppliers.Edit, DisplayName = "تعديل الموردين", Description = "تعديل بيانات الموردين" },
                        new() { Key = Permissions.Suppliers.Delete, DisplayName = "حذف الموردين", Description = "حذف الموردين نهائياً" },
                        new() { Key = Permissions.Suppliers.ManagePayments, DisplayName = "إدارة المدفوعات", Description = "تسجيل وإدارة دفعات الموردين" },
                        new() { Key = Permissions.Suppliers.Export, DisplayName = "تصدير الموردين", Description = "تصدير قائمة الموردين" }
                    }
                },

                // ========== العملاء ==========
                new PermissionGroup
                {
                    GroupName = "العملاء",
                    GroupIcon = "fa-users",
                    GroupColor = "#0288d1",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Customers.View, DisplayName = "عرض العملاء", Description = "عرض قائمة العملاء" },
                        new() { Key = Permissions.Customers.Create, DisplayName = "إضافة عميل", Description = "إضافة عملاء جدد" },
                        new() { Key = Permissions.Customers.Edit, DisplayName = "تعديل العملاء", Description = "تعديل بيانات العملاء" },
                        new() { Key = Permissions.Customers.Delete, DisplayName = "حذف العملاء", Description = "حذف العملاء نهائياً" },
                        new() { Key = Permissions.Customers.ManagePayments, DisplayName = "إدارة المدفوعات", Description = "تسجيل وإدارة دفعات العملاء" },
                        new() { Key = Permissions.Customers.Export, DisplayName = "تصدير العملاء", Description = "تصدير قائمة العملاء" }
                    }
                },

               // ========== المشتريات ==========
                 new PermissionGroup
                 {
                     GroupName = "المشتريات",
                     GroupIcon = "fa-shopping-cart",
                     GroupColor = "#388e3c",
                     Permissions = new List<PermissionInfo>
                     {
                         new() { Key = Permissions.Purchases.View, DisplayName = "عرض المشتريات", Description = "عرض فواتير المشتريات" },
                         new() { Key = Permissions.Purchases.Create, DisplayName = "إنشاء فاتورة شراء", Description = "تسجيل فواتير شراء جديدة" },
                         new() { Key = Permissions.Purchases.Edit, DisplayName = "تعديل المشتريات", Description = "تعديل فواتير الشراء (مسودة فقط)" },
                         new() { Key = Permissions.Purchases.Delete, DisplayName = "حذف المشتريات", Description = "حذف فواتير الشراء (مسودة فقط)" },
                         new() { Key = Permissions.Purchases.Receive, DisplayName = "استلام الفاتورة", Description = "تأكيد استلام البضاعة وتأثير المخزون" },
                         new() { Key = Permissions.Purchases.Cancel, DisplayName = "إلغاء الفاتورة", Description = "إلغاء فاتورة مستلمة وعكس التأثيرات" },
                         new() { Key = Permissions.Purchases.ManagePayments, DisplayName = "إدارة الدفعات", Description = "تسجيل دفعات على فواتير الكريدي" },
                         new() { Key = Permissions.Purchases.Print, DisplayName = "طباعة الفواتير", Description = "طباعة فواتير الشراء" },
                         new() { Key = Permissions.Purchases.Export, DisplayName = "تصدير المشتريات", Description = "تصدير فواتير الشراء" },
                         new() { Key = Permissions.Purchases.Duplicate, DisplayName = "نسخ فاتورة", Description = "إنشاء فاتورة جديدة من فاتورة موجودة" }
                     }
                 },
              
                 // ========== مرتجعات المشتريات - ✅ جديد ==========
                 new PermissionGroup
                 {
                     GroupName = "مرتجعات المشتريات",
                     GroupIcon = "fa-rotate-left",
                     GroupColor = "#e91e63",
                     Permissions = new List<PermissionInfo>
                     {
                         new() { Key = Permissions.PurchaseReturns.View, DisplayName = "عرض المرتجعات", Description = "عرض قائمة مرتجعات الشراء" },
                         new() { Key = Permissions.PurchaseReturns.Create, DisplayName = "إنشاء مرتجع", Description = "إرجاع منتجات للمورد" },
                         new() { Key = Permissions.PurchaseReturns.Edit, DisplayName = "تعديل المرتجعات", Description = "تعديل بيانات المرتجع" },
                         new() { Key = Permissions.PurchaseReturns.Delete, DisplayName = "حذف المرتجعات", Description = "حذف مرتجع نهائياً (خطير)" },
                         new() { Key = Permissions.PurchaseReturns.Cancel, DisplayName = "إلغاء المرتجع", Description = "إلغاء مرتجع وعكس التأثيرات" },
                         new() { Key = Permissions.PurchaseReturns.Print, DisplayName = "طباعة المرتجعات", Description = "طباعة فواتير المرتجعات" },
                         new() { Key = Permissions.PurchaseReturns.Export, DisplayName = "تصدير المرتجعات", Description = "تصدير قائمة المرتجعات" }
                     }
                 },

     
                 // ========== الصندوق (Caisse) ==========
                new PermissionGroup
                {
                    GroupName = "الصندوق (Caisse)",
                    GroupIcon = "fa-cash-register",
                    GroupColor = "#28a745",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.CashRegister.View, DisplayName = "عرض الصندوق", Description = "عرض الرصيد والحركات اليومية" },
                        new() { Key = Permissions.CashRegister.AddTransaction, DisplayName = "إضافة حركة", Description = "تسجيل وارد أو صادر يدوياً" },
                        new() { Key = Permissions.CashRegister.EditTransaction, DisplayName = "تعديل الحركات", Description = "تعديل حركات سابقة (خطر)" },
                        new() { Key = Permissions.CashRegister.CancelTransaction, DisplayName = "إلغاء الحركات", Description = "إلغاء حركة مع تسجيل السبب" },
                        new() { Key = Permissions.CashRegister.DeleteTransaction, DisplayName = "حذف الحركات", Description = "حذف نهائي للحركات (خطير)" },
                        new() { Key = Permissions.CashRegister.CloseDaily, DisplayName = "إغلاق اليوم", Description = "تنفيذ الجرد اليومي وإغلاق الصندوق" },
                        new() { Key = Permissions.CashRegister.ViewReports, DisplayName = "عرض التقارير", Description = "عرض تقارير الصندوق المتنوعة" },
                        new() { Key = Permissions.CashRegister.ExportReports, DisplayName = "تصدير التقارير", Description = "تصدير التقارير لـ Excel و PDF" },
                        new() { Key = Permissions.CashRegister.PrintReports, DisplayName = "طباعة التقارير", Description = "طباعة التقارير اليومية والشهرية" },
                        new() { Key = Permissions.CashRegister.ManageRegister, DisplayName = "إدارة الصندوق", Description = "تعديل إعدادات الصندوق الأساسية" },
                        new() { Key = Permissions.CashRegister.SetOpeningBalance, DisplayName = "تحديد الرصيد الافتتاحي", Description = "تعديل الرصيد الافتتاحي (مرة واحدة فقط)" }
                    }
                },

                            // ========== المبيعات ==========
            new PermissionGroup
            {
                GroupName = "المبيعات",
                GroupIcon = "fa-cash-register",
                GroupColor = "#d32f2f",
                Permissions = new List<PermissionInfo>
                {
                    new() { Key = Permissions.Sales.View, DisplayName = "عرض المبيعات", Description = "عرض فواتير المبيعات" },
                    new() { Key = Permissions.Sales.Create, DisplayName = "إنشاء فاتورة بيع", Description = "تسجيل فواتير بيع جديدة" },
                    new() { Key = Permissions.Sales.Edit, DisplayName = "تعديل المبيعات", Description = "تعديل فواتير البيع (مسودة فقط)" },
                    new() { Key = Permissions.Sales.Delete, DisplayName = "حذف المبيعات", Description = "حذف فواتير البيع (مسودة فقط)" },
                    new() { Key = Permissions.Sales.Cancel, DisplayName = "إلغاء الفاتورة", Description = "إلغاء فاتورة مع عكس التأثيرات على المخزون والصندوق" },
                    new() { Key = Permissions.Sales.ManagePayments, DisplayName = "إدارة الدفعات", Description = "تسجيل دفعات على فواتير الكريدي" },
                    new() { Key = Permissions.Sales.Print, DisplayName = "طباعة الفواتير", Description = "طباعة فواتير البيع" },
                    new() { Key = Permissions.Sales.Export, DisplayName = "تصدير المبيعات", Description = "تصدير فواتير البيع" },
                    new() { Key = Permissions.Sales.Duplicate, DisplayName = "نسخ فاتورة", Description = "إنشاء فاتورة جديدة من فاتورة موجودة" },
                    new() { Key = Permissions.Sales.QuickSale, DisplayName = "البيع السريع (POS)", Description = "استخدام شاشة البيع السريع" }
                }
            },

            // ========== مرتجعات البيع - جديد! ==========
            new PermissionGroup
            {
                GroupName = "مرتجعات البيع",
                GroupIcon = "fa-rotate-left",
                GroupColor = "#c2185b",
                Permissions = new List<PermissionInfo>
                {
                    new() { Key = Permissions.SaleReturns.View, DisplayName = "عرض المرتجعات", Description = "عرض قائمة مرتجعات البيع" },
                    new() { Key = Permissions.SaleReturns.Create, DisplayName = "إنشاء مرتجع", Description = "إرجاع منتجات من العميل" },
                    new() { Key = Permissions.SaleReturns.Edit, DisplayName = "تعديل المرتجعات", Description = "تعديل بيانات المرتجع" },
                    new() { Key = Permissions.SaleReturns.Delete, DisplayName = "حذف المرتجعات", Description = "حذف مرتجع نهائياً (خطير)" },
                    new() { Key = Permissions.SaleReturns.Cancel, DisplayName = "إلغاء المرتجع", Description = "إلغاء مرتجع وعكس التأثيرات" },
                    new() { Key = Permissions.SaleReturns.Print, DisplayName = "طباعة المرتجعات", Description = "طباعة فواتير المرتجعات" },
                    new() { Key = Permissions.SaleReturns.Export, DisplayName = "تصدير المرتجعات", Description = "تصدير قائمة المرتجعات" }
                }
            },

                // ========== حركات المخزون ==========
                new PermissionGroup
                {
                    GroupName = "حركات المخزون",
                    GroupIcon = "fa-exchange-alt",
                    GroupColor = "#00796b",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.StockMovements.View, DisplayName = "عرض الحركات", Description = "عرض حركات المخزون" },
                        new() { Key = Permissions.StockMovements.Create, DisplayName = "تسجيل حركة", Description = "تسجيل حركات مخزون يدوية" },
                        new() { Key = Permissions.StockMovements.Delete, DisplayName = "حذف الحركات", Description = "حذف حركات المخزون" },
                        new() { Key = Permissions.StockMovements.Export, DisplayName = "تصدير الحركات", Description = "تصدير سجل الحركات" }
                    }
                },

                // ========== التقارير ==========
                new PermissionGroup
                {
                    GroupName = "التقارير والإحصائيات",
                    GroupIcon = "fa-chart-bar",
                    GroupColor = "#5e35b1",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Reports.ViewStatistics, DisplayName = "عرض الإحصائيات", Description = "عرض إحصائيات النظام" },
                        new() { Key = Permissions.Reports.ViewSalesReport, DisplayName = "تقرير المبيعات", Description = "عرض تقارير المبيعات" },
                        new() { Key = Permissions.Reports.ViewPurchasesReport, DisplayName = "تقرير المشتريات", Description = "عرض تقارير المشتريات" },
                        new() { Key = Permissions.Reports.ViewStockReport, DisplayName = "تقرير المخزون", Description = "عرض تقارير المخزون" },
                        new() { Key = Permissions.Reports.ViewProfitReport, DisplayName = "تقرير الأرباح", Description = "عرض تقارير الأرباح والخسائر" },
                        new() { Key = Permissions.Reports.ExportToPdf, DisplayName = "تصدير PDF", Description = "تصدير التقارير كملفات PDF" },
                        new() { Key = Permissions.Reports.ExportToExcel, DisplayName = "تصدير Excel", Description = "تصدير التقارير كملفات Excel" }
                    }
                },

                // ========== إعدادات المؤسسة ==========
                new PermissionGroup
                {
                    GroupName = "إعدادات المؤسسة",
                    GroupIcon = "fa-building",
                    GroupColor = "#455a64",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.CompanySettings.View, DisplayName = "عرض الإعدادات", Description = "عرض بيانات المؤسسة" },
                        new() { Key = Permissions.CompanySettings.Edit, DisplayName = "تعديل الإعدادات", Description = "تعديل بيانات المؤسسة والشعار" }
                    }
                },

                // ========== المستخدمون ==========
                new PermissionGroup
                {
                    GroupName = "إدارة المستخدمين",
                    GroupIcon = "fa-users-cog",
                    GroupColor = "#c2185b",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Users.View, DisplayName = "عرض المستخدمين", Description = "عرض قائمة المستخدمين" },
                        new() { Key = Permissions.Users.Create, DisplayName = "إضافة مستخدم", Description = "إضافة مستخدمين جدد" },
                        new() { Key = Permissions.Users.Edit, DisplayName = "تعديل المستخدمين", Description = "تعديل بيانات المستخدمين" },
                        new() { Key = Permissions.Users.Delete, DisplayName = "حذف المستخدمين", Description = "حذف المستخدمين نهائياً" },
                        new() { Key = Permissions.Users.ChangePassword, DisplayName = "تغيير كلمات المرور", Description = "تغيير كلمات مرور المستخدمين" },
                        new() { Key = Permissions.Users.ToggleActive, DisplayName = "تفعيل/تعطيل المستخدمين", Description = "تفعيل أو تعطيل حسابات المستخدمين" }
                    }
                },

                // ========== الصلاحيات ==========
                new PermissionGroup
                {
                    GroupName = "إدارة الصلاحيات",
                    GroupIcon = "fa-user-shield",
                    GroupColor = "#6a1b9a",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Roles.View, DisplayName = "عرض الأدوار", Description = "عرض الأدوار والصلاحيات" },
                        new() { Key = Permissions.Roles.ManagePermissions, DisplayName = "إدارة الصلاحيات", Description = "تعديل صلاحيات الأدوار" }
                    }
                },

                // ========== قاعدة البيانات ==========
                new PermissionGroup
                {
                    GroupName = "قاعدة البيانات",
                    GroupIcon = "fa-database",
                    GroupColor = "#e64a19",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Database.View, DisplayName = "عرض إحصائيات قاعدة البيانات", Description = "عرض حالة قاعدة البيانات" },
                        new() { Key = Permissions.Database.Backup, DisplayName = "نسخ احتياطي", Description = "إنشاء نسخ احتياطية" },
                        new() { Key = Permissions.Database.Restore, DisplayName = "استعادة نسخة", Description = "استعادة نسخة احتياطية" },
                        new() { Key = Permissions.Database.Clear, DisplayName = "مسح البيانات", Description = "مسح جميع البيانات (خطير)" }
                    }
                },

                // ========== سجل النشاطات ==========
                new PermissionGroup
                {
                    GroupName = "سجل النشاطات",
                    GroupIcon = "fa-history",
                    GroupColor = "#5d4037",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.ActivityLogs.View, DisplayName = "عرض السجل", Description = "عرض سجل نشاطات المستخدمين" },
                        new() { Key = Permissions.ActivityLogs.Delete, DisplayName = "حذف السجلات", Description = "حذف سجلات النشاطات" },
                        new() { Key = Permissions.ActivityLogs.Export, DisplayName = "تصدير السجل", Description = "تصدير سجل النشاطات" }
                    }
                },

                // ========== الإشعارات ==========
                new PermissionGroup
                {
                    GroupName = "الإشعارات",
                    GroupIcon = "fa-bell",
                    GroupColor = "#ffa000",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Notifications.View, DisplayName = "عرض الإشعارات", Description = "عرض الإشعارات الشخصية" },
                        new() { Key = Permissions.Notifications.SendToAll, DisplayName = "إرسال إشعارات", Description = "إرسال إشعارات لجميع المستخدمين" },
                        new() { Key = Permissions.Notifications.Delete, DisplayName = "حذف الإشعارات", Description = "حذف الإشعارات" }
                    }
                },

                // ========== الدعم الفني ==========
                new PermissionGroup
                {
                    GroupName = "الدعم الفني",
                    GroupIcon = "fa-headset",
                    GroupColor = "#00897b",
                    Permissions = new List<PermissionInfo>
                    {
                        new() { Key = Permissions.Support.View, DisplayName = "عرض الدعم الفني", Description = "الوصول لصفحة الدعم الفني" }
                    }
                }
            };
        }

        /// <summary>
        /// الحصول على جميع الصلاحيات كقائمة مسطحة (بدون تجميع)
        /// </summary>
        public static List<string> GetAllPermissionKeys()
        {
            var allPermissions = new List<string>();
            foreach (var group in GetAllPermissions())
            {
                foreach (var permission in group.Permissions)
                {
                    allPermissions.Add(permission.Key);
                }
            }
            return allPermissions;
        }

        /// <summary>
        /// الحصول على الاسم العربي لصلاحية معينة
        /// </summary>
        public static string GetDisplayName(string permissionKey)
        {
            foreach (var group in GetAllPermissions())
            {
                var permission = group.Permissions.FirstOrDefault(p => p.Key == permissionKey);
                if (permission != null)
                    return permission.DisplayName;
            }
            return permissionKey;
        }
    }
}