using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class FranchiseMonthlyCharge : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public string ChargeCode { get; set; }
        public DateTime Date { get; set; }
        public int FranchiseId { get; set; }
        public int VehicleId { get; set; }
        public string Comments { get; set; }
        public string Log { get; set; }
        public decimal Amount { get; set; }
        public int AuthorizedStatusId { get; set; }

        public virtual Franchise Franchise { get; set; }
        public virtual ShippingVehicle Vehicle { get; set; }
    }
}
