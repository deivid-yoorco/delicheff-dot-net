using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingZone
{
    public class CreateViewModel
    {
        public string ZoneName { get; set; }
        public string PostalCodes { get; set; }
        public int FirstResponsableUserId { get; set; }

        public string ZoneColor { get; set; }
        public string AdditionalPostalCodes { get; set; }

        public string PostalCodesWarning { get; set; }
    }
}
