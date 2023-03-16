using Nop.Core;
using System;

namespace Teed.Plugin.Api.Domain.Groceries
{
    public class WebScrapingUnit : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string ProductName { get; set; }
        public virtual string Category1 { get; set; }
        public virtual string Category2 { get; set; }
        public virtual string Category3 { get; set; }
        public virtual string ImageUrl { get; set; }
        public virtual string ProductUrl { get; set; }
        public virtual string Store { get; set; }
    }
}