using System;

namespace Nop.Core.Domain.Catalog
{
    public partial class ProductLog : BaseEntity
    {
        public int ProductId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string Message { get; set; }
    }
}
