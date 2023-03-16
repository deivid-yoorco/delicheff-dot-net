using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
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
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.ScheduleTasks
{
    public class IncidentsCheckScheduleTask : IScheduleTask
    {
        private readonly IncidentsService _incidentsService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly BillingService _billingService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly PenaltiesVariablesService _penaltiesVariablesService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public IncidentsCheckScheduleTask(IncidentsService incidentsService,
            IOrderService orderService, ShippingVehicleRouteService shippingVehicleRouteService,
            BillingService billingService, PenaltiesCatalogService penaltiesCatalogService,
            NotDeliveredOrderItemService notDeliveredOrderItemService, IProductService productService,
            PenaltiesVariablesService penaltiesVariablesService, ShippingVehicleService shippingVehicleService,
            ISettingService settingService, ILogger logger)
        {
            _incidentsService = incidentsService;
            _orderService = orderService;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _billingService = billingService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _penaltiesVariablesService = penaltiesVariablesService;
            _shippingVehicleService = shippingVehicleService;
            _settingService = settingService;
            _logger = logger;
        }

        public void Execute()
        {
            //Check if its time to run
            if (TaskRunUtils.ShouldRunTask(_settingService, "IncidentsCheckScheduleTask"))
            {
                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Running IncidentsCheckScheduleTask.");

                try
                {
                    var controlDate = DateTime.Now.AddDays(-14).Date;
                    //var controlDate = DateTime.Now.AddMonths(-10).Date;
                    var today = DateTime.Now.Date;
                    var orders = OrderUtils.GetFilteredOrders(_orderService)
                        .Where(x => x.SelectedShippingDate >= controlDate && x.SelectedShippingDate <= today)
                        .ToList();
                    var vehicleRoutes = _shippingVehicleRouteService.GetAll().Where(x => x.ShippingDate >= controlDate && x.ShippingDate <= today).ToList();
                    var penaltiesCatalog = _penaltiesCatalogService.GetAll().ToList();
                    var penaltiesVariables = _penaltiesVariablesService.GetAll().ToList();

                    var orderIds = orders.Select(x => x.Id).ToList();
                    var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

                    GenerateNotDeliveredProductIncident(orders.Where(x => x.OrderStatusId == 30).ToList(), vehicleRoutes, penaltiesCatalog, penaltiesVariables); //R01
                    GenerateLateDeliveryIncidents(orders.Where(x => x.OrderStatusId == 30).ToList(), vehicleRoutes, penaltiesCatalog, penaltiesVariables, notDeliveredOrderItems); //R04
                    GenerateVeryLateDeliveryIncidents(orders.Where(x => x.OrderStatusId == 30).ToList(), vehicleRoutes, penaltiesCatalog, penaltiesVariables, notDeliveredOrderItems); //R05
                    GenerateNotDeliveredIncidents(orders.Where(x => x.OrderStatusId == 50).ToList(), vehicleRoutes, penaltiesCatalog, penaltiesVariables, notDeliveredOrderItems); //R06
                    GenerateRescueRouteIncidents(orders.Where(x => x.OrderStatusId == 30).ToList(), vehicleRoutes, penaltiesCatalog, penaltiesVariables, notDeliveredOrderItems); //R11
                    GenerateNotWorkingDayIncidents(orders, vehicleRoutes, penaltiesCatalog, penaltiesVariables, notDeliveredOrderItems); //P02
                }
                catch (Exception e)
                {
                    _logger.Error($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Error while running IncidentsCheckScheduleTask:\n\"{e.Message}\".", e);
                }

                _logger.Information($"[TaskRunUtils] - {DateTime.Now:dd/MM/yyyy hh:mm:ss tt} - Finished running IncidentsCheckScheduleTask.");
            }
        }

        private void GenerateNotDeliveredProductIncident(List<Order> orders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables)
        {
            var orderItemIdsString = _incidentsService.GetAll().Where(x => x.IncidentCode == "R01")
                .Select(x => x.OrderItemIds)
                .ToList();
            var reportedIncidentsOrderItemIds = string.Join(",", orderItemIdsString)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x.Trim()))
                .ToList();
            var orderIds = orders.Select(x => x.Id).ToList();
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll()
                .Where(x => !reportedIncidentsOrderItemIds.Contains(x.OrderItemId) && x.NotDeliveredReasonId != 1 && orderIds.Contains(x.OrderId))
                .ToList();
            var orderIdsWithNotDeliveredItems = notDeliveredOrderItems.Select(x => x.OrderId).ToList();
            var filteredOrders = orders.Where(x => orderIdsWithNotDeliveredItems.Contains(x.Id));
            var productIds = notDeliveredOrderItems.Select(x => x.ProductId).ToList();
            var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id)).ToList();

            foreach (var notDeliveredOrderItem in notDeliveredOrderItems)
            {
                var order = filteredOrders.Where(x => x.Id == notDeliveredOrderItem.OrderId).FirstOrDefault();
                if (order == null) continue;
                var vehicleRoute = vehicleRoutes.Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate.Value).FirstOrDefault();
                if (vehicleRoute == null || vehicleRoute.Vehicle == null) continue;
                var productName = products.Where(x => x.Id == notDeliveredOrderItem.ProductId).FirstOrDefault()?.Name;

                Incidents incident = new Incidents()
                {
                    Date = order.SelectedShippingDate.Value,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    Comments = $"Incidencia automática por producto no entregado: {productName}",
                    IncidentCode = "R01",
                    OrderIds = order.Id.ToString(),
                    OrderItemIds = notDeliveredOrderItem.OrderItemId.ToString(),
                    VehicleId = vehicleRoute.VehicleId,
                    AuthorizedStatusId = (int)IncidentStatus.Authorized,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó una incidencia de forma automática.\n",
                    SelectedQuantity = notDeliveredOrderItem.Quantity
                };
                _incidentsService.Insert(incident);

                incident.Amount = _billingService.IncidentWork(incident, costs, new List<Order>() { order }, penaltiesVariables, null, notDeliveredOrderItem.PriceInclTax);
                _incidentsService.Update(incident);
            }
        }

        private void GenerateNotDeliveredIncidents(List<Order> notDeliveredOrders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems)
        {
            var orderIdsString = _incidentsService.GetAll().Where(x => x.IncidentCode == "R06")
                .Select(x => x.OrderIds)
                .ToList();
            var reportedIncidentsOrderIds = string.Join(",", orderIdsString)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            var filteredOrders = notDeliveredOrders
                .Where(x => !reportedIncidentsOrderIds.Contains(x.Id))
                .ToList();

            foreach (var order in filteredOrders)
            {
                var vehicleRoute = vehicleRoutes.Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate.Value).FirstOrDefault();
                if (vehicleRoute == null || vehicleRoute.Vehicle == null) continue;

                Incidents incident = new Incidents()
                {
                    Date = order.SelectedShippingDate.Value,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    Comments = "Incidencia automática por pedido no entregado",
                    IncidentCode = "R06",
                    OrderIds = order.Id.ToString(),
                    VehicleId = vehicleRoute.VehicleId,
                    AuthorizedStatusId = (int)IncidentStatus.Authorized,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó una incidencia de forma automática.\n"
                };
                _incidentsService.Insert(incident);

                incident.Amount = _billingService.IncidentWork(incident, costs, new List<Order>() { order }, penaltiesVariables, notDeliveredOrderItems);
                _incidentsService.Update(incident);
            }
        }

        private void GenerateLateDeliveryIncidents(List<Order> orders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems)
        {
            var orderIdsString = _incidentsService.GetAll().Where(x => x.IncidentCode == "R04")
                .Select(x => x.OrderIds)
                .ToList();
            var reportedIncidentsOrderIds = string.Join(",", orderIdsString)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            var pedidos = OrderUtils.GetMainOrderOfPedidoOnlyByList(orders);
            var filteredOrders = pedidos
                .Where(x => !reportedIncidentsOrderIds.Contains(x.Id) && x.Shipments.Where(y => y.DeliveryDateUtc.HasValue).Any())
                .ToList();

            var lateOrders = GetOnlyLateOrders(filteredOrders, 16, 30);
            foreach (var order in lateOrders)
            {
                var vehicleRoute = vehicleRoutes.Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate.Value).FirstOrDefault();
                if (vehicleRoute == null || vehicleRoute.Vehicle == null) continue;

                Incidents incident = new Incidents()
                {
                    Date = order.SelectedShippingDate.Value,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    Comments = "Incidencia automática por pedido entregado fuera de horario (16 - 30 minutos). Orden #" + order.CustomOrderNumber,
                    IncidentCode = "R04",
                    OrderIds = order.Id.ToString(),
                    VehicleId = vehicleRoute.VehicleId,
                    AuthorizedStatusId = (int)IncidentStatus.Authorized,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó una incidencia de forma automática.\n"
                };
                _incidentsService.Insert(incident);

                incident.Amount = _billingService.IncidentWork(incident, costs, new List<Order>() { order }, penaltiesVariables, notDeliveredOrderItems);
                _incidentsService.Update(incident);
            }
        }

        private void GenerateVeryLateDeliveryIncidents(List<Order> orders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems)
        {
            var orderIdsString = _incidentsService.GetAll().Where(x => x.IncidentCode == "R05")
                .Select(x => x.OrderIds)
                .ToList();
            var reportedIncidentsOrderIds = string.Join(",", orderIdsString)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            var pedidos = OrderUtils.GetMainOrderOfPedidoOnlyByList(orders);
            var filteredOrders = pedidos
                .Where(x => !reportedIncidentsOrderIds.Contains(x.Id) && x.Shipments.Where(y => y.DeliveryDateUtc.HasValue).Any())
                .ToList();

            var lateOrders = GetOnlyLateOrders(filteredOrders, 31, 9999);
            foreach (var order in lateOrders)
            {
                var vehicleRoute = vehicleRoutes.Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate.Value).FirstOrDefault();
                if (vehicleRoute == null || vehicleRoute.Vehicle == null) continue;

                Incidents incident = new Incidents()
                {
                    Date = order.SelectedShippingDate.Value,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    Comments = "Incidencia automática por pedido entregado fuera de horario (más de 30 minutos) Orden #" + order.CustomOrderNumber,
                    IncidentCode = "R05",
                    OrderIds = order.Id.ToString(),
                    VehicleId = vehicleRoute.VehicleId,
                    AuthorizedStatusId = (int)IncidentStatus.Authorized,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó una incidencia de forma automática.\n"
                };
                _incidentsService.Insert(incident);

                incident.Amount = _billingService.IncidentWork(incident, costs, new List<Order>() { order }, penaltiesVariables, notDeliveredOrderItems);
                _incidentsService.Update(incident);
            }
        }

        private void GenerateRescueRouteIncidents(List<Order> orders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems)
        {
            var rescuedOrders = orders.Where(x => x.RescuedByRouteId.HasValue && x.RescuedByRouteId.Value > 0);

            var allIncidents = _incidentsService.GetAll().Where(x => x.IncidentCode == "R11").ToList();
            var orderIdsString = allIncidents.Select(x => x.OrderIds).ToList();

            var reportedIncidentsOrderIds = string.Join(",", orderIdsString)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            var pendingOrdersToCreateIncident = rescuedOrders
                .Where(x => !reportedIncidentsOrderIds.Contains(x.Id))
                .ToList();

            foreach (var order in pendingOrdersToCreateIncident)
            {
                ShippingVehicleRoute vehicleRoute = vehicleRoutes.Where(x => x.RouteId == order.RouteId && x.ShippingDate == order.SelectedShippingDate.Value).FirstOrDefault();
                if (vehicleRoute == null || vehicleRoute.Vehicle == null) continue;
                bool alreadyExistingIncident = allIncidents.Where(x => x.Date == order.SelectedShippingDate && x.VehicleId == vehicleRoute.VehicleId).Any();
                if (alreadyExistingIncident) continue;

                Incidents incident = new Incidents()
                {
                    Date = order.SelectedShippingDate.Value,
                    FranchiseId = vehicleRoute.Vehicle.FranchiseId,
                    Comments = "Incidencia automática por rescate de ruta",
                    IncidentCode = "R11",
                    OrderIds = order.Id.ToString(),
                    VehicleId = vehicleRoute.VehicleId,
                    AuthorizedStatusId = (int)IncidentStatus.Authorized,
                    Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó la incidencia de forma automática.\n"
                };
                _incidentsService.Insert(incident);

                incident.Amount = _billingService.IncidentWork(incident, costs, new List<Order>() { order }, penaltiesVariables, notDeliveredOrderItems);
                _incidentsService.Update(incident);

                allIncidents.Add(incident);
            }
        }

        private void GenerateNotWorkingDayIncidents(List<Order> orders,
            List<ShippingVehicleRoute> vehicleRoutes,
            List<PenaltiesCatalog> costs,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems)
        {
            var today = DateTime.Now.Date;
            var orderDates = orders.GroupBy(x => x.SelectedShippingDate.Value).Select(x => x.Key).Where(x => x < today).ToList();
            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId > 1 && x.Active).ToList();
            var allIncidents = _incidentsService.GetAll().Where(x => x.IncidentCode == "P02").ToList();
            foreach (var date in orderDates)
            {
                var dateVehicleIds = vehicleRoutes.Where(x => x.ShippingDate == date).Select(x => x.VehicleId).ToList();
                var notWorkingVehicleIds = franchiseVehicles.Where(x => x.CreatedOnUtc <= date).Select(x => x.Id).Except(dateVehicleIds).ToList();
                foreach (var notWorkingVehicleId in notWorkingVehicleIds)
                {
                    bool alreadyExistingIncident = allIncidents.Where(x => x.Date == date && x.VehicleId == notWorkingVehicleId).Any();
                    if (alreadyExistingIncident) continue;
                    Incidents incident = new Incidents()
                    {
                        Date = date,
                        FranchiseId = franchiseVehicles.Where(x => x.Id == notWorkingVehicleId).Select(x => x.FranchiseId).FirstOrDefault(),
                        Comments = "Incidencia automática por día no trabajado",
                        IncidentCode = "P02",
                        OrderIds = null,
                        VehicleId = notWorkingVehicleId,
                        AuthorizedStatusId = (int)IncidentStatus.Authorized,
                        Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - Se creó la incidencia de forma automática.\n"
                    };
                    _incidentsService.Insert(incident);
                    incident.Amount = _billingService.IncidentWork(incident, costs, null, penaltiesVariables, notDeliveredOrderItems);
                    _incidentsService.Update(incident);
                    allIncidents.Add(incident);
                }
            }
        }

        #region Shared Methods

        private List<Order> GetOnlyLateOrders(List<Order> orders, int minMinutes, int maxMinutes)
        {
            return orders
                .Where(x =>
                    DeliveryIsLate(x.SelectedShippingTime, x.Shipments.Where(y => y.DeliveryDateUtc.HasValue).Select(y => y.DeliveryDateUtc).FirstOrDefault().Value.ToLocalTime(), minMinutes, maxMinutes))
                .ToList();
        }

        private bool DeliveryIsLate(string selectedTime, DateTime deliveredDate, int minMinutes, int maxMinutes)
        {
            ParsedShippingTimeModel parsedTime = OrderUtils.ParseSelectedShippingTime(selectedTime);
            TimeSpan deliveredTime = deliveredDate.TimeOfDay;
            TimeSpan minTime = TimeSpan.FromMinutes(minMinutes);
            TimeSpan maxTime = TimeSpan.FromMinutes(maxMinutes + 1);
            return deliveredTime >= parsedTime.EndTime.Add(minTime) && deliveredTime < parsedTime.EndTime.Add(maxTime);
        }

        #endregion

    }

    public class LateOrderModel
    {
        public int OrderId { get; set; }
        public DateTime DeliveryDateUtc { get; set; }
        public string SelectedShippingTime { get; set; }
        public DateTime SelectedShippingDate { get; set; }
        public int RouteId { get; set; }
    }
}
