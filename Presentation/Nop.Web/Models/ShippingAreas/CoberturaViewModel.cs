using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.ShippingAreas
{
    public class CoberturaViewModel
    {
        public List<AreaData> AreasData { get; set; }
        //Body Send
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class AreaData
    {
        public string State { get; set; }
        public string City { get; set; }
        public string Suburbs { get; set; }
        public string PostalCode { get; set; }
        
    }
}
