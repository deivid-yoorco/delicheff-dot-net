using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.Franchise;
using Teed.Plugin.Groceries.Utils;

namespace Teed.Plugin.Groceries.Services
{
    public class BillingService
    {
        private readonly IRepository<Billing> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderService _orderService;
        private readonly IncidentsService _incidentsService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly PenaltiesCatalogService _penaltiesCatalogService;
        private readonly FranchiseService _franchiseService;
        private readonly ShippingVehicleRouteService _shippingVehicleRouteService;
        private readonly ShippingVehicleService _shippingVehicleService;
        private readonly PenaltiesVariablesService _penaltiesVariablesService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly IWorkContext _workContext;
        private readonly IPaymentService _paymentService;

        public BillingService(
            IRepository<Billing> db,
            IEventPublisher eventPublisher,
            IOrderService orderService,
            IncidentsService incidentsService,
            ISettingService settingService,
            ShippingRouteService shippingRouteService,
            PenaltiesCatalogService penaltiesCatalogService,
            FranchiseService franchiseService,
            ShippingVehicleRouteService shippingVehicleRouteService,
            ShippingVehicleService shippingVehicleService,
            IWorkContext workContext,
            PenaltiesVariablesService penaltiesVariablesService,
            NotDeliveredOrderItemService notDeliveredOrderItemService,
            IProductService productService,
            IPaymentService paymentService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _orderService = orderService;
            _incidentsService = incidentsService;
            _settingService = settingService;
            _shippingRouteService = shippingRouteService;
            _penaltiesCatalogService = penaltiesCatalogService;
            _franchiseService = franchiseService;
            _workContext = workContext;
            _shippingVehicleRouteService = shippingVehicleRouteService;
            _shippingVehicleService = shippingVehicleService;
            _penaltiesVariablesService = penaltiesVariablesService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _productService = productService;
            _paymentService = paymentService;
        }

        public IQueryable<Billing> GetAll(bool deleted = false)
        {
            if (deleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(Billing entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
            entity.BaseFixedRateCDMX = settings.BaseFixedRateCDMX;
            entity.BaseFixedRateOtherStates = settings.BaseFixedRateOtherStates;
            entity.BaseVariableRateCDMX = settings.BaseVariableRateCDMX;
            entity.BaseVariableRateOtherStates = settings.BaseVariableRateOtherStates;
            entity.FixedWeeklyBonusCeroIncidents = settings.FixedWeeklyBonusCeroIncidents;
            entity.VariableWeeklyBonusCeroIncidents = settings.VariableWeeklyBonusCeroIncidents;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(Billing entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Billing entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }

        public decimal GetBilledWithAdjustment(ShippingVehicle vehicle,
            DateTime initDate,
            DateTime endDate,
            int billId = 0,
            bool useSecondary = false,
            decimal amountAjustment = 0)
        {
            Billing billing = null;
            if (billId > 0)
                billing = GetAll().Where(x => x.Id == billId).FirstOrDefault();

            var billed = GetBilled(vehicle, initDate, endDate, currentBill: billing);

            return billed + (useSecondary ? amountAjustment : billing.AmountAjustment);
        }

        public decimal GetBilled(ShippingVehicle vehicle, DateTime initDate, DateTime endDate, Billing currentBill = null)
        {
            decimal total = 0;
            decimal baseFixedRateCDMX = 0;
            decimal baseVariableRateCDMX = 0;
            decimal baseFixedRateOtherStates = 0;
            decimal baseVariableRateOtherStates = 0;
            decimal fixedWeeklyBonusCeroIncidents = 0;
            decimal variableWeeklyBonusCeroIncidents = 0;

            var costs = _penaltiesCatalogService.GetAll().ToList();
            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();
            if (currentBill == null)
            {
                baseFixedRateCDMX = settings.BaseFixedRateCDMX;
                baseVariableRateCDMX = settings.BaseVariableRateCDMX;
                baseFixedRateOtherStates = settings.BaseFixedRateOtherStates;
                baseVariableRateOtherStates = settings.BaseVariableRateOtherStates;
                fixedWeeklyBonusCeroIncidents = settings.FixedWeeklyBonusCeroIncidents;
                variableWeeklyBonusCeroIncidents = settings.VariableWeeklyBonusCeroIncidents;
            }
            else
            {
                baseFixedRateCDMX = currentBill.BaseFixedRateCDMX;
                baseVariableRateCDMX = currentBill.BaseVariableRateCDMX;
                baseFixedRateOtherStates = currentBill.BaseFixedRateOtherStates;
                baseVariableRateOtherStates = currentBill.BaseVariableRateOtherStates;
                fixedWeeklyBonusCeroIncidents = currentBill.FixedWeeklyBonusCeroIncidents;
                variableWeeklyBonusCeroIncidents = currentBill.VariableWeeklyBonusCeroIncidents;
            }

            var vehicleOrders = OrderUtils.GetAllFranchiseOrders(new List<ShippingVehicle>() { vehicle }, _shippingVehicleRouteService, _orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate && (!x.RescuedByRouteId.HasValue || x.RescuedByRouteId.Value == 0));
            var franchiseRoutes = vehicleOrders.Select(x => DbFunctions.AddMilliseconds(x.SelectedShippingDate.Value, x.RouteId)).ToList();

            var rescuedByFranchise = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => (x.RescuedByRouteId.HasValue && x.RescuedByRouteId > 0) && x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate)
                .Where(x => franchiseRoutes.Contains(DbFunctions.AddMilliseconds(x.SelectedShippingDate, x.RescuedByRouteId.Value)));

            var allOrders = vehicleOrders.Union(rescuedByFranchise).Where(x => x.OrderStatusId == 30);
            if (allOrders.Any())
            {
                var ordersTotal = allOrders.Select(x => x.OrderTotal - x.OrderShippingInclTax + (x.CustomerBalanceUsedAmount ?? 0)).DefaultIfEmpty().Sum();
                // Sum up bonuses and totals of orders
                total += (ordersTotal * baseVariableRateCDMX) + baseFixedRateCDMX;
            }

            return Math.Round(total, 2);
        }

        public decimal GetTotalOfIncidents(Franchise franchise, DateTime initDate, DateTime endDate)
        {
            var total = (decimal)0;
            var costs = _penaltiesCatalogService.GetAll().ToList();

            // Get incidents, orders and routes of franchise
            var incidents = _incidentsService.GetAll()
                .Where(x => initDate <= x.Date && x.Date <= endDate
                && franchise.Id == x.FranchiseId).ToList();

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchise.Id).ToList();
            var orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService)
                .Where(x => initDate <= x.SelectedShippingDate && x.SelectedShippingDate <= endDate)
                .ToList();

            var penaltiesVariables = _penaltiesVariablesService.GetAll().ToList();

            var orderIds = incidents.SelectMany(x => x.OrderIds.Split(',')).Select(x => int.Parse(x)).ToList();
            var notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            if (orders.Any())
            {
                foreach (var incident in incidents)
                {
                    total += IncidentWork(incident, costs, orders, penaltiesVariables, notDeliveredOrderItems);
                }
            }
            return total;
        }

        public decimal GetTotalOfIncidentsByDate(Franchise franchise, DateTime day)
        {
            var total = (decimal)0;
            var costs = _penaltiesCatalogService.GetAll().ToList();

            // Get incidents, orders and routes of franchise
            var incidents = _incidentsService.GetAll()
                .Where(x => day == x.Date
                && franchise.Id == x.FranchiseId).ToList();
            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.FranchiseId == franchise.Id).ToList();
            var orders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService)
                .Where(x => x.SelectedShippingDate == day);

            if (orders.Any())
            {
                total -= incidents.Where(x => x.AuthorizedStatusId == (int)IncidentStatus.Authorized).Select(x => x.Amount).DefaultIfEmpty().Sum();
            }
            return total;
        }

        public decimal GetTotalOfIncident(int incidentId, int vehicleId, DateTime initDate, DateTime endDate, Incidents directIncident = null)
        {
            var total = (decimal)0;
            var costs = _penaltiesCatalogService.GetAll().ToList();

            var incident = _incidentsService.GetAll()
                .Where(x => incidentId == x.Id).FirstOrDefault();
            if (directIncident != null)
                incident = directIncident;
            if (incident == null)
                return total;

            var franchiseVehicles = _shippingVehicleService.GetAll().Where(x => x.Id == vehicleId).ToList();
            var franchiseVehicleOrders = OrderUtils.GetAllFranchiseOrders(franchiseVehicles, _shippingVehicleRouteService, _orderService)
                .Where(x => x.SelectedShippingDate >= initDate && x.SelectedShippingDate <= endDate)
                .ToList();

            var penaltiesVariables = _penaltiesVariablesService.GetAll().ToList();

            var orderIds = incident.OrderIds?.Split(',').Select(x => int.Parse(x)).ToList();
            List<NotDeliveredOrderItem> notDeliveredOrderItems = new List<NotDeliveredOrderItem>();

            if (orderIds != null)
                notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            if (franchiseVehicleOrders.Any())
            {
                total = IncidentWork(incident, costs, franchiseVehicleOrders, penaltiesVariables, notDeliveredOrderItems);
            }
            return total;
        }

        public void RecalculateIncidents(DateTime penaltyDate, string penaltyCustomId, string incidentCode = null)
        {
            var incidentsQuery = _incidentsService.GetAll().Where(x => x.Date >= penaltyDate);
            if (!string.IsNullOrWhiteSpace(incidentCode))
            {
                incidentsQuery = incidentsQuery.Where(x => x.IncidentCode == incidentCode);
            }

            var recentCost = _penaltiesCatalogService.GetAll()
                .Where(x => x.PenaltyCustomId == penaltyCustomId && x.ApplyDate > penaltyDate)
                .OrderBy(x => x.ApplyDate)
                .FirstOrDefault();

            if (recentCost != null)
                incidentsQuery = incidentsQuery.Where(x => x.Date < recentCost.ApplyDate);

            var incidents = incidentsQuery.ToList();
            var costs = _penaltiesCatalogService.GetAll().ToList();

            var orderIds = new List<int>();
            if (!incidents.Where(x => x.OrderIds == null).Any())
                orderIds = incidents.SelectMany(x => x.OrderIds.Split(',')).Select(x => int.Parse(x)).ToList();

            var orders = orderIds.Count == 0 ? new List<Order>() : OrderUtils.GetFilteredOrders(_orderService).Where(x => orderIds.Contains(x.Id)).ToList();
            var penaltiesVariables = _penaltiesVariablesService.GetAll().ToList();

            var notDeliveredOrderItems = orderIds.Count == 0 ? new List<NotDeliveredOrderItem>() : _notDeliveredOrderItemService.GetAll().Where(x => orderIds.Contains(x.OrderId)).ToList();

            foreach (var incident in incidents)
            {
                var total = IncidentWork(incident, costs, orders, penaltiesVariables, notDeliveredOrderItems);
                if (total != incident.Amount && total > 0)
                {
                    incident.Log += $"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")} - El monto de la incidencia cambió debido a una modificación que se hizo en la penalización o su variable. El monto cambió de {incident.Amount.ToString("C")} a {total.ToString("C")}\n";
                    incident.Amount = total;
                    _incidentsService.Update(incident);
                }
            }
        }

        public decimal IncidentWork(Incidents incident,
            List<PenaltiesCatalog> costs,
            List<Order> orders,
            List<PenaltiesVariables> penaltiesVariables,
            List<NotDeliveredOrderItem> notDeliveredOrderItems,
            decimal notDeliveredOrderItemPrice = 0)
        {
            var penaltyVariable = penaltiesVariables
                        .Where(x => x.IncidentCode == incident.IncidentCode && incident.Date >= x.ApplyDate)
                        .OrderByDescending(x => x.ApplyDate)
                        .FirstOrDefault();
            decimal total = 0;
            if (incident.IncidentCode != "Z00")
            {
                if (penaltyVariable == null) return 0;
                total = costs.Where(x => x.PenaltyCustomId == penaltyVariable.PenaltyCustomId)
                            .Where(x => incident.Date >= x.ApplyDate)
                            .OrderByDescending(x => x.ApplyDate).Select(x => x.Amount)
                            .FirstOrDefault() * penaltyVariable.Coefficient;
                var orderIds = incident.OrderIds?.Split(',').Select(x => int.Parse(x)).ToList();
                if (orderIds != null)
                    orders = orders.Where(x => orderIds.Contains(x.Id)).ToList();
            }
            switch (incident.IncidentCode)
            {
                case "R01":
                case "R02":
                    var orderItemIds = incident.OrderItemIds.Split(',')
                        .Select(x => int.Parse(x)).ToList();
                    if (notDeliveredOrderItemPrice > 0)
                        total += notDeliveredOrderItemPrice;
                    else
                    {
                        decimal itemPrice = orders.SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true))
                        .Where(x => orderItemIds.Contains(x.Id))
                        .Select(x => x.PriceInclTax).DefaultIfEmpty().Sum();
                        if (incident.SelectedQuantity != null && incident.SelectedQuantity > 0)
                        {
                            decimal amountWithSpecifiedQty = orders.SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true))
                            .Where(x => orderItemIds.Contains(x.Id))
                            .Select(x => _paymentService.CalculateGroceryPrice(x.Product, null, incident.SelectedQuantity ?? 0, false, x)).DefaultIfEmpty().Sum();
                            if (amountWithSpecifiedQty != itemPrice)
                            {
                                itemPrice = amountWithSpecifiedQty;
                                incident.AmountOfSelectedQuantity = amountWithSpecifiedQty;
                                incident.UnitePrice = orders.SelectMany(x => x.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService, notDeliveredOrderItems, false, true))
                                .Where(x => orderItemIds.Contains(x.Id))
                                .Select(x => _paymentService.CalculateGroceryPrice(x.Product, null, 1, false, x)).DefaultIfEmpty().Sum();
                            }
                        }
                        if (itemPrice > 0)
                            total += itemPrice;
                    }
                    break;
                case "R06":
                    var orderIds6 = incident.OrderIds.Split(',')
                        .Select(x => int.Parse(x)).ToList();
                    total += orders
                        .Where(x => orderIds6.Contains(x.Id))
                        .Select(x => x.OrderTotal + x.OrderSubTotalDiscountInclTax + (x.CustomerBalanceUsedAmount ?? 0)).DefaultIfEmpty().Sum();
                    break;
                case "R09":
                case "R10":
                case "D01":
                case "Z00":
                    total += incident.ReportedAmount ?? 0;
                    break;
                default:
                    break;
            }
            return total;
        }

        public bool CreateAutomaticBills(Franchise franchise = null)
        {
            var noErrors = true;
            var franchises = franchise == null ? _franchiseService.GetAll().ToList() : new List<Franchise>() { franchise };
            var franchiseIds = franchises.Select(x => x.Id).ToList();
            var vehicles = _shippingVehicleService.GetAll().Where(x => franchiseIds.Contains(x.FranchiseId)).ToList();
            var vehicleIds = _shippingVehicleService.GetAll().Where(x => franchiseIds.Contains(x.FranchiseId)).Select(x => x.Id).ToList();
            var bills = GetAll().Where(x => vehicleIds.Contains(x.VehicleId)).ToList();
            foreach (var vehicle in vehicles)
            {
                AutomaticBillingWorks(vehicle, bills);
            }
            return noErrors;
        }

        public void AutomaticBillingWorks(ShippingVehicle vehicle, List<Billing> bills)
        {
            var startOfWeek = DateTime.Today;
            var addDays = -((int)startOfWeek.DayOfWeek) + 1;
            if (addDays != 0) startOfWeek = startOfWeek.AddDays(addDays);
            var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
            RatesAndBonusesSettings settings = _settingService.LoadSetting<RatesAndBonusesSettings>();

            for (int i = 0; i < 12; i++)
            {
                startOfWeek = startOfWeek.AddDays(-7);
                endOfWeek = endOfWeek.AddDays(-7);

                var alreadyBilled = bills.Where(x => x.InitDate.Date == startOfWeek.Date && x.EndDate.Date == endOfWeek.Date && x.VehicleId == vehicle.Id).FirstOrDefault();
                var billed = GetBilled(vehicle, startOfWeek, endOfWeek);

                if (alreadyBilled != null && alreadyBilled.Billed != billed && billed > 0)
                {
                    alreadyBilled.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"Se actualizó de forma automática el monto de la comisión de {alreadyBilled.Billed} a {billed}.\n";
                    alreadyBilled.Billed = billed;
                    Update(alreadyBilled);
                }
                else if (alreadyBilled == null && billed > 0)
                {
                    var bill = new Billing
                    {
                        Billed = billed,
                        VehicleId = vehicle.Id,
                        InitDate = startOfWeek.Date,
                        EndDate = endOfWeek.Date,
                        StatusId = (int)PaymentStatus.Pending,
                        AmountAjustment = 0
                    };
                    Insert(bill);
                }
            }
        }

        //private void AutomaticBillingWorks(List<Billing> bills, Franchise franchise)
        //{
        //    var startOfWeek = DateTime.Today.AddDays(-7);
        //    var addDays = -((int)startOfWeek.DayOfWeek) + 1;
        //    if (addDays > 0 || addDays < 0)
        //        startOfWeek = startOfWeek.AddDays(addDays);
        //    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
        //    var billsOfFranchise = bills.Where(x => x.FranchiseId == franchise.Id);
        //    if (billsOfFranchise.Any())
        //    {
        //        var lastDate = billsOfFranchise
        //            .OrderByDescending(x => x.EndDate)
        //            .FirstOrDefault().EndDate;

        //        if (lastDate < endOfWeek)
        //        {
        //            var count = 0;
        //            do
        //            {
        //                if (count > 0)
        //                {
        //                    startOfWeek = startOfWeek.AddDays(-7);
        //                    endOfWeek = endOfWeek.AddDays(-7);
        //                }
        //                var billed = GetBilled(franchise, startOfWeek, endOfWeek);
        //                var bill = new Billing
        //                {
        //                    Billed = billed.Billed,
        //                    BonusAmount = billed.BonusAmount,
        //                    FranchiseId = franchise.Id,
        //                    InitDate = startOfWeek,
        //                    EndDate = endOfWeek,
        //                    StatusId = (int)BillingStatus.Pending,
        //                };
        //                Insert(bill);
        //                bill.Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la factura {bill.Id} (Creada con servicio automático).\n";
        //                Update(bill);
        //                count++;
        //            } while (endOfWeek.Date != lastDate.AddDays(-7).Date);
        //        }
        //    }
        //    else
        //    {
        //        var dateofCreation = franchise.CreatedOnUtc.ToLocalTime().Date;
        //        var addDaysCreation = -((int)dateofCreation.DayOfWeek) + 1;
        //        if (addDaysCreation > 0 || addDaysCreation < 0)
        //            dateofCreation = dateofCreation.AddDays(addDaysCreation);

        //        var creationStartOfWeek = dateofCreation.AddDays(-1 * (int)(dateofCreation.DayOfWeek) + 1);
        //        var creationEndOfWeek = creationStartOfWeek.AddDays(6);
        //        var count = 0;
        //        do
        //        {
        //            if (count > 0)
        //            {
        //                startOfWeek = startOfWeek.AddDays(-7);
        //                endOfWeek = endOfWeek.AddDays(-7);
        //            }
        //            var billed = GetBilled(franchise, startOfWeek, endOfWeek);
        //            var bill = new Billing
        //            {
        //                Billed = billed.Billed,
        //                BonusAmount = billed.BonusAmount,
        //                FranchiseId = franchise.Id,
        //                InitDate = startOfWeek,
        //                EndDate = endOfWeek,
        //                StatusId = (int)BillingStatus.Pending,
        //            };
        //            Insert(bill);
        //            bill.Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + _workContext.CurrentCustomer.GetFullName() + $" ({_workContext.CurrentCustomer.Id}) creó la factura {bill.Id} (Creada con servicio automático).\n";
        //            Update(bill);
        //            count++;
        //        } while (endOfWeek.Date != creationEndOfWeek.AddDays(-7).Date);
        //    }
        //}
    }
}