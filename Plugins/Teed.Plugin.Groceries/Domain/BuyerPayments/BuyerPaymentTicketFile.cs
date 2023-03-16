using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public class BuyerPaymentTicketFile : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int BuyerPaymentId { get; set; }
        public int FileId { get; set; }
        public int CreatedByCustomerId { get; set; }
    }
}
