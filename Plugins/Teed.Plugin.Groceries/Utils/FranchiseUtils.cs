using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Services;

namespace Teed.Plugin.Groceries.Utils
{
    public static class FranchiseUtils
    {
        public static List<FranchiseBonus> GetFranchiseBonus(DateTime initDate, DateTime endDate, List<int> vehicleIds, FranchiseBonusService franchiseBonusService)
        {
            return franchiseBonusService.GetAll()
                .Where(x => x.Date >= initDate && x.Date <= endDate && vehicleIds.Contains(x.VehicleId))
                .ToList();
        }

        public static decimal GetIncidentsValue(DateTime initDate, DateTime endDate, List<int> vehicleIds, IncidentsService incidentsService)
        {
            return incidentsService.GetAll()
                .Where(x => x.Date >= initDate && x.Date <= endDate && vehicleIds.Contains(x.VehicleId))
                .Select(x => x.Amount)
                .DefaultIfEmpty()
                .Sum();
        }

        public static decimal GetBilled(DateTime initDate, DateTime endDate, List<int> vehicleIds, BillingService billingService)
        {
            var parsedEndDate = endDate.Date;
            var parsedInitDate = initDate.Date;
            return billingService.GetAll()
                    .Where(x => DbFunctions.TruncateTime(x.InitDate) == parsedInitDate && DbFunctions.TruncateTime(x.EndDate) == parsedEndDate && vehicleIds.Contains(x.VehicleId))
                    .Select(x => x.Billed)
                    .DefaultIfEmpty()
                    .FirstOrDefault();
        }
    }
}
