using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.BuyerPayments
{
    public class BuyerPaymentByteFile : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public byte[] FileBytes { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
    }
}
