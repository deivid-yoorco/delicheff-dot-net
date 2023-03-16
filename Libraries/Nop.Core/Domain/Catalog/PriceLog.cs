using System;

namespace Nop.Core.Domain.Catalog
{
    public partial class PriceLog : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public string SKU { get; set; }
        public string Product { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? OldCost { get; set; }
        public decimal? NewCost { get; set; }
        public string User { get; set; }
        public int ProductId { get; set; }
    }
}
