using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingRoute
{
    public class CreateViewModel
    {
        public string RouteName { get; set; }
        //public string VehicleName { get; set; }
        //public decimal LoadingCapacity { get; set; }
        //public string PostalCodes { get; set; }
        //public int FirstResponsableUserId { get; set; }
        public bool Active { get; set; }

        //public decimal FridgeVolume { get; set; }
        //public decimal BunchVolume { get; set; }

        //public bool UserCanSelectedFranchise { get; set; }

        //public int? FranchiseId { get; set; }
        //public IList<SelectListItem> AvailablesFranchises { get; set; }
    }
}
