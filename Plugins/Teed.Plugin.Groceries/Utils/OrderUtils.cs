using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Controllers;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.Product;
using Teed.Plugin.Groceries.Domain.ShippingVehicles;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.Groceries.Models.Order;
using Teed.Plugin.Groceries.Models.ShippingRoute;
using Teed.Plugin.Groceries.Models.SimplePedido;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Settings;
using static Teed.Plugin.Groceries.Controllers.OrderDeliveryReportsController;

namespace Teed.Plugin.Groceries.Utils
{
    public static class OrderUtils
    {
        public static readonly List<DateTime> DisabledDates = new List<DateTime>()
            {
                new DateTime(2022, 1, 1),
                new DateTime(2022, 2, 7),
                new DateTime(2022, 3, 21),
                new DateTime(2022, 9, 16),
                new DateTime(2022, 11, 21),
                new DateTime(2022, 4, 15),
            };

        public static string GetPaymentOptionName(string paymentMethodSystemName)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodSystemName)) return "";
            return paymentMethodSystemName.Contains("Cash") ? "Efectivo contra entrega" :
                    paymentMethodSystemName.Contains("CardOnDelivery") ? "Tarjeta contra entrega" :
                    paymentMethodSystemName.Contains("Replacement") ? "Reposición" :
                    paymentMethodSystemName.Contains("MercadoPagoQr") ? "QR Mercadopago" :
                    paymentMethodSystemName.Contains("Benefits") ? "" :
                    paymentMethodSystemName.Contains("Visa") ? "Tarjeta en línea" :
                    paymentMethodSystemName.Contains("Paypal") ? "Paypal" :
                    paymentMethodSystemName;
        }

        public static IQueryable<Order> GetFilteredOrders(IOrderService orderService, bool includeCanceledWithRoute = false)
        {
            var query = orderService.GetAllOrdersQuery()
                .Where(x => !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatusId == 10));

            if (includeCanceledWithRoute)
                query = query.Where(x => x.RouteId > 0);
            else
                query = query.Where(x => x.OrderStatusId != 40);

            return query;
        }

        public static List<Order> GetOrdersInPedidoByOrder(Order order, IOrderService orderService)
        {
            return GetFilteredOrders(orderService).Where(x => x.CustomerId == order.CustomerId &&
            x.SelectedShippingDate == order.SelectedShippingDate &&
            x.ShippingAddress.Address1 == order.ShippingAddress.Address1)
                .ToList();
        }

        public static IQueryable<IGrouping<object, Order>> GetPedidosGroup(IQueryable<Order> orders)
        {
            return orders.GroupBy(x => new { x.CustomerId, x.SelectedShippingDate.Value, x.SelectedShippingTime, x.ShippingAddress.Address1 });
        }

        public static IEnumerable<IGrouping<dynamic, Order>> GetPedidosGroupByList(List<Order> orders)
        {
            return orders.GroupBy(x => new
            {
                x.CustomerId,
                SelectedShippingDate = x.SelectedShippingDate.Value,
                x.SelectedShippingTime,
                ShippingAddress = x.ShippingAddress.Address1
            });
        }

        public static IEnumerable<IGrouping<SimplePedidoBaseModel, T>> GetSimplePedidosGroupByList<T>(List<T> orders) where T : SimplePedidoBaseModel
        {
            return orders.GroupBy(x => new SimplePedidoBaseModel()
            {
                CustomerId = x.CustomerId,
                SelectedShippingDate = x.SelectedShippingDate,
                SelectedShippingTime = x.SelectedShippingTime,
                ShippingAddress = x.ShippingAddress
            });
        }

        public static IQueryable<Order> GetPedidosOnly(IQueryable<Order> orders)
        {
            return GetPedidosGroup(orders).Select(x => x.FirstOrDefault());
        }

        public static List<Order> GetMainOrderOfPedidoOnlyByList(List<Order> orders)
        {
            return GetPedidosGroupByList(orders).Select(x => x.OrderByDescending(y => y.OrderTotal).FirstOrDefault()).ToList();
        }

        public static IQueryable<Order> GetAllFranchiseOrders(List<ShippingVehicle> franchiseVehicles,
            ShippingVehicleRouteService shippingVehicleRouteService,
            IOrderService orderService)
        {
            List<int> franchiseVehicleIds = franchiseVehicles.Select(x => x.Id).ToList();
            var franchiseVehicleRoute = shippingVehicleRouteService.GetAll().Where(x => franchiseVehicleIds.Contains(x.VehicleId));
            var routeDateCombination = franchiseVehicleRoute.Select(x => DbFunctions.AddMilliseconds(x.ShippingDate, x.RouteId)).ToList();

            return GetFilteredOrders(orderService)
                .Where(x => routeDateCombination.Contains(DbFunctions.AddMilliseconds(x.SelectedShippingDate.Value, x.RouteId)));
        }

        public static ParsedShippingTimeModel ParseSelectedShippingTime(string selectedShippingTime)
        {
            switch (selectedShippingTime)
            {
                case "1:00 PM - 3:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(13, 0, 0),
                        EndTime = new TimeSpan(15, 0, 0)
                    };
                case "3:00 PM - 5:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(15, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0)
                    };
                case "5:00 PM - 7:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(17, 0, 0),
                        EndTime = new TimeSpan(19, 0, 0)
                    };
                case "7:00 PM - 9:00 PM":
                    return new ParsedShippingTimeModel()
                    {
                        InitTime = new TimeSpan(19, 0, 0),
                        EndTime = new TimeSpan(21, 0, 0)
                    };
                default:
                    return null;
            }
        }

        public static Order Clone(Order originalOrder)
        {
            var order = new Order()
            {
                Id = originalOrder.Id,
                SelectedShippingDate = originalOrder.SelectedShippingDate,
                SelectedShippingTime = originalOrder.SelectedShippingTime,
                CustomOrderNumber = originalOrder.CustomOrderNumber,
                ShippingAddress = originalOrder.ShippingAddress,
                AffiliateId = originalOrder.AffiliateId,
                AllowStoringCreditCardNumber = originalOrder.AllowStoringCreditCardNumber,
                AuthorizationTransactionCode = originalOrder.AuthorizationTransactionCode,
                AuthorizationTransactionId = originalOrder.AuthorizationTransactionId,
                AuthorizationTransactionResult = originalOrder.AuthorizationTransactionResult,
                BillingAddress = originalOrder.BillingAddress,
                BillingAddressId = originalOrder.BillingAddressId,
                CheckoutAttributeDescription = originalOrder.CheckoutAttributeDescription,
                CheckoutAttributesXml = originalOrder.CheckoutAttributesXml,
                PaymentMethodAdditionalFeeExclTax = originalOrder.PaymentMethodAdditionalFeeExclTax,
                PaymentMethodAdditionalFeeInclTax = originalOrder.PaymentMethodAdditionalFeeInclTax,
                PickupAddress = originalOrder.PickupAddress,
                PickupAddressId = originalOrder.PickupAddressId,
                RefundedAmount = originalOrder.RefundedAmount,
                ShippingAddressId = originalOrder.ShippingAddressId,
                BoxesDistribution = originalOrder.BoxesDistribution,
                CaptureTransactionId = originalOrder.CaptureTransactionId,
                CaptureTransactionResult = originalOrder.CaptureTransactionResult,
                CardCvv2 = originalOrder.CardCvv2,
                CardExpirationMonth = originalOrder.CardExpirationMonth,
                CardExpirationYear = originalOrder.CardExpirationYear,
                CardName = originalOrder.CardName,
                CardNumber = originalOrder.CardNumber,
                CardType = originalOrder.CardType,
                CreatedOnUtc = originalOrder.CreatedOnUtc,
                CurrencyRate = originalOrder.CurrencyRate,
                Customer = originalOrder.Customer,
                CustomerCurrencyCode = originalOrder.CustomerCurrencyCode,
                CustomerId = originalOrder.CustomerId,
                CustomerIp = originalOrder.CustomerIp,
                CustomerLanguageId = originalOrder.CustomerLanguageId,
                RouteId = originalOrder.RouteId,
                RouteDisplayOrder = originalOrder.RouteDisplayOrder,
                CustomerTaxDisplayType = originalOrder.CustomerTaxDisplayType,
                CustomerTaxDisplayTypeId = originalOrder.CustomerTaxDisplayTypeId,
                RescuedByRouteId = originalOrder.RescuedByRouteId,
                RescueRouteDisplayOrder = originalOrder.RescueRouteDisplayOrder,
                CustomValuesXml = originalOrder.CustomValuesXml,
                Deleted = originalOrder.Deleted,
                MaskedCreditCardNumber = originalOrder.MaskedCreditCardNumber,
                MessageGift = originalOrder.MessageGift,
                OrderDiscount = originalOrder.OrderDiscount,
                OrderGuid = originalOrder.OrderGuid,
                OrderShippingExclTax = originalOrder.OrderShippingExclTax,
                OrderShippingInclTax = originalOrder.OrderShippingInclTax,
                OrderStatus = originalOrder.OrderStatus,
                OrderStatusId = originalOrder.OrderStatusId,
                OrderSubTotalDiscountExclTax = originalOrder.OrderSubTotalDiscountExclTax,
                OrderSubTotalDiscountInclTax = originalOrder.OrderSubTotalDiscountInclTax,
                OrderSubtotalExclTax = originalOrder.OrderSubtotalExclTax,
                OrderSubtotalInclTax = originalOrder.OrderSubtotalInclTax,
                OrderTax = originalOrder.OrderTax,
                OrderTotal = originalOrder.OrderTotal,
                OriginalOrderTotal = originalOrder.OriginalOrderTotal,
                PaidDateUtc = originalOrder.PaidDateUtc,
                PaymentMethodSystemName = originalOrder.PaymentMethodSystemName,
                PaymentParameter = originalOrder.PaymentParameter,
                PaymentStatus = originalOrder.PaymentStatus,
                PaymentStatusId = originalOrder.PaymentStatusId,
                PickUpInStore = originalOrder.PickUpInStore,
                PreviousPointTransferDistance = originalOrder.PreviousPointTransferDistance,
                PreviousPointTransferTime = originalOrder.PreviousPointTransferTime,
                RedeemedRewardPointsEntry = originalOrder.RedeemedRewardPointsEntry,
                RewardPointsHistoryEntryId = originalOrder.RewardPointsHistoryEntryId,
                ShippingMethod = originalOrder.ShippingMethod,
                ShippingRateComputationMethodSystemName = originalOrder.ShippingRateComputationMethodSystemName,
                ShippingStatus = originalOrder.ShippingStatus,
                ShippingStatusId = originalOrder.ShippingStatusId,
                SignsGift = originalOrder.SignsGift,
                StoreId = originalOrder.StoreId,
                SubscriptionTransactionId = originalOrder.SubscriptionTransactionId,
                TaxRates = originalOrder.TaxRates,
                VatNumber = originalOrder.VatNumber,
                ZoneId = originalOrder.ZoneId
            };

            foreach (var item in originalOrder.OrderItems)
                order.OrderItems.Add(item);

            foreach (var item in originalOrder.Shipments)
                order.Shipments.Add(item);

            return order;
        }

        public static DailyOrdersModel GetTimesPedidosData(DateTime selectedDate,
            ISettingService _settingService,
            IOrderService _orderService,
            ShippingRegionZoneService _shippingRegionZoneService,
            ShippingZoneService _shippingZoneService,
            CorcelCustomerService _corcelCustomerService,
            bool getPins = false)
        {
            var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();
            var monitorSettings = _settingService.LoadSetting<GoalsTodaySettings>();

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService);
            var pedidos = OrderUtils.GetPedidosOnly(ordersQuery.Where(x => x.SelectedShippingDate == selectedDate)).ToList();

            var ordersTime1 = pedidos.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM").Count();
            var ordersTime2 = pedidos.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM").Count();
            var ordersTime3 = pedidos.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM").Count();
            var ordersTime4 = pedidos.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM").Count();
            var total = ordersTime1 + ordersTime2 + ordersTime3 + ordersTime4;

            var regions = _shippingRegionZoneService.GetAll().GroupBy(x => x.Region).ToList();
            var regionListModel = new List<OrdersByRegion>();
            foreach (var region in regions)
            {
                var postalCodes = region.Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes)
                    .Select(x => x.Split(','))
                    .SelectMany(x => x)
                    .Select(x => x.Trim())
                    .ToList();

                var regionOrders = pedidos.Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode));
                var regionOrdersTime1 = regionOrders.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM").Count();
                var regionOrdersTime2 = regionOrders.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM").Count();
                var regionOrdersTime3 = regionOrders.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM").Count();
                var regionOrdersTime4 = regionOrders.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM").Count();

                var regionTotalLimit = region.Key.Schedule1Quantity + region.Key.Schedule2Quantity + region.Key.Schedule3Quantity + region.Key.Schedule4Quantity;
                var regionTotal = regionOrdersTime1 + regionOrdersTime2 + regionOrdersTime3 + regionOrdersTime4;

                regionListModel.Add(new OrdersByRegion()
                {
                    RegionId = region.Key.Id,
                    RegionName = region.Key.Name,
                    OrdersTime1 = regionOrdersTime1,
                    OrdersTime2 = regionOrdersTime2,
                    OrdersTime3 = regionOrdersTime3,
                    OrdersTime4 = regionOrdersTime4,
                    OrdersTotal = regionTotal,
                    OrdersTime1Limit = region.Key.Schedule1Quantity,
                    OrdersTime2Limit = region.Key.Schedule2Quantity,
                    OrdersTime3Limit = region.Key.Schedule3Quantity,
                    OrdersTime4Limit = region.Key.Schedule4Quantity,
                    OrdersTotalLimit = regionTotalLimit,
                    RegionGoal = DashboardUtils.GetGoal(regionTotalLimit),
                    RegionGoalColor = DashboardUtils.GetGoalColor(regionTotal, regionTotalLimit)
                });
            }

            var ordersTotalLimit = GetGoalByDate(selectedDate, monitorSettings);
            if (ordersTotalLimit == 0)
                ordersTotalLimit = globalSchedule.Schedule1Quantity + globalSchedule.Schedule2Quantity + globalSchedule.Schedule3Quantity + globalSchedule.Schedule4Quantity;
            var controlDate = DashboardUtils.GetControlTime();

            var orderPins = new List<MapOrderData>();
            if (getPins)
            {
                var zones = _shippingZoneService.GetAll().ToList();
                var clientIds = pedidos.Select(x => x.CustomerId).Distinct().ToList();
                var ordersOfClients = ordersQuery.Where(x => clientIds.Contains(x.CustomerId)).ToList();
                orderPins = OrderUtils.GetPedidosGroupByList(pedidos).Select(y => new MapOrderData
                {
                    OrderIds = string.Join(",", y.Select(z => z.Id.ToString()).ToList()),
                    //ShippingAddress = y.FirstOrDefault()?.ShippingAddress?.Address1,
                    //ZoneName = y.FirstOrDefault().ZoneId.HasValue ? zones.Where(z => z.Id == y.FirstOrDefault().ZoneId).Select(z => z.ZoneName).FirstOrDefault() : null,
                    //OrderNumber = string.Join(", ", y.Select(z => "Orden #" + z.CustomOrderNumber).ToList()),
                    //PostalCode = y.FirstOrDefault().ShippingAddress.ZipPostalCode,
                    //ShippingFullName = y.FirstOrDefault().ShippingAddress.FirstName + " " + y.FirstOrDefault().ShippingAddress.LastName,
                    SelectedShippingTime = y.FirstOrDefault().SelectedShippingTime,
                    Latitude = y.FirstOrDefault().ShippingAddress.Latitude,
                    Longitude = y.FirstOrDefault().ShippingAddress.Longitude,
                    LastOrderCount = ordersOfClients.Where(x => x.CustomerId == y.FirstOrDefault().CustomerId).Count(),
                }).ToList();
            }

            return new DailyOrdersModel()
            {
                OrdersTime1 = ordersTime1,
                OrdersTime2 = ordersTime2,
                OrdersTime3 = ordersTime3,
                OrdersTime4 = ordersTime4,
                OrdersTime1Limit = globalSchedule.Schedule1Quantity,
                OrdersTime2Limit = globalSchedule.Schedule2Quantity,
                OrdersTime3Limit = globalSchedule.Schedule3Quantity,
                OrdersTime4Limit = globalSchedule.Schedule4Quantity,
                OrdersTotal = total,
                OrdersTotalLimit = ordersTotalLimit,
                OrdersTime1Color = DashboardUtils.GetGoalColor(ordersTime1, globalSchedule.Schedule1Quantity),
                OrdersTime2Color = DashboardUtils.GetGoalColor(ordersTime2, globalSchedule.Schedule2Quantity),
                OrdersTime3Color = DashboardUtils.GetGoalColor(ordersTime3, globalSchedule.Schedule3Quantity),
                OrdersTime4Color = DashboardUtils.GetGoalColor(ordersTime4, globalSchedule.Schedule4Quantity),
                OrdersTimeTotalColor = DashboardUtils.GetGoalColor(total, ordersTotalLimit),
                Regions = regionListModel,
                SelectedDate = selectedDate,
                AllRegionsGoalColor = DashboardUtils.GetGoalColor(regionListModel.Select(x => x.OrdersTime1 + x.OrdersTime2 + x.OrdersTime3 + x.OrdersTime4).DefaultIfEmpty().Sum(), regionListModel.Select(x => x.OrdersTime1Limit + x.OrdersTime2Limit + x.OrdersTime3Limit + x.OrdersTime4Limit).DefaultIfEmpty().Sum()),
                CurrentTime = controlDate.ToString("HH:00"),
                TimePercentaje = DashboardUtils.GetCurrentTimePercentaje(controlDate),
                OrderPins = orderPins,
                WeeklyCorcelCustomers = GetWeeklyCorcelCustomersModel(_orderService, _corcelCustomerService),
            };
        }

        public static DailyAmountsModel GetTimesSellsData(DateTime selectedDate,
            ISettingService _settingService,
            IOrderService _orderService,
            ShippingRegionZoneService _shippingRegionZoneService,
            ShippingZoneService _shippingZoneService,
            CorcelCustomerService _corcelCustomerService,
            bool getPins = false)
        {
            int amountPerOrder = 1100;
            var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate == selectedDate);
            var pedidos = OrderUtils.GetPedidosOnly(ordersQuery).ToList();

            var amountTime1 = pedidos.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
            var amountTime2 = pedidos.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
            var amountTime3 = pedidos.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
            var amountTime4 = pedidos.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
            var total = amountTime1 + amountTime2 + amountTime3 + amountTime4;

            var regions = _shippingRegionZoneService.GetAll().GroupBy(x => x.Region).ToList();
            var regionListModel = new List<AmountsByRegion>();
            foreach (var region in regions)
            {
                var postalCodes = region.Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes)
                    .Select(x => x.Split(','))
                    .SelectMany(x => x)
                    .Select(x => x.Trim())
                    .ToList();

                var regionOrders = pedidos.Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode));
                var regionAmountTime1 = regionOrders.Where(x => x.SelectedShippingTime == "1:00 PM - 3:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                var regionAmountTime2 = regionOrders.Where(x => x.SelectedShippingTime == "3:00 PM - 5:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                var regionAmountTime3 = regionOrders.Where(x => x.SelectedShippingTime == "5:00 PM - 7:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();
                var regionAmountTime4 = regionOrders.Where(x => x.SelectedShippingTime == "7:00 PM - 9:00 PM").Select(x => x.OrderTotal).DefaultIfEmpty().Sum();

                var regionTotalLimit = (region.Key.Schedule1Quantity + region.Key.Schedule2Quantity + region.Key.Schedule3Quantity + region.Key.Schedule4Quantity) * amountPerOrder;
                var regionTotal = regionAmountTime1 + regionAmountTime2 + regionAmountTime3 + regionAmountTime4;

                regionListModel.Add(new AmountsByRegion()
                {
                    RegionId = region.Key.Id,
                    RegionName = region.Key.Name,
                    OrdersTime1 = regionAmountTime1,
                    OrdersTime2 = regionAmountTime2,
                    OrdersTime3 = regionAmountTime3,
                    OrdersTime4 = regionAmountTime4,
                    OrdersTotal = regionTotal,
                    OrdersTime1Limit = region.Key.Schedule1Quantity * amountPerOrder,
                    OrdersTime2Limit = region.Key.Schedule2Quantity * amountPerOrder,
                    OrdersTime3Limit = region.Key.Schedule3Quantity * amountPerOrder,
                    OrdersTime4Limit = region.Key.Schedule4Quantity * amountPerOrder,
                    OrdersTotalLimit = regionTotalLimit,
                    RegionGoal = DashboardUtils.GetGoal(regionTotalLimit),
                    RegionGoalColor = DashboardUtils.GetGoalColor(regionTotal, regionTotalLimit)
                });
            }

            var ordersTotalLimit = (globalSchedule.Schedule1Quantity + globalSchedule.Schedule2Quantity + globalSchedule.Schedule3Quantity + globalSchedule.Schedule4Quantity) * amountPerOrder;
            var controlDate = DashboardUtils.GetControlTime();

            var orderPins = new List<MapOrderData>();
            if (getPins)
            {
                var zones = _shippingZoneService.GetAll().ToList();
                var clientIds = pedidos.Select(x => x.CustomerId).Distinct().ToList();
                orderPins = OrderUtils.GetPedidosGroupByList(pedidos).Select(y => new MapOrderData
                {
                    OrderIds = string.Join(",", y.Select(z => z.Id.ToString()).ToList()),
                    //ShippingAddress = y.FirstOrDefault()?.ShippingAddress?.Address1,
                    //ZoneName = y.FirstOrDefault().ZoneId.HasValue ? zones.Where(z => z.Id == y.FirstOrDefault().ZoneId).Select(z => z.ZoneName).FirstOrDefault() : null,
                    //OrderNumber = string.Join(", ", y.Select(z => "Orden #" + z.CustomOrderNumber).ToList()),
                    //PostalCode = y.FirstOrDefault().ShippingAddress.ZipPostalCode,
                    //ShippingFullName = y.FirstOrDefault().ShippingAddress.FirstName + " " + y.FirstOrDefault().ShippingAddress.LastName,
                    SelectedShippingTime = y.FirstOrDefault().SelectedShippingTime,
                    Latitude = y.FirstOrDefault().ShippingAddress.Latitude,
                    Longitude = y.FirstOrDefault().ShippingAddress.Longitude,
                    OrdersTotal = y.Select(x => x.OrderTotal).DefaultIfEmpty().Sum(),
                }).ToList();
            }

            return new DailyAmountsModel()
            {
                OrdersTime1 = amountTime1,
                OrdersTime2 = amountTime2,
                OrdersTime3 = amountTime3,
                OrdersTime4 = amountTime4,
                OrdersTime1Limit = globalSchedule.Schedule1Quantity * amountPerOrder,
                OrdersTime2Limit = globalSchedule.Schedule2Quantity * amountPerOrder,
                OrdersTime3Limit = globalSchedule.Schedule3Quantity * amountPerOrder,
                OrdersTime4Limit = globalSchedule.Schedule4Quantity * amountPerOrder,
                OrdersTotal = total,
                OrdersTotalLimit = ordersTotalLimit,
                OrdersTime1Color = DashboardUtils.GetGoalColor(amountTime1, globalSchedule.Schedule1Quantity * amountPerOrder),
                OrdersTime2Color = DashboardUtils.GetGoalColor(amountTime2, globalSchedule.Schedule2Quantity * amountPerOrder),
                OrdersTime3Color = DashboardUtils.GetGoalColor(amountTime3, globalSchedule.Schedule3Quantity * amountPerOrder),
                OrdersTime4Color = DashboardUtils.GetGoalColor(amountTime4, globalSchedule.Schedule4Quantity * amountPerOrder),
                OrdersTimeTotalColor = DashboardUtils.GetGoalColor(total, ordersTotalLimit),
                Regions = regionListModel,
                SelectedDate = selectedDate,
                AllRegionsGoalColor = DashboardUtils.GetGoalColor(regionListModel.Select(x => x.OrdersTime1 + x.OrdersTime2 + x.OrdersTime3 + x.OrdersTime4).DefaultIfEmpty().Sum(), regionListModel.Select(x => (x.OrdersTime1Limit + x.OrdersTime2Limit + x.OrdersTime3Limit + x.OrdersTime4Limit) * amountPerOrder).DefaultIfEmpty().Sum()),
                CurrentTime = controlDate.ToString("HH:00"),
                TimePercentaje = DashboardUtils.GetCurrentTimePercentaje(controlDate),
                OrderPins = orderPins,
                WeeklyCorcelCustomers = GetWeeklyCorcelCustomersModel(_orderService, _corcelCustomerService),
            };
        }

        public static WeeklyCorcelCustomersModel GetWeeklyCorcelCustomersModel(IOrderService _orderService,
            CorcelCustomerService _corcelCustomerService)
        {
            var weeklyCorcelCustomersInfos = new List<WeeklyCorcelCustomersInfoModel>();

            var currentWeekDates = DateUtils.CampusVueDateRange(DateTime.Now.Date, DayOfWeek.Monday.ToString());
            var startOfThisWeek = currentWeekDates.Item1;
            var endOfThisWeek = currentWeekDates.Item2;
            var date26weeksAgo = startOfThisWeek.AddDays(-175);

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate != null && date26weeksAgo <= x.SelectedShippingDate && x.SelectedShippingDate <= endOfThisWeek)
                .Select(x => new { x.Id, x.SelectedShippingDate, x.CustomerId })
                .ToList();

            var allCustomersWithOrderIds = new List<int>();

            for (DateTime i = date26weeksAgo; i < endOfThisWeek; i = i.AddDays(7))
            {
                var startOfWeek = i.Date;
                var endOfWeek = i.Date.AddDays(6);

                var orderIds = orders
                .Where(x => x.SelectedShippingDate != null && startOfWeek <= x.SelectedShippingDate && x.SelectedShippingDate <= endOfWeek)
                .Select(x => x.Id)
                .ToList();

                var customersWithOrderIds = _corcelCustomerService.GetAll()
                    .Where(x => orderIds.Contains(x.OrderId)).ToList();

                if (customersWithOrderIds.Any())
                    allCustomersWithOrderIds.AddRange(customersWithOrderIds.Select(x => x.Id).ToList());

                weeklyCorcelCustomersInfos.Add(new WeeklyCorcelCustomersInfoModel
                {
                    StartOfWeek = startOfWeek.ToString("d/M/yy"),
                    EndOfWeek = endOfWeek.ToString("d/M/yy"),
                    AmountOfNewCustomers = customersWithOrderIds.Count(),
                });
            }

            var customersWithOrderIdsCount = _corcelCustomerService.GetAll()
                .Where(x => !allCustomersWithOrderIds.Contains(x.Id)).Count();

            return new WeeklyCorcelCustomersModel
            {
                AmountOfCustomers = customersWithOrderIdsCount,
                WeeklyCorcelCustomersInfos = weeklyCorcelCustomersInfos
            };
        }

        public static bool CheckIfDeliveredInTime(string selectedShippingTime, DateTime deliveryTime)
        {
            TimeSpan endTime = default;
            if (selectedShippingTime == "1:00 PM - 3:00 PM")
                endTime = new TimeSpan(15, 15, 0);
            else if (selectedShippingTime == "3:00 PM - 5:00 PM")
                endTime = new TimeSpan(17, 15, 0);
            else if (selectedShippingTime == "5:00 PM - 7:00 PM")
                endTime = new TimeSpan(19, 15, 0);
            else if (selectedShippingTime == "7:00 PM - 9:00 PM")
                endTime = new TimeSpan(21, 15, 0);

            return deliveryTime.TimeOfDay <= endTime;
        }

        private static int GetGoalByDate(DateTime currentDate, GoalsTodaySettings settings)
        {
            switch (currentDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 0;
                case DayOfWeek.Monday:
                    return settings.GoalsMonday;
                case DayOfWeek.Tuesday:
                    return settings.GoalsTuesday;
                case DayOfWeek.Wednesday:
                    return settings.GoalsWednesday;
                case DayOfWeek.Thursday:
                    return settings.GoalsThursday;
                case DayOfWeek.Friday:
                    return settings.GoalsFriday;
                case DayOfWeek.Saturday:
                    return settings.GoalsSaturday;
                default:
                    return 0;
            }

        }

        public static List<SelectListItem> GetAllowedQuantities(int orderItemId, IOrderService orderService, int maxElements = 50)
        {
            OrderItem item = orderService.GetOrderItemById(orderItemId);
            if (item == null) return null;
            var itemDto = GetOrderItemDto(item, null, null, new List<int>(), null);
            if (itemDto == null) return null;
            return GetAllowedQuantitiesItems(itemDto, maxElements);
        }

        public static List<SelectListItem> GetAllowedQuantities(int orderItemId, IOrderService orderService, out int originalQuantity,
            NotDeliveredOrderItemService notDeliveredOrderItemService = null, bool returnOrderItemQuantity = false, int maxElements = 50)
        {
            originalQuantity = 1;
            OrderItemDto item = null;
            var orderItem = orderService.GetOrderItemById(orderItemId);
            if (orderItem == null && returnOrderItemQuantity)
            {
                var notDeliveredOrderItem = notDeliveredOrderItemService.GetAll().Where(x => x.OrderItemId == orderItemId).FirstOrDefault();
                if (notDeliveredOrderItem != null)
                    item = GetOrderItemDto(null, notDeliveredOrderItem, null, new List<int>(), null);
            }
            else if (orderItem != null)
            {
                item = GetOrderItemDto(orderItem, null, null, new List<int>(), null);
            }
            if (item == null) return null;
            originalQuantity = item.RawQuantity;
            return GetAllowedQuantitiesItems(item, maxElements);
        }

        public static List<SelectListItem> GetAllowedQuantitiesItems(OrderItemDto item, int maxElements = 50)
        {
            int max = maxElements;
            List<SelectListItem> data = new List<SelectListItem>();
            if (item.BuyingBySecondary && item.EquivalenceCoefficient > 0)
            {
                for (int i = 1; i <= max; i++)
                {
                    string unit = " gr";
                    var value = Math.Round((i * 1000) / item.EquivalenceCoefficient, 2);
                    if (value >= 1000)
                    {
                        value = Math.Round(value / 1000, 2);
                        unit = " kg";
                    }

                    data.Add(new SelectListItem()
                    {
                        Value = i.ToString(),
                        Text = value + unit
                    });
                }
            }
            else if (item.WeightInterval > 0)
            {
                for (int i = 1; i <= max; i++)
                {
                    string unit = " gr";
                    var value = Math.Round(i * item.WeightInterval, 2);
                    if (value >= 1000)
                    {
                        value = Math.Round(value / 1000, 2);
                        unit = " kg";
                    }

                    data.Add(new SelectListItem()
                    {
                        Value = i.ToString(),
                        Text = value + unit
                    });
                }
            }
            else
            {
                for (int i = 1; i <= max; i++)
                {
                    string unit = " pz";
                    data.Add(new SelectListItem()
                    {
                        Value = i.ToString(),
                        Text = i + " " + unit
                    });
                }
            }
            return data;
        }

        public static decimal GetTotalQuantity(decimal quantity,
            decimal equivalenceCoefficient,
            bool buyingBySecondary,
            decimal weightInterval)
        {
            decimal result = quantity;
            if (equivalenceCoefficient > 0 && buyingBySecondary)
                result = ((quantity * 1000) / equivalenceCoefficient) / 1000;
            else if (weightInterval > 0)
                result = (quantity * weightInterval) / 1000;
            return Math.Round(result, 2);
        }

        public static string GetProductRequestedUnit(decimal equivalenceCoefficient, bool buyingBySecondary, decimal weightInterval)
        {
            return ((equivalenceCoefficient > 0 && buyingBySecondary) || weightInterval > 0) ? "kg" : "pz";
        }

        public static decimal GetUpdatedPrice(OrderItem orderItem, decimal originalQty)
        {
            int qty = orderItem.Quantity;
            var parsedUnitPrice = ParseUnitPrice(orderItem, originalQty);
            if (orderItem.EquivalenceCoefficient > 0)
                return Math.Round((qty * parsedUnitPrice) / orderItem.EquivalenceCoefficient, 2);
            else if (orderItem.WeightInterval > 0)
                return Math.Round(((qty * orderItem.WeightInterval) * parsedUnitPrice) / 1000, 2);
            else
                return Math.Round(qty * orderItem.UnitPriceInclTax, 2);
        }

        public static decimal ParseUnitPrice(OrderItem orderItem, decimal originalQty)
        {
            if ((orderItem.EquivalenceCoefficient > 0 && orderItem.BuyingBySecondary) || orderItem.WeightInterval > 0)
                return orderItem.UnitPriceInclTax / originalQty;
            else if (orderItem.EquivalenceCoefficient > 0 && !orderItem.BuyingBySecondary)
                return (orderItem.EquivalenceCoefficient * orderItem.UnitPriceInclTax) / originalQty;
            else
                return orderItem.UnitPriceInclTax;
        }

        public static BuyerAmountModel GetBuyerCashAndTransferAmount(List<OrderItem> parsedOrderItems,
            int buyerId,
            List<OrderItemBuyer> orderItemBuyer,
            List<Manufacturer> manufacturers,
            List<ProductMainManufacturer> productMainManufacturers)
        {
            var manufacturerIdsWithTransfer = manufacturers.Where(x => x.IsPaymentWhithTransfer).Select(x => x.Id).ToList();
            var manufacturerIdsWithCard = manufacturers.Where(x => x.IsPaymentWhithCorporateCard).Select(x => x.Id).ToList();
            var filteredItems = parsedOrderItems.Select(x => new GetBuyerCashAndTransferAmountModel()
            {
                OrderItemId = x.Id,
                MainManufacturerId = ProductUtils.GetMainManufacturerId(x.Product.ProductManufacturers, productMainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault())
            }).ToList();

            var orderItemIdsWithTransferManufacturer = filteredItems.Where(x => manufacturerIdsWithTransfer.Contains(x.MainManufacturerId)).Select(x => x.OrderItemId).ToList();
            var orderItemIdsWithCardManufacturer = filteredItems.Where(x => manufacturerIdsWithCard.Contains(x.MainManufacturerId)).Select(x => x.OrderItemId).ToList();
            decimal amountInCash = orderItemBuyer.Where(x => x.CustomerId == buyerId && !orderItemIdsWithTransferManufacturer.Contains(x.OrderItemId) &&
                                            !orderItemIdsWithCardManufacturer.Contains(x.OrderItemId))
                                            .Select(x => x.Cost).DefaultIfEmpty().Sum();

            var controlDate = parsedOrderItems.Where(x => x.Order != null).Select(x => x.Order.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
            if (buyerId == 4076622 && controlDate > new DateTime(2021, 12, 22)) //csuper@centralenlinea.com, the date is when we added the extra cash
                amountInCash += 1000;

            decimal amountToTransfer = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWithTransferManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            decimal amountInCard = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWithCardManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            return new BuyerAmountModel() { Cash = amountInCash, Transfer = amountToTransfer, Card = amountInCard, BuyerId = buyerId };
        }

        public static BuyerAmountModel GetBuyerCashAndTransferAmountWithGivenBoughtTypes(List<OrderItem> parsedOrderItems,
            int buyerId,
            List<OrderItemBuyer> orderItemBuyer,
            List<Manufacturer> manufacturers,
            List<ProductMainManufacturer> productMainManufacturers,
            List<OrderItemBoughtType> orderItemBoughtTypes)
        {
            var productIdsWithTransfer = orderItemBoughtTypes.Where(x => x.BoughtType == BoughtType.Transfer).Select(x => x.ProductId).ToList();
            var productIdsWithCard = orderItemBoughtTypes.Where(x => x.BoughtType == BoughtType.CorporateCard).Select(x => x.ProductId).ToList();
            var productIdsWithCash = orderItemBoughtTypes.Where(x => x.BoughtType == BoughtType.Cash).Select(x => x.ProductId).ToList();

            var filteredItems = parsedOrderItems.Select(x => new GetBuyerCashAndTransferAmountModel()
            {
                ProductId = x.ProductId,
                OrderItemId = x.Id,
                MainManufacturerId = ProductUtils.GetMainManufacturerId(x.Product.ProductManufacturers, productMainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault())
            }).ToList();

            var orderItemIdsWithTransferManufacturer = filteredItems.Where(x => productIdsWithTransfer.Contains(x.ProductId)).Select(x => x.OrderItemId).ToList();
            var orderItemIdsWithCardManufacturer = filteredItems.Where(x => productIdsWithCard.Contains(x.ProductId)).Select(x => x.OrderItemId).ToList();
            decimal amountInCash = orderItemBuyer.Where(x => x.CustomerId == buyerId && !orderItemIdsWithTransferManufacturer.Contains(x.OrderItemId) &&
                                        !orderItemIdsWithCardManufacturer.Contains(x.OrderItemId))
                                        .Select(x => x.Cost).DefaultIfEmpty().Sum();

            var controlDate = parsedOrderItems.Where(x => x.Order != null).Select(x => x.Order.SelectedShippingDate.Value).OrderBy(x => x).FirstOrDefault();
            if (buyerId == 4076622 && controlDate > new DateTime(2021, 12, 22)) //csuper@centralenlinea.com, the date is when we added the extra cash
                amountInCash += 1000;

            decimal amountToTransfer = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWithTransferManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            decimal amountInCard = orderItemBuyer.Where(x => x.CustomerId == buyerId && orderItemIdsWithCardManufacturer.Contains(x.OrderItemId))
                                     .Select(x => x.Cost).DefaultIfEmpty().Sum();

            return new BuyerAmountModel() { Cash = amountInCash, Transfer = amountToTransfer, Card = amountInCard, BuyerId = buyerId };
        }

        public static int GetBoughtTypeId(Manufacturer manufacturer, int? boughtTypeId)
        {
            var id = (int)BoughtType.Cash;
            if (manufacturer != null && boughtTypeId == null)
            {
                if (!manufacturer.IsPaymentWhithTransfer && !manufacturer.IsPaymentWhithCorporateCard)
                    id = (int)BoughtType.Cash;
                else if (manufacturer.IsPaymentWhithTransfer && !manufacturer.IsPaymentWhithCorporateCard)
                    id = (int)BoughtType.Transfer;
                else if (!manufacturer.IsPaymentWhithTransfer && manufacturer.IsPaymentWhithCorporateCard)
                    id = (int)BoughtType.CorporateCard;
                else
                    id = (int)BoughtType.Cash;
            }
            else
                id = boughtTypeId ?? (int)BoughtType.Cash;

            return id;
        }

        private static decimal RoundBuyerCashAmount(decimal costSum)
        {
            if (costSum == 0) return 0;
            costSum += 50;
            costSum = Math.Ceiling(costSum / 100) * 100;
            return costSum;
        }

        public static List<string> GetOrderSuspiciousElements(Order order, List<Order> customerOrders, ICustomerService customerService)
        {
            var result = new List<string>();
            var pedidos = GetPedidosGroupByList(customerOrders).ToList();
            bool isFirstPedido = pedidos.Count == 1;
            bool isVisaPayment = order.PaymentMethodSystemName == "Payments.Visa";
            decimal orderTotal = order.OrderTotal;
            bool duplicatedPhoneNumber = customerService.GetAllCustomers(phone: order.ShippingAddress.PhoneNumber).Count() > 1;
            int visaScore = isVisaPayment ? GetVisaScore(order.OrderNotes) : 0;
            bool hasVisaScore = isVisaPayment ? HasVisaScore(order.OrderNotes) : false;
            bool onlyOneProductType = order.OrderItems.GroupBy(x => x.ProductId).Count() == 1;
            bool onDeliveryPaymentMethod = order.PaymentMethodSystemName != null ? order.PaymentMethodSystemName.Contains("OnDelivery") : false;
            bool dangerousCategory = order.OrderItems.Where(x => !x.Product.GiftProductEnable && x.Product.ProductCategories.Where(y => y.CategoryId == 152 || y.Category.ParentCategoryId == 152 || y.CategoryId == 106).Any()).Any();
            bool previousOrderLessThanAWeek = false;

            DateTime? previousOrderDate = customerOrders
                .Where(x => x.SelectedShippingDate < order.SelectedShippingDate)
                .Select(x => x.SelectedShippingDate)
                .OrderByDescending(x => x)
                .FirstOrDefault();
            if (previousOrderDate.HasValue)
            {
                previousOrderLessThanAWeek = (order.SelectedShippingDate.Value - previousOrderDate.Value).TotalDays < 7;
            }

            if (isVisaPayment && visaScore > 9)
            {
                result.Add("El pago de la orden fue a través de Cybersource y el score de fraude es mayor o igual a 10.");
            };

            if (isVisaPayment && !hasVisaScore)
            {
                result.Add("La orden no cuenta con notas de orden con verificación de VISA por Cybersource, posible fraude.");
            };

            if (duplicatedPhoneNumber)
            {
                result.Add("El número de teléfono ha sido utilizado por otro cliente.");
            };

            if (isFirstPedido && isVisaPayment && visaScore > 30)
            {
                result.Add("Es primer pedido del cliente y la puntuación de VISA está alta.");
            }

            if (onlyOneProductType && orderTotal > 3000 && isFirstPedido && (visaScore > 20 || visaScore == 0))
            {
                result.Add("Es primer pedido del cliente, la orden solo tiene un producto y el total de la orden es mayor a 3,000 MXN.");
            }

            if (onDeliveryPaymentMethod && isFirstPedido && orderTotal > 3000)
            {
                result.Add("Es primer pedido del cliente, el pago es contra entrega y el total de la orden es mayor a 3,000 MXN.");
            }

            if (dangerousCategory && isFirstPedido && orderTotal > 1000)
            {
                result.Add("Es primer pedido del cliente, tiene productos de una categoría considerada de riesgo (bebidas alcohólicas) y el total de la orden es mayor a 1,000 MXN.");
            }

            if (previousOrderLessThanAWeek && isVisaPayment && pedidos.Count <= 3 && order.DiscountUsageHistory.Count == 0)
            {
                result.Add("El cliente hizo una orden hace menos de 1 semana, tiene menos de 4 pedidos, el pago fue con tarjeta en línea y la orden no tiene ningún cupón.");
            }

            return result;
        }

        private static bool HasVisaScore(ICollection<OrderNote> orderNotes)
        {
            OrderNote orderMobileNote = orderNotes.Where(x => x.Note.Contains("Pago con Payments.Visa registrado desde la app móvil. Resultado:")).FirstOrDefault();
            if (orderMobileNote != null)
                return true;
            else
            {
                var visaNote = orderNotes.Where(x => x.Note.Contains("{\"_links\":{")).FirstOrDefault();
                if (visaNote == null) return false;
                else return true;
            }
        }

        //private static bool IsOrderNoteVisaJson(string note)
        //{
        //    if (string.IsNullOrEmpty(note))
        //        return false;
        //    return note.Contains("Pago con Payments.Visa registrado desde la app móvil. Resultado:") ||
        //        note.Contains("{\"_links\":{");
        //}

        private static int GetVisaScore(ICollection<OrderNote> orderNotes)
        {
            OrderNote orderMobileNote = orderNotes.Where(x => x.Note.Contains("Pago con Payments.Visa registrado desde la app móvil. Resultado:")).FirstOrDefault();
            dynamic result = null;
            if (orderMobileNote != null)
            {
                string visaJson = orderMobileNote.Note.Replace("Pago con Payments.Visa registrado desde la app móvil. Resultado: ", "");
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(visaJson);
            }
            else
            {
                var visaNote = orderNotes.Where(x => x.Note.Contains("{\"_links\":{")).FirstOrDefault();
                if (visaNote == null) return 0;
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(visaNote.Note);
            }
            int.TryParse(result.riskInformation.score.result.ToString(), out int scoreResult);
            return scoreResult;
        }

        public static OrderItemDto GetOrderItemDto(OrderItem orderItem,
            NotDeliveredOrderItem notDeliveredOrderItem,
            Product product,
            List<int> notBuyedReportedByBuyer,
            IPictureService _pictureService)
        {
            if (orderItem == null)
            {
                return new OrderItemDto()
                {
                    Id = 0,
                    BuyingBySecondary = notDeliveredOrderItem.BuyingBySecondary,
                    EquivalenceCoefficient = notDeliveredOrderItem.EquivalenceCoefficient,
                    ProductId = notDeliveredOrderItem.ProductId,
                    WeightInterval = notDeliveredOrderItem.WeightInterval,
                    ProductName = product?.Name,
                    RequestedQuantity = GetTotalQuantity(notDeliveredOrderItem.Quantity, notDeliveredOrderItem.EquivalenceCoefficient, notDeliveredOrderItem.BuyingBySecondary, notDeliveredOrderItem.WeightInterval),
                    RequestedUnit = GetProductRequestedUnit(notDeliveredOrderItem.EquivalenceCoefficient, notDeliveredOrderItem.BuyingBySecondary, notDeliveredOrderItem.WeightInterval),
                    SelectedPropertyOption = notDeliveredOrderItem.SelectedPropertyOption,
                    ProductPictureUrl = product == null || _pictureService == null ? null : product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().PictureId) : null,
                    Price = notDeliveredOrderItem.PriceInclTax,
                    UnitPrice = notDeliveredOrderItem.UnitPriceInclTax,
                    RawQuantity = notDeliveredOrderItem.Quantity,
                    NotDeliveredReasonId = notDeliveredOrderItem == null ? 0 : notDeliveredOrderItem.NotDeliveredReasonId,
                    NotDeliveredReason = notDeliveredOrderItem?.NotDeliveredReason,
                    NotBuyedReportedByBuyer = notDeliveredOrderItem != null && notBuyedReportedByBuyer.Any(x => x == notDeliveredOrderItem.ProductId)
                };
            }
            else
            {
                return new OrderItemDto()
                {
                    Id = orderItem.Id,
                    BuyingBySecondary = orderItem.BuyingBySecondary,
                    EquivalenceCoefficient = orderItem.EquivalenceCoefficient,
                    ProductId = orderItem.ProductId,
                    WeightInterval = orderItem.WeightInterval,
                    ProductName = orderItem.Product.Name,
                    RequestedQuantity = GetTotalQuantity(orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval),
                    RequestedUnit = GetProductRequestedUnit(orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval),
                    SelectedPropertyOption = orderItem.SelectedPropertyOption,
                    ProductPictureUrl = _pictureService != null && orderItem.Product.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(orderItem.Product.ProductPictures.FirstOrDefault().PictureId) : null,
                    Price = orderItem.PriceInclTax,
                    UnitPrice = orderItem.UnitPriceInclTax,
                    RawQuantity = orderItem.Quantity,
                    NotBuyedReportedByBuyer = notBuyedReportedByBuyer.Any(x => x == orderItem.ProductId)
                };
            }
        }

        public static OrderItem ConvertToOrderItem(NotDeliveredOrderItem ndoi,
            IOrderService _orderService,
            IProductService _productService,
            List<Product> products = null,
            List<Order> orders = null,
            bool orderRequired = true,
            bool productRequired = true)
        {
            return new OrderItem()
            {
                AttributeDescription = ndoi.AttributeDescription,
                DiscountAmountExclTax = ndoi.DiscountAmountExclTax,
                AttributesXml = ndoi.AttributesXml,
                DiscountAmountInclTax = ndoi.DiscountAmountInclTax,
                BuyingBySecondary = ndoi.BuyingBySecondary,
                EquivalenceCoefficient = ndoi.EquivalenceCoefficient,
                ItemWeight = ndoi.ItemWeight,
                OrderId = ndoi.OrderId,
                OriginalProductCost = ndoi.OriginalProductCost,
                PriceExclTax = ndoi.PriceExclTax,
                PriceInclTax = ndoi.PriceInclTax,
                ProductId = ndoi.ProductId,
                Quantity = ndoi.Quantity,
                SelectedPropertyOption = ndoi.SelectedPropertyOption,
                UnitPriceExclTax = ndoi.UnitPriceExclTax,
                UnitPriceInclTax = ndoi.UnitPriceInclTax,
                WeightInterval = ndoi.WeightInterval,
                Id = ndoi.OrderItemId,
                Order = !orderRequired ? null : orders == null ? _orderService.GetOrderById(ndoi.OrderId) : orders.Where(x => x.Id == ndoi.OrderId).FirstOrDefault(),
                Product = !productRequired ? null : products == null && productRequired ? _productService.GetProductById(ndoi.ProductId) : products.Where(x => x.Id == ndoi.ProductId).FirstOrDefault()
            };
        }

        public static string GetOrderMinimumPedidoCheckMsg(Customer customer, DateTime date,
            IOrderService _orderService, IOrderProcessingService _orderProcessingService)
        {
            var currentAddress = customer.ShippingAddress;
            var cart = customer.ShoppingCartItems.ToList();
            var dummyOrder = new Order
            {
                CustomerId = customer.Id,
                SelectedShippingDate = date,
                ShippingAddress = new Nop.Core.Domain.Common.Address
                {
                    Address1 = currentAddress?.Address1
                }
            };
            var pedidoWithSameInfo = GetOrdersInPedidoByOrder(dummyOrder, _orderService);
            if (!pedidoWithSameInfo.Any())
            {
                var minOrderSubtotalMessage = _orderProcessingService.GetMinOrderSubtotalAmountMessage(cart);
                var minOrderTotalAmountMessage = _orderProcessingService.GetMinOrderTotalAmountMessage(cart);
                if (!string.IsNullOrEmpty(minOrderSubtotalMessage) || !string.IsNullOrEmpty(minOrderTotalAmountMessage))
                    return !string.IsNullOrEmpty(minOrderSubtotalMessage) ? minOrderSubtotalMessage : minOrderTotalAmountMessage;
            }

            return string.Empty;
        }
    }

    public class ParsedShippingTimeModel
    {
        public TimeSpan InitTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class GetBuyerCashAndTransferAmountModel
    {
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
        public int MainManufacturerId { get; set; }
    }
}
