using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.SubstituteBuyers
{
    public class SubstituteBuyer : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime SelectedShippingDate { get; set; }
        public int CustomerId { get; set; }
        public int SubstituteCustomerId { get; set; }
        public string Log { get; set; }
    }
}
