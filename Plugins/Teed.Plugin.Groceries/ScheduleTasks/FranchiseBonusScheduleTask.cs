using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class FranchiseBonusScheduleTask : IScheduleTask
    {
        private readonly IncidentsService _incidentsService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly BillingService _billingService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly FranchiseBonusService _franchiseBonusService;
        private readonly ILogger _logger;

        public FranchiseBonusScheduleTask(IncidentsService incidentsService,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            BillingService billingService, PenaltiesCatalogService penaltiesCatalogService,
            FranchiseBonusService franchiseBonusService, ISettingService settingService,
            ILogger logger)
        {
            _incidentsService = incidentsService;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _billingService = billingService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _franchiseBonusService = franchiseBonusService;
            _settingService = settingService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "FranchiseBonusScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running FranchiseBonusScheduleTask.");

                try
                {
                    var yesterday = DateTime.Now.AddDays(-1).Date;
                    var controlDate = yesterday.AddDays(-14).Date;
                    //var controlDate = yesterday.AddMonths(-5).Date;
                    IQueryable<Order> ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                        .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= yesterday && x.OrderStatusId == 30); // Completed orders
                    var incidents = _incidentsService.GetAll().Where(x => x.AuthorizedStatusId == 1 && x.Date >= controlDate && x.Date <= yesterday).ToList();
                    List<FranchiseBonus> bonuses = _franchiseBonusService.GetAll()
                        .Where(x => x.Date >= controlDate && x.Date <= yesterday)
                        .ToList();
                    List<ShippingVehicleRoute> vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate >= controlDate && x.ShippingDate <= yesterday).ToList();
                    RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
                    var penaltiesCatalog = _penaltiesCatalogService.GetAll().ToList();

                    for (DateTime i = controlDate; i <= yesterday; i = i.AddDays(1))
                    {
                        var dateVehicleRoute = vehicleRoutes.Where(x => x.ShippingDate == i).GroupBy(x => x.Vehicle).ToList();
                        foreach (var vehicleRoute in dateVehicleRoute)
                        {
                            GenerateDailyBonus(i, incidents, vehicleRoute.FirstOrDefault(), ordersQuery, settings, bonuses);
                            GenerateInTimeBonus(i, vehicleRoute.FirstOrDefault(), ordersQuery, settings, bonuses, penaltiesCatalog);
                            GenerateRescueBonus(i, vehicleRoute.FirstOrDefault(), ordersQuery, settings, bonuses, penaltiesCatalog);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running FranchiseBonusScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running FranchiseBonusScheduleTask.");
            }
        }

        private void GenerateDailyBonus(DateTime date,
            List<Incidents> incidents,
            ShippingVehicleRoute vehicleRoute,
            IQueryable<Order> ordersQuery,
            RatesAndBonusesSettings settings,
            List<FranchiseBonus> bonuses)
        {
            if (vehicleRoute.VehicleId <= 0) return;
            var anyIncident = incidents.Where(x => x.Date == date && x.VehicleId == vehicleRoute.Vehicle.Id).Any();
            if (anyIncident) return;
            var existingBonus = bonuses
                .Where(x => x.VehicleId == vehicleRoute.VehicleId && x.Date == date && x.BonusCode == "B01")
                .FirstOrDefault();

            var orderTotal = ordersQuery
                .Where(x => x.SelectedShippingDate == date && (x.RouteId == vehicleRoute.RouteId || x.RescuedByRouteId == vehicleRoute.RouteId))
                .Select(x => x.OrderTotal - x.OrderShippingInclTax + (x.CustomerBalanceUsedAmount ?? 0))
                .DefaultIfEmpty()
                .Sum();

            var bonusAmount = _franchiseBonusService.GetBonusAmount("B01", settings, orderTotal);

            if (existingBonus == null && bonusAmount > 0)
            {
                var bonus = new FranchiseBonus()
                {
                    BonusCode = "B01",
                    Comments = $"Bono por no incidencia el día {date:dd-MM-yyyy} de la camioneta {VehicleUtils.GetVehicleName(vehicleRoute.Vehicle)}",
                    Date = date,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    VehicleId = vehicleRoute.VehicleId,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó de forma automática el bono diario por cero incidencias \n",
                    Amount = _franchiseBonusService.GetBonusAmount("B01", settings, orderTotal)
                };
                _franchiseBonusService.Insert(bonus);
            }
            else if (existingBonus != null && existingBonus.Amount != bonusAmount)
            {
                existingBonus.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - Se actualizó automáticamente el monto del bono de {existingBonus.Amount} a {bonusAmount} \n";
                existingBonus.Amount = bonusAmount;
                _franchiseBonusService.Update(existingBonus);
            }
        }

        private void GenerateInTimeBonus(DateTime date,
            ShippingVehicleRoute vehicleRoute,
            IQueryable<Order> ordersQuery,
            RatesAndBonusesSettings settings,
            List<FranchiseBonus> bonuses,
            List<PenaltiesCatalog> penaltiesCatalogs)
        {
            if (vehicleRoute.VehicleId <= 0) return;

            var existingBonus = bonuses
                .Where(x => x.VehicleId == vehicleRoute.VehicleId && x.Date == date && x.BonusCode == "B02")
                .Any();
            if (existingBonus) return;
            var orders = ordersQuery
                .Where(x => x.SelectedShippingDate == date &&
                (x.RouteId == vehicleRoute.RouteId
                // don't take in consideration Rescued Orders - Specified at 13/06/2022
                //|| x.RescuedByRouteId == vehicleRoute.RouteId)
                ))
                .ToList();
            var pendingToDeliver = orders
                .Where(x => x.Shipments.Count() == 0 || !x.Shipments.Where(y => y.DeliveryDateUtc.HasValue).Any())
                .Any();
            if (pendingToDeliver) return;
            // CHECK IF VEHICLE HAS AN UNAUTHORIZED INCIDENT R04 SO WE CAN APPLY THE BONUS
            //var hasUnauthorizedIncident = _incidentsService.GetAll()
            //    .Where(x => x.IncidentCode == "R04" && x.VehicleId == vehicleRoute.VehicleId && x.Date == date && x.AuthorizedStatusId == 0)
            //    .Any();
            foreach (var order in orders)
            {
                var deliverdDate = order.Shipments
                    .Where(x => x.DeliveryDateUtc.HasValue)
                    .Select(x => x.DeliveryDateUtc.Value)
                    .OrderBy(x => x)
                    .FirstOrDefault()
                    .ToLocalTime();

                bool deliveredInTime = OrderUtils.CheckIfDeliveredInTime(order.SelectedShippingTime, deliverdDate);
                if (!deliveredInTime) return;
            }

            var bonus = new FranchiseBonus()
            {
                BonusCode = "B02",
                Comments = $"Bono por todas las órdenes entregadas a tiempo el día {date:dd-MM-yyyy} de la camioneta {VehicleUtils.GetVehicleName(vehicleRoute.Vehicle)}",
                Date = date,
                FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                VehicleId = vehicleRoute.VehicleId,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó de forma automática el bono diario por todas las órdenes entregadas a tiempo \n",
                Amount = _franchiseBonusService.GetBonusAmount("B02", settings, penaltiesCatalogs: penaltiesCatalogs, date: date)
            };
            _franchiseBonusService.Insert(bonus);
        }

        private void GenerateRescueBonus(DateTime date,
            ShippingVehicleRoute vehicleRoute,
            IQueryable<Order> ordersQuery,
            RatesAndBonusesSettings settings,
            List<FranchiseBonus> bonuses,
            List<PenaltiesCatalog> penaltiesCatalogs)
        {
            var existingBonus = bonuses
                .Where(x => x.VehicleId == vehicleRoute.VehicleId && x.Date == date && x.BonusCode == "B03")
                .Any();
            if (existingBonus) return;
            var hasRescuedOrders = ordersQuery
                .Where(x => x.SelectedShippingDate == date && x.RescuedByRouteId == vehicleRoute.RouteId)
                .Any();
            if (hasRescuedOrders)
            {
                var bonus = new FranchiseBonus()
                {
                    BonusCode = "B03",
                    Comments = $"Bono por rescate de ruta el día {date:dd-MM-yyyy} de la camioneta {VehicleUtils.GetVehicleName(vehicleRoute.Vehicle)}",
                    Date = date,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    VehicleId = vehicleRoute.VehicleId,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó de forma automática el bono por rescate de otra ruta.\n",
                    Amount = _franchiseBonusService.GetBonusAmount("B03", settings, penaltiesCatalogs: penaltiesCatalogs, date: date)
                };
                _franchiseBonusService.Insert(bonus);
            }
        }


    }
}
