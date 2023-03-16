using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Product
{
    public class RelatedProductGenerated : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int ProductId { get; set; }
        public int RelatedProductId { get; set; }
        public int UpdateControlId { get; set; }
        public int OrderRelationCount { get; set; }
    }
}
