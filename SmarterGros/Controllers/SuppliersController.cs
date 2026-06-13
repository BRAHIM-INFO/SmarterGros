using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Suppliers/Index
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(suppliers);
        }

        // GET: /Suppliers/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null)
                return Json(new { success = false });

            return Json(new
            {
                id = s.Id,
                name = s.Name,
                businessActivity = s.BusinessActivity,
                phone = s.Phone,
                phone2 = s.Phone2,
                email = s.Email,
                address = s.Address,
                city = s.City,
                RC = s.RC,
                NIF = s.NIF,
                ai = s.AI,
                nis = s.NIS,
                bankAccount = s.BankAccount,
                bankName = s.BankName,
                initialDebt = s.InitialDebt,
                isActive = s.IsActive
            });
        }

        // GET: /Suppliers/GetFile/{id}
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            // 1. جلب بيانات المورد
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return Json(new { success = false, message = "المورد غير موجود" });

            // 2. جلب فواتير الشراء الخاصة بالمورد
            var purchases = await _context.Purchases
                .Where(p => p.SupplierId == id)
                .OrderBy(p => p.PurchaseDate)
                .ToListAsync();

            // 3. جلب كل الدفعات الخاصة بالمورد (من جدول SupplierPayments)
            var allPayments = await _context.SupplierPayments
                .Where(p => p.SupplierId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // 4. الحسابات العامة
            decimal totalPurchases = purchases.Sum(p => p.TotalAmount);
            decimal totalPaid = allPayments.Sum(p => p.Amount);

            // حساب ما تم سداده من الدين الابتدائي (الدفعات التي ليس لها فاتورة مرتبطة)
            decimal paidForInitialDebt = allPayments
                .Where(p => p.PurchaseId == null)
                .Sum(p => p.Amount);

            decimal initialDebtRemaining = Math.Max(0, supplier.InitialDebt - paidForInitialDebt);

            // إجمالي الديون الحالية
            decimal totalDebt = supplier.InitialDebt + totalPurchases - totalPaid;

            // 5. تجهيز قائمة الفواتير للعرض (Invoices)
            var invoicesData = new List<object>();

            // أ) إذا كان هناك دين ابتدائي، نضيفه كأول عنصر في القائمة
            if (supplier.InitialDebt > 0)
            {
                invoicesData.Add(new
                {
                    invoiceNumber = "الدين الابتدائي",
                    date = "قبل البدء",
                    total = supplier.InitialDebt,
                    paid = paidForInitialDebt,
                    remaining = initialDebtRemaining,
                    isInitialDebt = true // علامة للتمييز في الواجهة
                });
            }

            // ب) إضافة فواتير الشراء العادية
            foreach (var purchase in purchases)
            {
                // حساب المبلغ المدفوع لهذه الفاتورة تحديداً
                decimal paidForThisInvoice = allPayments
                    .Where(p => p.PurchaseId == purchase.Id)
                    .Sum(p => p.Amount);

                invoicesData.Add(new
                {
                    // تنبيه: تأكد أن اسم العمود في موديلك هو PurchaseNumber
                    // إذا كان اسمه InvoiceNumber في قاعدتك، غيّر السطر التالي
                    invoiceNumber = purchase.InvoiceNumber,
                    date = purchase.PurchaseDate.ToString("dd/MM/yyyy"),
                    total = purchase.TotalAmount,
                    paid = paidForThisInvoice,
                    remaining = purchase.TotalAmount - paidForThisInvoice,
                    isInitialDebt = false
                });
            }

            // 6. تجهيز سجل الدفعات للعرض (Payments)
            var paymentsData = allPayments.Select(p => new
            {
                date = p.PaymentDate.ToString("dd/MM/yyyy"),
                amount = p.Amount,
                invoiceRef = p.PurchaseId.HasValue
                    ? purchases.FirstOrDefault(x => x.Id == p.PurchaseId)?.InvoiceNumber ?? "غير معروف"
                    : "سداد دين ابتدائي"
            }).ToList();

            // 7. إرجاع النتيجة
            return Json(new
            {
                id = supplier.Id,
                name = supplier.Name,
                phone = supplier.Phone ?? "---",
                totalPurchases,
                totalPaid,
                totalDebt,
                initialDebt = supplier.InitialDebt,
                initialDebtRemaining,
                invoices = invoicesData, // استخدام القائمة الجديدة
                payments = paymentsData
            });
        }

        // POST: /Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] SupplierDto dto)
        {
            try
            {
                var supplier = new Supplier
                {
                    Name = dto.Name,
                    BusinessActivity = dto.BusinessActivity,
                    Phone = dto.Phone,
                    Phone2 = dto.Phone2,
                    Email = dto.Email,
                    Address = dto.Address,
                    City = dto.City,
                    RC = dto.RC,
                    NIF = dto.NIF,
                    AI = dto.AI,
                    NIS = dto.NIS,
                    BankAccount = dto.BankAccount,
                    BankName = dto.BankName,
                    InitialDebt = dto.InitialDebt,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = supplier.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: /Suppliers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] SupplierDto dto)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(dto.Id);
                if (supplier == null)
                    return Json(new { success = false, message = "المورد غير موجود" });

                supplier.Name = dto.Name;
                supplier.BusinessActivity = dto.BusinessActivity;
                supplier.Phone = dto.Phone;
                supplier.Phone2 = dto.Phone2;
                supplier.Email = dto.Email;
                supplier.Address = dto.Address;
                supplier.City = dto.City;
                supplier.RC = dto.RC;
                supplier.NIF = dto.NIF;
                supplier.AI = dto.AI;
                supplier.NIS = dto.NIS;
                supplier.BankAccount = dto.BankAccount;
                supplier.BankName = dto.BankName;
                supplier.InitialDebt = dto.InitialDebt;
                supplier.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: /Suppliers/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                    return Json(new { success = false });

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Suppliers/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay([FromBody] PaymentDto dto)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
                if (supplier == null)
                    return Json(new { success = false });

                var payment = new SupplierPayment
                {
                    SupplierId = dto.SupplierId,
                    Amount = dto.Amount,
                    PaymentDate = dto.Date,
                    CreatedAt = DateTime.Now
                };

                _context.SupplierPayments.Add(payment);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // تحديث SupplierDto
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? BusinessActivity { get; set; }
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? RC { get; set; } // RC
        public string? NIF { get; set; } // NIF
        public string? AI { get; set; }
        public string? NIS { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public decimal InitialDebt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PaymentDto
    {
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}

 