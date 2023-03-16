using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teed.Plugin.Groceries.Domain.ShippingAreas;
using Teed.Plugin.Groceries.Domain.ShippingRegions;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.Groceries.Helpers;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Settings;
using Teed.Plugin.Groceries.Utils;
using OrderReportService = Teed.Plugin.Groceries.Services.OrderReportService;

namespace Teed.Plugin.Groceries.Controllers
{
    public class AppController : ApiBaseController
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreContext _storeContext;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IPaymentService _paymentService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ITaxService _taxService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly OrderItemBuyerService _orderItemBuyerService;
        private readonly BuyerListDownloadService _buyerListDownloadService;
        private readonly OrderReportService _orderReportService;
        private readonly OrderReportLogService _orderReportLogService;
        private readonly OrderReportStatusService _orderReportStatusService;
        private readonly OrderReportFileService _orderReportFileService;
        private readonly ShippingRouteUserService _shippingRouteUserService;
        private readonly ShippingRouteService _shippingRouteService;
        private readonly OrderItemLogService _orderItemLogService;
        private readonly NotDeliveredOrderItemService _notDeliveredOrderItemService;
        private readonly TipsWithCardService _tipsWithCardService;
        private readonly ShippingAreaService _shippingAreaService;
        private readonly PostalCodeSearchService _postalCodeSearchService;
        private readonly ShippingRegionService _shippingRegionService;
        private readonly ShippingRegionZoneService _shippingRegionZoneService;
        private readonly PostalCodeNotificationRequestService _postalCodeNotificationRequestService;

        public AppController(IOrderService orderService,
            ICustomerService customerService,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            OrderItemBuyerService orderItemBuyerService,
            OrderReportService orderReportService,
            OrderReportLogService orderReportLogService,
            OrderReportStatusService orderReportStatusService,
            OrderReportFileService orderReportFileService,
            ShippingRouteService shippingRouteService,
            IStoreContext storeContext,
            ShippingRouteUserService shippingRouteUserService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IPaymentService paymentService,
            IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService,
            OrderItemLogService orderItemLogService,
            NotDeliveredOrderItemService notDeliveredOrderItemService,
            ITaxService taxService,
            IProductService productService,
            TipsWithCardService tipsWithCardService,
            BuyerListDownloadService buyerListDownloadService,
            ShippingAreaService shippingAreaService,
            PostalCodeSearchService postalCodeSearchService,
            ShippingRegionService shippingRegionService,
            ShippingRegionZoneService shippingRegionZoneService,
            ISettingService settingService,
            PostalCodeNotificationRequestService postalCodeNotificationRequestService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _orderItemBuyerService = orderItemBuyerService;
            _orderReportService = orderReportService;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
            _orderReportLogService = orderReportLogService;
            _orderReportStatusService = orderReportStatusService;
            _orderReportFileService = orderReportFileService;
            _storeContext = storeContext;
            _shippingRouteUserService = shippingRouteUserService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _shippingRouteService = shippingRouteService;
            _paymentService = paymentService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _orderItemLogService = orderItemLogService;
            _notDeliveredOrderItemService = notDeliveredOrderItemService;
            _taxService = taxService;
            _productService = productService;
            _tipsWithCardService = tipsWithCardService;
            _buyerListDownloadService = buyerListDownloadService;
            _shippingAreaService = shippingAreaService;
            _postalCodeSearchService = postalCodeSearchService;
            _shippingRegionService = shippingRegionService;
            _shippingRegionZoneService = shippingRegionZoneService;
            _settingService = settingService;
            _postalCodeNotificationRequestService = postalCodeNotificationRequestService;
        }

        [HttpGet]
        public IActionResult GetNotDeliveredProducts(int orderId)
        {
            int userId = int.Parse(UserId);
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();
            if (order.CustomerId != userId) return Unauthorized();

            var notDeliveredProducts = _notDeliveredOrderItemService.GetAll().Where(x => x.OrderId == orderId).ToList();
            var dto = new List<ProductDto>();
            foreach (var item in notDeliveredProducts)
            {
                var product = _productService.GetProductById(item.ProductId);
                dto.Add(new ProductDto()
                {
                    Id = item.ProductId,
                    BuyingBySecondary = item.BuyingBySecondary,
                    EquivalenceCoefficient = item.EquivalenceCoefficient,
                    PictureUrl = "/Product/ProductImage?id=" + item.ProductId,
                    Name = product.Name,
                    CurrentCartQuantity = item.Quantity,
                    SelectedPropertyOption = item.SelectedPropertyOption,
                    Sku = product.Sku,
                    SubTotal = item.PriceInclTax,
                    UnitPrice = item.UnitPriceInclTax,
                    WeightInterval = ProductHelper.GetWeightInterval(product)
                });
            }
            return Ok(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyPostalCodeRegion(string postalCode)
        {
            //var isValid = _shippingRegionZoneService.GetAll().Where(x => x.Zone.PostalCodes.Contains(postalCode) || x.Zone.AdditionalPostalCodes.Contains(postalCode)).Any();
            var postalCodes = _shippingAreaService.GetAll().Select(x => x.PostalCode.Trim()).ToList();
            var isValid = postalCodes.Contains(postalCode);
            return Ok(isValid);
        }

        [HttpGet]
        public IActionResult GetAvailableDaysByCp(string postalCode)
        {
            int userId = int.Parse(UserId);

            var regionZonesQuery = _shippingRegionZoneService.GetAll()
                .Where(x => x.Zone.PostalCodes.Contains(postalCode) || x.Zone.AdditionalPostalCodes.Contains(postalCode));
            if (!regionZonesQuery.Any()) return NotFound();

            ShippingRegion region = regionZonesQuery.FirstOrDefault().Region;
            List<string> postalCodeList = _shippingRegionZoneService.GetAll()
                .Where(x => x.RegionId == region.Id)
                .Select(x => x.Zone.PostalCodes + "," + x.Zone.AdditionalPostalCodes)
                .ToList();
            string[] postalCodes = string.Join(",", postalCodeList).Split(',').Select(x => x.Trim()).ToArray();

            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
            DateTime nextWeek = DateTime.Now.AddDays(7).Date;

            var allOrdersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue &&
                x.SelectedShippingDate.Value >= tomorrow &&
                x.SelectedShippingDate.Value <= nextWeek);

            allOrdersQuery = OrderUtils.GetPedidosOnly(allOrdersQuery);
            var allOrdersList = allOrdersQuery.Select(x => new { x.CustomerId, x.SelectedShippingDate, x.SelectedShippingTime }).ToList();
            var regionOrdersList = allOrdersQuery.Where(x => postalCodes.Contains(x.ShippingAddress.ZipPostalCode))
                .Select(x => new { x.CustomerId, x.SelectedShippingDate, x.SelectedShippingTime })
                .ToList();
            var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();

            List<ShippingTimeModel> times = new List<ShippingTimeModel>()
            {
                new ShippingTimeModel() { Time = "1:00 PM - 3:00 PM", Quota = region.Schedule1Quantity },
                new ShippingTimeModel() { Time = "3:00 PM - 5:00 PM", Quota = region.Schedule2Quantity },
                new ShippingTimeModel() { Time = "5:00 PM - 7:00 PM", Quota = region.Schedule3Quantity },
                new ShippingTimeModel() { Time = "7:00 PM - 9:00 PM", Quota = region.Schedule4Quantity },
            };

            List<DateTime> holydays = new List<DateTime>()
            {
                new DateTime(2021, 12, 24),
                new DateTime(2021, 12, 31),
            };

            List<ShippingDateDto> dto = new List<ShippingDateDto>();
            for (DateTime i = tomorrow; i <= nextWeek; i = i.AddDays(1))
            {
                foreach (var time in times)
                {
                    var regionOrders = regionOrdersList.Where(x => x.SelectedShippingTime == time.Time && x.SelectedShippingDate == i);
                    var allOrders = allOrdersList.Where(x => x.SelectedShippingTime == time.Time && x.SelectedShippingDate == i);
                    var shouldBlockTime = allOrdersList.Where(x => x.CustomerId == userId && x.SelectedShippingTime != time.Time && x.SelectedShippingDate == i).Any();

                    dto.Add(new ShippingDateDto()
                    {
                        Date = i,
                        Disabled = i.DayOfWeek == DayOfWeek.Sunday || OrderUtils.DisabledDates.Contains(i) || (holydays.Contains(i) && (time.Time == "7:00 PM - 9:00 PM" || time.Time == "5:00 PM - 7:00 PM")) || shouldBlockTime,
                        ShippingTime = time.Time,
                        IsActive = allOrders.Where(x => x.CustomerId == userId).Any() || (regionOrders.Count() < time.Quota && allOrders.Count() < GetGlobalQuota(time.Time, globalSchedule))
                    });
                }
            }

            return Ok(dto);
        }

        // NOT CONSIDERING REGION
        //[HttpGet]
        //public IActionResult GetAvailableDaysByCp(string postalCode)
        //{
        //    int userId = int.Parse(UserId);

        //    var postalCodes = _shippingAreaService.GetAll().Select(x => x.PostalCode.Trim()).ToList();

        //    if (!postalCodes.Contains(postalCode)) return NotFound();

        //    DateTime tomorrow = DateTime.Now.AddDays(1).Date;
        //    DateTime nextWeek = DateTime.Now.AddDays(7).Date;

        //    var allOrdersQuery = OrderUtils.GetFilteredOrders(_orderService)
        //        .Where(x => x.SelectedShippingDate.HasValue &&
        //        x.SelectedShippingDate.Value >= tomorrow &&
        //        x.SelectedShippingDate.Value <= nextWeek);

        //    allOrdersQuery = OrderUtils.GetPedidosOnly(allOrdersQuery);
        //    var allOrdersList = allOrdersQuery.Select(x => new { x.CustomerId, x.SelectedShippingDate, x.SelectedShippingTime }).ToList();

        //    var globalSchedule = _settingService.LoadSetting<ScheduleSettings>();

        //    List<ShippingTimeModel> times = new List<ShippingTimeModel>()
        //    {
        //        new ShippingTimeModel() { Time = "1:00 PM - 3:00 PM", Quota = globalSchedule.Schedule1Quantity },
        //        new ShippingTimeModel() { Time = "3:00 PM - 5:00 PM", Quota = globalSchedule.Schedule2Quantity },
        //        new ShippingTimeModel() { Time = "5:00 PM - 7:00 PM", Quota = globalSchedule.Schedule3Quantity },
        //        new ShippingTimeModel() { Time = "7:00 PM - 9:00 PM", Quota = globalSchedule.Schedule4Quantity },
        //    };

        //    List<DateTime> holydays = new List<DateTime>()
        //    {
        //        new DateTime(2020, 12, 24),
        //        new DateTime(2020, 12, 31),
        //    };

        //    List<ShippingDateDto> dto = new List<ShippingDateDto>();
        //    for (DateTime i = tomorrow; i <= nextWeek; i = i.AddDays(1))
        //    {
        //        foreach (var time in times)
        //        {
        //            var allOrders = allOrdersList.Where(x => x.SelectedShippingTime == time.Time && x.SelectedShippingDate == i);
        //            var shouldBlockTime = allOrdersList.Where(x => x.CustomerId == userId && x.SelectedShippingTime != time.Time && x.SelectedShippingDate == i).Any();

        //            dto.Add(new ShippingDateDto()
        //            {
        //                Date = i,
        //                Disabled = i.DayOfWeek == DayOfWeek.Sunday || OrderUtils.DisabledDates.Contains(i) || (holydays.Contains(i) && time.Time == "7:00 PM - 9:00 PM") || shouldBlockTime,
        //                ShippingTime = time.Time,
        //                IsActive = allOrders.Where(x => x.CustomerId == userId).Any() || (allOrders.Count() < time.Quota)
        //            });
        //        }
        //    }

        //    return Ok(dto);
        //}

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RegisterInvalidPostalCode(string postalCode)
        {
            PostalCodeSearch postalCodeSearch = new PostalCodeSearch()
            {
                PostalCode = postalCode
            };
            _postalCodeSearchService.Insert(postalCodeSearch);

            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidatePostalCode(string postalCode)
        {
            var postalCodes = _shippingAreaService.GetAll().Select(x => x.PostalCode.Trim()).ToList();
            var isValid = postalCodes.Contains(postalCode);

            if (!isValid)
            {
                PostalCodeSearch postalCodeSearch = new PostalCodeSearch()
                {
                    PostalCode = postalCode
                };
                _postalCodeSearchService.Insert(postalCodeSearch);
            }

            return Ok(isValid);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PostalCodeNotificationRequest([FromBody] PostalCodeNotificationRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("No fue posible guardar la información. Por favor contáctanos para ayudarte.");

            _postalCodeNotificationRequestService.Insert(new PostalCodeNotificationRequest()
            {
                PostalCode = dto.PostalCode,
                Email = dto.Email
            });

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetMinOrderAmountErrorMessage()
        {
            int userId = int.Parse(UserId);
            Customer currentCustomer = _customerService.GetCustomerById(userId);
            if (!currentCustomer.Active) return Unauthorized();

            if (_orderProcessingService.ShouldCheckForOrderMinimumValidation(currentCustomer))
            {
                Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
                var cart = customer.ShoppingCartItems.ToList();
                var orderSubtotalErrorMessage = _orderProcessingService.GetMinOrderSubtotalAmountMessage(cart);
                var orderTotalErrorMessage = _orderProcessingService.GetMinOrderTotalAmountMessage(cart);

                return Ok(!string.IsNullOrEmpty(orderSubtotalErrorMessage) ? orderSubtotalErrorMessage :
                    !string.IsNullOrEmpty(orderTotalErrorMessage) ? orderTotalErrorMessage :
                    string.Empty);
            }
            return Ok(string.Empty);
        }

        [HttpGet]
        public IActionResult GetOrderMinimumPedidoCheck(string date)
        {
            int userId = int.Parse(UserId);
            Customer currentCustomer = _customerService.GetCustomerById(userId);
            if (!currentCustomer.Active) return Unauthorized();
            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;

            return Ok(OrderUtils.GetOrderMinimumPedidoCheckMsg(currentCustomer, parsedDate, _orderService, _orderProcessingService));
        }

        #region DEPRECATED

        // 02-2021
        [HttpGet]
        public IActionResult GetAvailableDays()
        {
            List<ShippingTimeModel> times = new List<ShippingTimeModel>()
            {
                new ShippingTimeModel() { Time = "1:00 PM - 3:00 PM", Quota = 14 },
                new ShippingTimeModel() { Time = "3:00 PM - 5:00 PM", Quota = 52 },
                new ShippingTimeModel() { Time = "5:00 PM - 7:00 PM", Quota = 57 },
                new ShippingTimeModel() { Time = "7:00 PM - 9:00 PM", Quota = 37 },
            };

            List<DateTime> disabledDates = new List<DateTime>()
            {
                new DateTime(2020, 11, 16),
                new DateTime(2020, 12, 25),
                new DateTime(2021, 1, 1),
                new DateTime(2021, 2, 1),
                new DateTime(2021, 3, 15),
                new DateTime(2021, 5, 1),
                new DateTime(2021, 9, 16),
                new DateTime(2021, 11, 15),
                new DateTime(2021, 12, 25),
            };

            List<DateTime> holydays = new List<DateTime>()
            {
                new DateTime(2020, 12, 24),
                new DateTime(2020, 12, 31),
            };

            int userId = int.Parse(UserId);
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
            DateTime nextWeek = DateTime.Now.AddDays(7).Date;
            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && x.SelectedShippingDate.Value >= tomorrow && x.SelectedShippingDate.Value <= nextWeek);

            List<ShippingDateDto> dto = new List<ShippingDateDto>();
            for (DateTime i = tomorrow; i <= nextWeek; i = i.AddDays(1))
            {
                foreach (var time in times)
                {
                    var filteredOrders = orders.Where(x => x.SelectedShippingTime == time.Time && x.SelectedShippingDate == i);
                    dto.Add(new ShippingDateDto()
                    {
                        Date = i,
                        Disabled = i.DayOfWeek == DayOfWeek.Sunday || disabledDates.Contains(i) || (holydays.Contains(i) && time.Time == "7:00 PM - 9:00 PM"),
                        ShippingTime = time.Time,
                        IsActive = filteredOrders.Where(x => x.CustomerId == userId).Any() || filteredOrders.Count() < time.Quota
                    });
                }
            }

            return Ok(dto);
        }

        //02-2021
        [HttpGet]
        public IActionResult VerifyPostalCode(string postalCode)
        {
            List<string> validPostalCodes = _shippingAreaService.GetAll().Select(x => x.PostalCode).ToList();
            return Ok(validPostalCodes.Contains(postalCode));
        }

        private int GetGlobalQuota(string time, ScheduleSettings settings)
        {
            switch (time)
            {
                case "1:00 PM - 3:00 PM":
                    return settings.Schedule1Quantity;
                case "3:00 PM - 5:00 PM":
                    return settings.Schedule2Quantity;
                case "5:00 PM - 7:00 PM":
                    return settings.Schedule3Quantity;
                case "7:00 PM - 9:00 PM":
                    return settings.Schedule4Quantity;
                default:
                    return 0;
            }
        }

        #endregion
    }

    public class ShippingTimeModel
    {
        public string Time { get; set; }
        public int Quota { get; set; }
    }
}
