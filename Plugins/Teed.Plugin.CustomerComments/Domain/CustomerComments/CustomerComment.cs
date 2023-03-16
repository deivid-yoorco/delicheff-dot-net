using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomerComments.Domain.CustomerComment
{
    public class CustomerComment : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string Message { get; set; }
        public int CustomerId { get; set; }
        public int CreatedByCustomerId { get; set; }
        public string Log { get; set; }
    }
}
