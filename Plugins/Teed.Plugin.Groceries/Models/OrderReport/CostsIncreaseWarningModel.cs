using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.Warnings
{
    public class CostsIncreaseWarningModel
    {
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime UpdatedOn { get; set; }
        public virtual int ProductId { get; set; }
        public virtual string ProductName { get; set; }
        public virtual decimal NewCost { get; set; }
        public virtual decimal OldCost { get; set; }
        public virtual DateTime OldReportedCostDate { get; set; }
        public virtual DateTime? OriginalOrderShippingDate { get; set; }
        public virtual bool Attended { get; set; }
        public virtual string Comment { get; set; }
        public virtual string Log { get; set; }
    }
}
