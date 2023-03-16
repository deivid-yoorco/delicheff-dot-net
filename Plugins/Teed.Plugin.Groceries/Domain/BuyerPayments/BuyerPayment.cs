using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public class BuyerPayment : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime ShippingDate { get; set; }
        public int BuyerId { get; set; }
        public int ManufacturerId { get; set; }
        public decimal RequestedAmount { get; set; }
        public int PaymentFileId { get; set; }
        public int PaymentStatusId { get; set; }
        public int InvoiceFileXmlId { get; set; }
        public int InvoiceFilePdfId { get; set; }

        public string Log { get; set; }
    }
}
