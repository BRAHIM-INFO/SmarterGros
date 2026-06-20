using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Models;

namespace SmarterGros.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; } // ✅ جديد
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 جدول سجل النشاطات - ✅ جديد
        // ═══════════════════════════════════════════════════
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔄 جداول المرتجعات - ✅ جديد
        // ═══════════════════════════════════════════════════
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 جداول الصندوق - ✅ جديد
        // ═══════════════════════════════════════════════════
        public DbSet<CashRegister> CashRegisters { get; set; }

        // 🔐 نظام الترخيص
        public DbSet<LicenseInfo> LicenseInfos { get; set; }

        public DbSet<CashTransaction> CashTransactions { get; set; }
        public DbSet<DailyClosure> DailyClosures { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== Customer =====
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Balance)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0);
                entity.Property(e => e.InitialDebt)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsActive);
            });

            // ===== CustomerPayment =====
            builder.Entity<CustomerPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Payments)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sale)
                      .WithMany()
                      .HasForeignKey(e => e.SaleId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // ===== Product =====
            builder.Entity<Product>()
                .Property(p => p.PurchasePriceHT).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.PurchasePriceTTC).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.WholesalePriceHT).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.WholesalePriceTTC).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.SemiWholesalePriceHT).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.SemiWholesalePriceTTC).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.RetailPriceHT).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.RetailPriceTTC).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.TaxRate).HasColumnType("decimal(5,2)");
            builder.Entity<Product>()
                .Property(p => p.WholesaleMargin).HasColumnType("decimal(5,2)");
            builder.Entity<Product>()
                .Property(p => p.SemiWholesaleMargin).HasColumnType("decimal(5,2)");
            builder.Entity<Product>()
                .Property(p => p.RetailMargin).HasColumnType("decimal(5,2)");

            // ═══════════════════════════════════════════════════
            // 💰 Sale Configuration - محدّث
            // ═══════════════════════════════════════════════════
            builder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ تحويل Enums إلى int
                entity.Property(e => e.Status)
                      .HasConversion<int>();

                entity.Property(e => e.PaymentType)
                      .HasConversion<int>();

                entity.Property(e => e.PriceType)
                      .HasConversion<int>();

                // الحقول النصية
                entity.Property(e => e.InvoiceNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                // Decimal Precision
                entity.Property(e => e.SubTotal)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TaxAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Discount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DiscountPercentage)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PaidAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RemainingAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalCost)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalProfit)
                      .HasColumnType("decimal(18,2)");

                // علاقة مع Customer (اختياري)
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Indexes للأداء السريع
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.SaleDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaymentType);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.PriceType);
                entity.HasIndex(e => e.CreatedById);

                // Index مركّب
                entity.HasIndex(e => new { e.Status, e.SaleDate });
                entity.HasIndex(e => new { e.CustomerId, e.Status });
            });

            // ═══════════════════════════════════════════════════
            // 📦 SaleItem Configuration - محدّث
            // ═══════════════════════════════════════════════════
            builder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Decimal Precision
                entity.Property(e => e.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.UnitCost)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Profit)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Discount)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.TaxRate)
                      .HasColumnType("decimal(5,2)");

                // علاقة مع Sale
                entity.HasOne(e => e.Sale)
                      .WithMany(s => s.SaleItems)
                      .HasForeignKey(e => e.SaleId)
                      .OnDelete(DeleteBehavior.Cascade);

                // علاقة مع Product
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.SaleItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.SaleId);
                entity.HasIndex(e => e.ProductId);
            });

            // ═══════════════════════════════════════════════════
            // 🛒 Purchase - تحديث Configuration
            // ═══════════════════════════════════════════════════
            builder.Entity<Purchase>(entity =>
            {
                // ✅ تحويل Enums إلى int في قاعدة البيانات
                entity.Property(e => e.Status)
                      .HasConversion<int>();

                entity.Property(e => e.PaymentType)
                      .HasConversion<int>();

                // ⭐ الـ Nullable Enum يحتاج <int?>
                entity.Property(e => e.ShippingStatus)
                      .HasConversion<int?>();

                // Decimal Precision للحقول الجديدة
                entity.Property(e => e.PaidAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RemainingAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ShippingCost)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DiscountPercentage)
                      .HasColumnType("decimal(5,2)");

                // Indexes للأداء السريع
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaymentType);
                entity.HasIndex(e => e.PurchaseDate);
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.CreatedById);

                // Index مركّب للبحث المتقدم
                entity.HasIndex(e => new { e.Status, e.PurchaseDate });
                entity.HasIndex(e => new { e.SupplierId, e.Status });

                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);  // ← هذا موجود ✅


            });
             
            // ═══════════════════════════════════════════════════
            // 📦 PurchaseItem - تحديث Configuration
            // ═══════════════════════════════════════════════════
            builder.Entity<PurchaseItem>(entity =>
            {
                // Decimal Precision للحقول الجديدة
                entity.Property(e => e.Discount)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.TaxRate)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");


                // علاقة مع Purchase
                entity.HasOne(e => e.Purchase)
                      .WithMany(p => p.PurchaseItems)
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // علاقة مع Product
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.PurchaseItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.PurchaseId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.BatchNumber);
            });

            // ═══════════════════════════════════════════════════
            // 📦 Supplier
            // ═══════════════════════════════════════════════════ 
            builder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.InitialDebt)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0);

                entity.Property(e => e.Phone)
                      .HasMaxLength(20);

                entity.Property(e => e.Phone2)
                      .HasMaxLength(20);

                entity.Property(e => e.Email)
                      .HasMaxLength(100);

                entity.Property(e => e.Address)
                      .HasMaxLength(300);

                entity.Property(e => e.City)
                      .HasMaxLength(100);

                entity.Property(e => e.RC)
                      .HasMaxLength(50);

                entity.Property(e => e.NIF)
                      .HasMaxLength(50);

                entity.Property(e => e.AI)
                      .HasMaxLength(50);

                entity.Property(e => e.NIS)
                      .HasMaxLength(50);

                entity.Property(e => e.BankAccount)
                      .HasMaxLength(50);

                entity.Property(e => e.BankName)
                      .HasMaxLength(100);

                // Index للبحث السريع
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsActive);
            });

            // ===== SupplierPayment =====
            builder.Entity<SupplierPayment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Payments)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Purchase)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });
             
            // ═══════════════════════════════════════════════════
            // 📝 ActivityLog Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ Indexes للبحث السريع
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ActionType);
                entity.HasIndex(e => e.Module);
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.CreatedAt);

                // ✅ Index مركّب للبحث المتقدم
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.Module, e.ActionType, e.CreatedAt });
            });

            // ═══════════════════════════════════════════════════
            // 🔄 PurchaseReturn Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<PurchaseReturn>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ تحويل Enum إلى int في قاعدة البيانات
                entity.Property(e => e.RefundMethod)
                      .HasConversion<int>();

                // الحقول النصية
                entity.Property(e => e.ReturnNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                // Decimal Precision
                entity.Property(e => e.SubTotal)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TaxAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DeductedFromDebt)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CashRefunded)
                      .HasColumnType("decimal(18,2)");

                // علاقة مع Purchase (الفاتورة الأصلية)
                entity.HasOne(e => e.Purchase)
                      .WithMany(p => p.Returns)
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // علاقة مع Supplier
                entity.HasOne(e => e.Supplier)
                      .WithMany()
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.NoAction); 

                // Indexes للبحث السريع
                entity.HasIndex(e => e.ReturnNumber).IsUnique();
                entity.HasIndex(e => e.ReturnDate);
                entity.HasIndex(e => e.PurchaseId);
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.IsCancelled);
                entity.HasIndex(e => e.RefundMethod);

                // Index مركّب
                entity.HasIndex(e => new { e.SupplierId, e.ReturnDate });
            });
             
            // ═══════════════════════════════════════════════════
            // 📦 PurchaseReturnItem Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<PurchaseReturnItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Decimal Precision
                entity.Property(e => e.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TaxRate)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                // علاقة مع PurchaseReturn (الأم)
                entity.HasOne(e => e.PurchaseReturn)
                      .WithMany(r => r.ReturnItems)
                      .HasForeignKey(e => e.PurchaseReturnId)
                      .OnDelete(DeleteBehavior.Cascade);

                // علاقة مع PurchaseItem (البند الأصلي - للتتبع)
                entity.HasOne(e => e.PurchaseItem)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                // علاقة مع Product
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.PurchaseReturnId);
                entity.HasIndex(e => e.PurchaseItemId);
                entity.HasIndex(e => e.ProductId);
            });


            // ═══════════════════════════════════════════════════
            // 💰 CashRegister Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<CashRegister>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // Decimal Precision
                entity.Property(e => e.OpeningBalance)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CurrentBalance)
                      .HasColumnType("decimal(18,2)");

                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDefault);
            });

            // ═══════════════════════════════════════════════════
            // 🔄 CashTransaction Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<CashTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ تحويل Enums إلى int
                entity.Property(e => e.Type)
                      .HasConversion<int>();

                entity.Property(e => e.Category)
                      .HasConversion<int>();

                entity.Property(e => e.PaymentMethod)
                      .HasConversion<int>();

                // الحقول النصية
                entity.Property(e => e.TransactionNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Description)
                      .IsRequired()
                      .HasMaxLength(500);

                // Decimal Precision
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.BalanceBefore)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.BalanceAfter)
                      .HasColumnType("decimal(18,2)");

                // علاقة مع CashRegister
                entity.HasOne(e => e.CashRegister)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(e => e.CashRegisterId)
                      .OnDelete(DeleteBehavior.Restrict);

                // علاقة مع Supplier (اختيارية)
                entity.HasOne(e => e.Supplier)
                      .WithMany()
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // علاقة مع Customer (اختيارية)
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // علاقة مع DailyClosure (اختيارية)
                entity.HasOne(e => e.DailyClosure)
                      .WithMany(d => d.Transactions)
                      .HasForeignKey(e => e.DailyClosureId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Indexes للبحث السريع
                entity.HasIndex(e => e.TransactionNumber).IsUnique();
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CashRegisterId);
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.IsCancelled);
                entity.HasIndex(e => e.ReferenceType);

                // Index مركّب للتقارير
                entity.HasIndex(e => new { e.CashRegisterId, e.TransactionDate });
                entity.HasIndex(e => new { e.Type, e.TransactionDate });
                entity.HasIndex(e => new { e.Category, e.TransactionDate });
            });

            // ═══════════════════════════════════════════════════
            // 🔒 DailyClosure Configuration - ✅ جديد
            // ═══════════════════════════════════════════════════
            builder.Entity<DailyClosure>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Decimal Precision
                entity.Property(e => e.OpeningBalance)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalIncome)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalExpense)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ExpectedBalance)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ActualBalance)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Difference)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CoinsAmount)
                      .HasColumnType("decimal(18,2)");

                // علاقة مع CashRegister
                entity.HasOne(e => e.CashRegister)
                      .WithMany(c => c.DailyClosures)
                      .HasForeignKey(e => e.CashRegisterId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.ClosureDate);
                entity.HasIndex(e => e.CashRegisterId);
                entity.HasIndex(e => e.IsClosed);

                // Index مركّب - لا يمكن إغلاق نفس اليوم مرتين لنفس الصندوق
                entity.HasIndex(e => new { e.CashRegisterId, e.ClosureDate })
                      .IsUnique();
            });

        }
    }
}