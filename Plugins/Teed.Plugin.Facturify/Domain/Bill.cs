using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public class Bill : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public string CfdiUuid { get; set; }
        public string XmlBase64 { get; set; }
        public string PdfBase64 { get; set; }
        public string Serie { get; set; }
        public int Folio { get; set; }
        public string Type { get; set; }
        public int CustomerBillingAddressId { get; set; }
        public int OrderId { get; set; }
        public string JobId { get; set; }
    }
}
