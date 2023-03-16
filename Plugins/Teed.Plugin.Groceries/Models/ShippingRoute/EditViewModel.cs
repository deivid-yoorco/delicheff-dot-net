using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingRoute
{
    public class EditViewModel
    {
        public int Id { get; set; }
        public string RouteName { get; set; }
        //public string VehicleName { get; set; }
        //public decimal LoadingCapacity { get; set; }
        //public string PostalCodes { get; set; }
        public bool Active { get; set; }
        public bool CanEdit { get; set; }
        public List<Domain.ShippingRoutes.ShippingRoute> ShippingRoutes { get; set; }

        //public decimal FridgeVolume { get; set; }
        //public decimal BunchVolume { get; set; }

        //public bool UserCanSelectedFranchise { get; set; }
        //public int? FranchiseId { get; set; }
        //public IList<SelectListItem> AvailablesFranchises { get; set; }
    }
}
