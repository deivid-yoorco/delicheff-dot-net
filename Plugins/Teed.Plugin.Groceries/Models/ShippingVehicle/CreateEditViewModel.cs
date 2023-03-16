using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.ShippingVehicle
{
    public class CreateEditViewModel
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public decimal LoadingCapacity { get; set; }
        public string Log { get; set; }
        public decimal FridgeVolume { get; set; }
        public decimal BunchVolume { get; set; }
        public int FranchiseId { get; set; }
        public virtual string Brand { get; set; }
        public virtual int Year { get; set; }
        public virtual string Plates { get; set; }
        public IList<SelectListItem> AvailablesFranchises { get; set; }
        public string InstallationDateString { get; set; }
    }
}
