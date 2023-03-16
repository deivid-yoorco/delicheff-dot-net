using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.PrintedCouponBooks
{
    public class PrintedCouponBook : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string Name { get; set; }
        public virtual int ReferencePictureId { get; set; }
        public virtual int Inventory { get; set; }
        public virtual bool Active { get; set; }
        public virtual int BookTypeId { get; set; }
        public virtual string BookTypeValue { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual int ConnectedProductId { get; set; }

        public virtual string Log { get; set; }
    }
}
