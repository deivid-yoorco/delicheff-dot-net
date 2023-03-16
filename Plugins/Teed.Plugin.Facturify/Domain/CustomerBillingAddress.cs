using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public class CustomerBillingAddress : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string BusinessName { get; set; }
        public string RFC { get; set; }
        public string FacturifyUuid { get; set; }
        public string Email { get; set; }
        public int CustomerId { get; set; }
    }
}