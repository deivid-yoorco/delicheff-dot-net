using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Manager.Domain.PurchaseOrders;

namespace Teed.Plugin.Manager.Models.PurchaseOrder
{
    public class EditPurchaseOrderModel
    {
        public int Id { get; set; }
        public PurchaseOrderPaymentStatus PaymentStatus { get; set; }
        public PurchaseOrderStatus PurchaseOrderStatus { get; set; }
        public string Comment { get; set; }
        public string Log { get; set; }
        public decimal PartialPaymentValue { get; set; }
    }
}
