using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.MercadoPago.Domain
{
    public class MercadoPagoApiLog : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public int OrderId { get; set; }
        public string NotificationJson { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
