namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 🔄 طريقة استرداد قيمة المرتجع من المورد
    /// </summary>
    public enum ReturnRefundMethod
    {
        /// <summary>
        /// 💳 خصم من الدين
        /// يُخصم مبلغ المرتجع من رصيد الدين على الشركة
        /// </summary>
        DeductFromDebt = 1,

        /// <summary>
        /// 💵 استرداد نقدي
        /// المورد يُرجع المبلغ نقداً (يدخل الصندوق)
        /// </summary>
        CashRefund = 2,

        /// <summary>
        /// 🔀 مزيج
        /// جزء يُخصم من الدين وجزء يُسترد نقداً
        /// </summary>
        Mixed = 3
    }

    /// <summary>
    /// 🛠️ Extension Methods لطرق الاسترداد
    /// </summary>
    public static class ReturnRefundMethodExtensions
    {
        public static string GetArabicName(this ReturnRefundMethod method)
        {
            return method switch
            {
                ReturnRefundMethod.DeductFromDebt => "خصم من الدين",
                ReturnRefundMethod.CashRefund => "استرداد نقدي",
                ReturnRefundMethod.Mixed => "مزيج (نقدي + خصم)",
                _ => "غير معروف"
            };
        }

        public static string GetBadgeColor(this ReturnRefundMethod method)
        {
            return method switch
            {
                ReturnRefundMethod.DeductFromDebt => "info",
                ReturnRefundMethod.CashRefund => "success",
                ReturnRefundMethod.Mixed => "warning",
                _ => "secondary"
            };
        }

        public static string GetIcon(this ReturnRefundMethod method)
        {
            return method switch
            {
                ReturnRefundMethod.DeductFromDebt => "fa-scale-balanced",
                ReturnRefundMethod.CashRefund => "fa-money-bill-transfer",
                ReturnRefundMethod.Mixed => "fa-shuffle",
                _ => "fa-question"
            };
        }
    }
}