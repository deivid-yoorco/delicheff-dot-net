using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Payment
{
    public class UpdateOrderPaymentMethodModel
    {
        public int OrderId { get; set; }
        public string PaymentName { get; set; }
        public string OldPaymentName { get; set; }
    }
}
