using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Dtos
{
    public class CreatePaymentRequestDto
    {
        public decimal Amount { get; set; }
        public int ManufacturerId { get; set; }
        public List<PaymentRequestFileDto> Files { get; set; }
        public string ShippingDate { get; set; }
    }

    public class PaymentRequestFileDto
    {
        public string Base64 { get; set; }
        public string MimeType { get; set; }
        public string Uri { get; set; }
    }

    public class BuyerPaymentDto
    {
        public int Id { get; set; }
        public DateTime ShippingDate { get; set; }
        public int BuyerId { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int PaymentFileId { get; set; }
        public int PaymentStatusId { get; set; }
        public string PaymentStatus { get; set; }
        public List<int> FileIds { get; set; }
    }
}
