using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teed.Plugin.Groceries.Domain.TipsWithCards
{
    public class TipsWithCard : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual decimal Amount { get; set; }
        public virtual int OrderId { get; set; }
        public virtual int ReportedByUserId { get; set; }
        public virtual string Log { get; set; }
    }
}