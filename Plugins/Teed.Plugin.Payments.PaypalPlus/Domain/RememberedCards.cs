using Nop.Core;
using System;

namespace Teed.Plugin.Payments.PaypalPlus.Domain
{
    public class RememberedCards : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string RememberedCardsHash { get; set; }
        public int CustomerId { get; set; }
    }
}