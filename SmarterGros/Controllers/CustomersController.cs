using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Customers/Index
        public async Task<IActionResult> Index()
        {
            var Customers = await _context.Customers
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(Customers);
        }

        // GET: /Customers/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.Customers.FindAsync(id);
            if (s == null)
                return Json(new { success = false });

            return Json(new
            {
                id = s.Id,
                name = s.Name, 
                phone = s.Phone,
                phone2 = s.Phone2,
                email = s.Email,
                address = s.Address,
                city = s.City,
                RC = s.RC,
                NIF = s.NIF, 
                initialDebt = s.InitialDebt,
                isActive = s.IsActive
            });
        }

        // GET: /Customers/GetFile/{id}
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            // 1. جلب بيانات المورد
            var Customer = await _context.Customers.FindAsync(id);
            if (Customer == null)
                return Json(new { success = false, message = "المورد غير موجود" });

            // 2. جلب فواتير الشراء الخاصة بالمورد
            var sales = await _context.Sales
                .Where(p => p.Id == id)
                .OrderBy(p => p.SaleDate)
                .ToListAsync();

            // 3. جلب كل الدفعات الخاصة بالمورد (من جدول CustomerPayments)
            var allPayments = await _context.CustomerPayments
                .Where(p => p.CustomerId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // 4. الحسابات العامة
            decimal totalSales = sales.Sum(p => p.TotalAmount);
            decimal totalPaid = allPayments.Sum(p => p.Amount);

            // حساب ما تم سداده من الدين الابتدائي (الدفعات التي ليس لها فاتورة مرتبطة)
            decimal paidForInitialDebt = allPayments
                .Where(p => p.SaleId == null)
                .Sum(p => p.Amount);

            decimal initialDebtRemaining = Math.Max(0, Customer.InitialDebt - paidForInitialDebt);

            // إجمالي الديون الحالية
            decimal totalDebt = Customer.InitialDebt + totalSales - totalPaid;

            // 5. تجهيز قائمة الفواتير للعرض (Invoices)
            var invoicesData = new List<object>();

            // أ) إذا كان هناك دين ابتدائي، نضيفه كأول عنصر في القائمة
            if (Customer.InitialDebt > 0)
            {
                invoicesData.Add(new
                {
                    invoiceNumber = "الدين الابتدائي",
                    date = "قبل البدء",
                    total = Customer.InitialDebt,
                    paid = paidForInitialDebt,
                    remaining = initialDebtRemaining,
                    isInitialDebt = true // علامة للتمييز في الواجهة
                });
            }

            // ب) إضافة فواتير الشراء العادية
            foreach (var sale in sales)
            {
                // حساب المبلغ المدفوع لهذه الفاتورة تحديداً
                decimal paidForThisInvoice = allPayments
                    .Where(p => p.SaleId == sale.Id)
                    .Sum(p => p.Amount);

                invoicesData.Add(new
                {
                    // تنبيه: تأكد أن اسم العمود في موديلك هو PurchaseNumber
                    // إذا كان اسمه InvoiceNumber في قاعدتك، غيّر السطر التالي
                    invoiceNumber = sale.InvoiceNumber,
                    date = sale.SaleDate.ToString("dd/MM/yyyy"),
                    total = sale.TotalAmount,
                    paid = paidForThisInvoice,
                    remaining = sale.TotalAmount - paidForThisInvoice,
                    isInitialDebt = false
                });
            }

            // 6. تجهيز سجل الدفعات للعرض (Payments)
            var paymentsData = allPayments.Select(p => new
            {
                date = p.PaymentDate.ToString("dd/MM/yyyy"),
                amount = p.Amount,
                invoiceRef = p.SaleId.HasValue
                    ? sales.FirstOrDefault(x => x.Id == p.SaleId)?.InvoiceNumber ?? "غير معروف"
                    : "سداد دين ابتدائي"
            }).ToList();

            // 7. إرجاع النتيجة
            return Json(new
            {
                id = Customer.Id,
                name = Customer.Name,
                phone = Customer.Phone ?? "---",
                totalSales,
                totalPaid,
                totalDebt,
                initialDebt = Customer.InitialDebt,
                initialDebtRemaining,
                invoices = invoicesData, // استخدام القائمة الجديدة
                payments = paymentsData
            });
        }

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CustomerDto dto)
        {
            try
            {
                var Customer = new Customer
                {
                    Name = dto.Name, 
                    Phone = dto.Phone,
                    Phone2 = dto.Phone2,
                    Email = dto.Email,
                    Address = dto.Address,
                    City = dto.City,
                    RC = dto.RC,
                    NIF = dto.NIF, 
                    InitialDebt = dto.InitialDebt,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Customers.Add(Customer);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = Customer.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: /Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] CustomerDto dto)
        {
            try
            {
                var Customer = await _context.Customers.FindAsync(dto.Id);
                if (Customer == null)
                    return Json(new { success = false, message = "المورد غير موجود" });

                Customer.Name = dto.Name; 
                Customer.Phone = dto.Phone;
                Customer.Phone2 = dto.Phone2;
                Customer.Email = dto.Email;
                Customer.Address = dto.Address;
                Customer.City = dto.City;
                Customer.RC = dto.RC;
                Customer.NIF = dto.NIF; 
                Customer.InitialDebt = dto.InitialDebt;
                Customer.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: /Customers/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var Customer = await _context.Customers.FindAsync(id);
                if (Customer == null)
                    return Json(new { success = false });

                _context.Customers.Remove(Customer);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Customers/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay([FromBody] PaymentCusDto dto)
        {
            try
            {
                var Customer = await _context.Customers.FindAsync(dto.CustomerId);
                if (Customer == null)
                    return Json(new { success = false });

                var payment = new CustomerPayment
                {
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    PaymentDate = dto.Date,
                    CreatedAt = DateTime.Now
                };

                _context.CustomerPayments.Add(payment);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // تحديث CustomerDto
    public class CustomerDto
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

    public class PaymentCusDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}

