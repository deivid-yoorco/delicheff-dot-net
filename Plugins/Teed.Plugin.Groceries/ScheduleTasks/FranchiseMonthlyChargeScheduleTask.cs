using Nop.Services.Configuration;
using Nop.Services.Logging;
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
    public class FranchiseMonthlyChargeScheduleTask : IScheduleTask
    {
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly FranchiseMonthlyChargeService _franchiseMonthlyChargeService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public FranchiseMonthlyChargeScheduleTask(PenaltiesCatalogService penaltiesCatalogService,
            ShippingVehicleService shippingVehicleService, FranchiseMonthlyChargeService franchiseMonthlyChargeService,
            ISettingService settingService, ILogger logger)
        {
            _penaltiesCatalogService = penaltiesCatalogService;
            _shippingVehicleService = shippingVehicleService;
            _franchiseMonthlyChargeService = franchiseMonthlyChargeService;
            _settingService = settingService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "FranchiseMonthlyChargeScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running FranchiseMonthlyChargeScheduleTask.");

                try
                {
                    var shippingVehicles = _shippingVehicleService.GetAll().Where(x => x.GpsInstallationDate.HasValue && x.Active).ToList();
                    var penaltiesCatalog = _penaltiesCatalogService.GetAll().Where(x => x.PenaltyCustomId == "GPS").ToList();
                    var monthlyCharges = _franchiseMonthlyChargeService.GetAll().Where(x => x.ChargeCode == "GPS").ToList();
                    GenerateGpsMonthlyCharge(shippingVehicles, monthlyCharges, penaltiesCatalog);
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running FranchiseMonthlyChargeScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running FranchiseMonthlyChargeScheduleTask.");
            }
        }

        private void GenerateGpsMonthlyCharge(List<ShippingVehicle> shippingVehicles,
            List<FranchiseMonthlyCharge> franchiseMonthlyCharges,
            List<PenaltiesCatalog> penaltiesCatalogs)
        {
            foreach (var shippingVehicle in shippingVehicles)
            {
                var gpsInstallationDate = shippingVehicle.GpsInstallationDate.Value;
                var controlDate = new DateTime(gpsInstallationDate.Year, gpsInstallationDate.Month, 1).AddMonths(2);
                var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                for (DateTime i = controlDate; i <= endDate; i = i.AddMonths(1))
                {
                    var exists = franchiseMonthlyCharges.Where(x => x.Date == i && x.VehicleId == shippingVehicle.Id).Any();
                    if (exists) continue;
                    var cost = penaltiesCatalogs.OrderByDescending(x => x.ApplyDate <= i).ThenByDescending(x => x.ApplyDate).FirstOrDefault();
                    if (cost == null) continue;
                    _franchiseMonthlyChargeService.Insert(new FranchiseMonthlyCharge()
                    {
                        VehicleId = shippingVehicle.Id,
                        AuthorizedStatusId = 1,
                        ChargeCode = "GPS",
                        Amount = cost.Amount,
                        Comments = "Servicio mensual de GPS",
                        Date = i,
                        FranchiseId = shippingVehicle.FranchiseId,
                        Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó el cobro de GPS de forma automática.\n"
                    });
                }
            }
        }
    }
}
