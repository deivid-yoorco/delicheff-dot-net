using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;

namespace Teed.Plugin.Groceries.Domain.Franchise
{
    public class Billing : BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Billed { get; set; }
        public int StatusId { get; set; }
        public string Comment { get; set; }
        public decimal AmountAjustment { get; set; }
        public decimal BaseFixedRateCDMX { get; set; }
        public decimal BaseVariableRateCDMX { get; set; }
        public decimal BaseFixedRateOtherStates { get; set; }
        public decimal BaseVariableRateOtherStates { get; set; }
        public decimal FixedWeeklyBonusCeroIncidents { get; set; }
        public decimal VariableWeeklyBonusCeroIncidents { get; set; }
        public string Log { get; set; }

        public virtual ShippingVehicle Vehicle { get; set; }
        public int VehicleId { get; set; }
    }
}
