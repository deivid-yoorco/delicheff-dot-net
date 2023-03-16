using Nop.Core;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teed.Plugin.Api.Domain.Groceries
{
    public class WebScrapingHistory : BaseEntity
    {
        public virtual Guid GuidId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual int WebScrapingProductId { get; set; }
        [ForeignKey("WebScrapingProductId")]
        public virtual WebScrapingUnit WebScrapingProduct { get; set; }
        public virtual decimal Price { get; set; }
    }
}