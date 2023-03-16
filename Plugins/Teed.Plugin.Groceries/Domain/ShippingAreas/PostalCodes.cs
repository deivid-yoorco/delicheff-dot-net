using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Domain.ShippingAreas
{
    public class PostalCodes : BaseEntity
    {
        public string Colonia { get; set; }
        public string Cp { get; set; }
        public string Edo { get; set; }
        public string Municipio { get; set; }
    }
}
