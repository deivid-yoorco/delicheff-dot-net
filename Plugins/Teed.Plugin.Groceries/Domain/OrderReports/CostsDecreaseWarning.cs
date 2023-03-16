using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.OrderReports
{
    public class CostsDecreaseWarning : BaseEntity
    {   
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int ProductId { get; set; }
        public virtual decimal NewCost { get; set; }
        public virtual decimal OldCost { get; set; }
        public virtual DateTime OldReportedCostDate { get; set; }
        public virtual DateTime? OriginalOrderShippingDate { get; set; }
        public virtual bool Attended { get; set; }
        public virtual string Comment { get; set; }

        public virtual string Log { get; set; }
    }
}
