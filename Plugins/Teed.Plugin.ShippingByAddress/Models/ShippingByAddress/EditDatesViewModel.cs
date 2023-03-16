using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.ShippingByAddress.Models
{
    public class EditDatesViewModel
    {
        public int Id { get; set; }
        public string PostalCode { get; set; }
        public string DaysToShip { get; set; }
        public SelectList Branches { get; set; }
        public int? ShippingBranchId { get; set; }
    }
}
