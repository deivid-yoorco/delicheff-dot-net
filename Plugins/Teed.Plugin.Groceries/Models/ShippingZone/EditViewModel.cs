using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingZone
{
    public class EditViewModel
    {
        public int Id { get; set; }
        public string ZoneName { get; set; }
        public string PostalCodes { get; set; }
        public string ZoneColor { get; set; }
        public string AdditionalPostalCodes { get; set; }
        public List<Domain.ShippingZones.ShippingZone> ShippingZones { get; set; }
        public string PostalCodesWarning { get; set; }
    }
}
