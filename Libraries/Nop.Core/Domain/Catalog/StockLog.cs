using System;

namespace Nop.Core.Domain.Catalog
{
    public partial class StockLog : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public string SKU { get; set; }
        public string Product { get; set; }
        public int OldStock { get; set; }
        public int NewStock { get; set; }
        public string User { get; set; }
        public int ProductId { get; set; }
        public TypeChangeEnum ChangeType { get; set; }
    }
}
