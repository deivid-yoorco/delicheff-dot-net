using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class IncidentFile : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual byte[] File { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }

        public virtual Incidents Incident { get; set; }
        public virtual int IncidentId { get; set; }
    }
}
