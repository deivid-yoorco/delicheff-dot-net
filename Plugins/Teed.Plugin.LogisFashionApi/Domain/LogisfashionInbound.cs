using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Domain
{
    public class LogisfashionInbound : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual Guid GuidId { get; set; }
        public virtual bool Deleted { get; set; }

        public string PONumber { get; set; }
        public int NotificationLogId { get; set; }
        public DateTime CreationDateUtc { get; set; }
        public string InventoryLocation { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int AuthorId { get; set; }
        public string Comments { get; set; }
    }
}
