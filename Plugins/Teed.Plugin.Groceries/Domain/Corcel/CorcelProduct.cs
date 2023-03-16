using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.Corcel
{
    public class CorcelProduct : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public string Log { get; set; }
    }
}
