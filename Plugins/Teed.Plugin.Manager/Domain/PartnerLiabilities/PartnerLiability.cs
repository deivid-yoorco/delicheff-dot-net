using Nop.Core;
using System;

namespace Teed.Plugin.Manager.Domain.PartnerLiabilities
{
    public class PartnerLiability : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual DateTime SelectedDate { get; set; }
        public virtual int CreatedByUserId { get; set; }
        public virtual int PartnerId { get; set; }
        public virtual string Comments { get; set; }
        public virtual int CategoryId { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Log { get; set; }
    }
}