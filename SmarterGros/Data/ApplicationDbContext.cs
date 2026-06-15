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
             

            // ===== Purchase =====
            builder.Entity<Purchase>()
                .Property(p => p.SubTotal).HasColumnType("decimal(18,2)");
            builder.Entity<Purchase>()
                .Property(p => p.TaxAmount).HasColumnType("decimal(18,2)");
            builder.Entity<Purchase>()
                .Property(p => p.Discount).HasColumnType("decimal(18,2)");
            builder.Entity<Purchase>()
                .Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");


            builder.Entity<PurchaseItem>()
                .Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<PurchaseItem>()
                .Property(p => p.TotalPrice).HasColumnType("decimal(18,2)");
           
            builder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.InvoiceNumber)
                      .HasMaxLength(50);

                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Sale =====
            builder.Entity<Sale>()
                .Property(p => p.SubTotal).HasColumnType("decimal(18,2)");
            builder.Entity<Sale>()
                .Property(p => p.TaxAmount).HasColumnType("decimal(18,2)");
            builder.Entity<Sale>()
                .Property(p => p.Discount).HasColumnType("decimal(18,2)");
            builder.Entity<Sale>()
                .Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");

            builder.Entity<SaleItem>()
                .Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<SaleItem>()
                .Property(p => p.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Entity<SaleItem>()
                .Property(p => p.Profit).HasColumnType("decimal(18,2)");

            builder.Entity<Customer>()
                .Property(p => p.Balance).HasColumnType("decimal(18,2)");

            // ===== Supplier =====
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


        }
    }
}