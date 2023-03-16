using System;

namespace Nop.Core.Domain.Catalog
{
    public partial class ManufacturerLog : BaseEntity
    {
        public int ManufacturerId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string Message { get; set; }
    }
}
