using Nop.Core;
using System;

namespace Teed.Plugin.Groceries.Domain.Corcel
{
    public class CorcelCustomer : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public DateTime DateConvertedUtc { get; set; }

        public string Log { get; set; }
    }
}
