using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class OrderReportTransfer : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public DateTime OrderShippingDate { get; set; }
        public int BuyerId { get; set; }


        public decimal TransferAmount { get; set; }
        public byte[] File { get; set; }
        public string Extension { get; set; }
        public int FileUploadedByUserId { get; set; }
    }
}
