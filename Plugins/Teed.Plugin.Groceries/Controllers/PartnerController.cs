using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Extensions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderItemBuyers;
using Teed.Plugin.Groceries.Domain.MarkedBuyerItems;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;
using Teed.Plugin.Groceries.Domain.TipsWithCards;
using Teed.Plugin.Groceries.Dtos;
using Teed.Plugin.Groceries.Extensions;
using Teed.Plugin.Groceries.Models.OrderDeliveryReports;
using Teed.Plugin.Groceries.Services;
using Teed.Plugin.Groceries.Utils;
using OrderReportService = Teed.Plugin.Groceries.Services.OrderReportService;
using Teed.Plugin.Groceries.Domain.SubstituteBuyers;
using Teed.Plugin.Groceries.Domain.BuyerPayments;
using System.Net;

namespace Teed.Plugin.Groceries.Controllers
{
    public class PartnerController : ApiBaseController
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
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderItem;
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
        private readonly MarkedBuyerItemService _markedBuyerItemService;
        private readonly OrderReportTransferService _orderReportTransferService;
        private readonly SupermarketBuyerService _supermarketBuyerService;
        private readonly ProductMainManufacturerService _productMainManufacturerService;
        private readonly SubstituteBuyerService _substituteBuyerService;
        private readonly BuyerPaymentService _buyerPaymentService;
        private readonly BuyerPaymentTicketFileService _buyerPaymentTicketFileService;
        private readonly BuyerPaymentByteFileService _buyerPaymentByteFileService;

        public PartnerController(BuyerPaymentByteFileService buyerPaymentByteFileService,
            BuyerPaymentService buyerPaymentService,
            BuyerPaymentTicketFileService buyerPaymentTicketFileService,
            IOrderService orderService,
            ICustomerService customerService,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            OrderItemBuyerService orderItemBuyerService,
            OrderReportService orderReportService,
            OrderReportLogService orderReportLogService,
            OrderReportStatusService orderReportStatusService,
            OrderReportFileService orderReportFileService,
            ShippingRouteService shippingRouteService,
            SupermarketBuyerService supermarketBuyerService,
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
            IPermissionService permissionService,
            IWorkContext workContext,
            IOrderService orderItem,
            OrderReportTransferService orderReportTransferService,
            MarkedBuyerItemService markedBuyerItemService,
            ShippingAreaService shippingAreaService,
            ProductMainManufacturerService productMainManufacturerService,
            SubstituteBuyerService substituteBuyerService)
        {
            _buyerPaymentByteFileService = buyerPaymentByteFileService;
            _buyerPaymentTicketFileService = buyerPaymentTicketFileService;
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
            _markedBuyerItemService = markedBuyerItemService;
            _permissionService = permissionService;
            _workContext = workContext;
            _orderItem = orderItem;
            _orderReportTransferService = orderReportTransferService;
            _supermarketBuyerService = supermarketBuyerService;
            _productMainManufacturerService = productMainManufacturerService;
            _substituteBuyerService = substituteBuyerService;
            _buyerPaymentService = buyerPaymentService;
        }

        // DEPRECATED WILL BE REMOVED
        public IActionResult GetBuyerDates(int page, int elementsPerPage)
        {
            int customerId = int.Parse(UserId);
            List<int> orderItems = _orderItemBuyerService.GetAll().Where(x => x.CustomerId == customerId).Select(x => x.OrderItemId).ToList();

            var datesQuery = _orderService.GetAllOrdersQuery()
                .Select(x => new { x.SelectedShippingDate, ItemIds = x.OrderItems.Select(y => y.Id) })
                .ToList()
                .Where(x => x.ItemIds.Intersect(orderItems).Any())
                .Select(x => x.SelectedShippingDate)
                .GroupBy(x => x)
                .Select(x => x.FirstOrDefault())
                .OrderByDescending(x => x)
                .Select(x => new ReportDateDto()
                {
                    Date = x
                });

            var pagedList = new PagedList<ReportDateDto>(datesQuery, page, elementsPerPage, datesQuery.Count());
            foreach (var item in pagedList)
            {
                var status = _orderReportStatusService.GetAll().Where(x => x.BuyerId == customerId && x.ShippingDate == item.Date).FirstOrDefault();
                item.Date = item.Date.Value.ToUniversalTime();
                if (status == null) continue;
                item.StatusId = status.StatusTypeId;
                item.StatusValue = EnumHelper.GetDisplayName((ReportStatusType)status.StatusTypeId);
            }

            return Ok(pagedList);
        }

        public IActionResult GetAllBuyerDates()
        {
            int customerId = int.Parse(UserId);
            List<int> orderItems = _orderItemBuyerService.GetAll().Where(x => x.CustomerId == customerId).Select(x => x.OrderItemId).ToList();
            DateTime controlDate = DateTime.Now.AddDays(-7);
            //DateTime controlDate = DateTime.Now.AddDays(-20);
            DateTime today = DateTime.Now;

            var additionalDates = new List<DateTime>()
            {
                //new DateTime(2022, 1, 10),
                //new DateTime(2022, 5, 10),
                //new DateTime(2022, 6, 2),
                //new DateTime(2022, 8, 4),
                new DateTime(2022, 9, 6)
            };

            var ordersQuery = _orderService.GetAllOrdersQuery();
            if (customerId != 3557507) // Dont apply this filter if test user
                ordersQuery = ordersQuery.Where(x => (x.SelectedShippingDate > controlDate && x.SelectedShippingDate <= today) || additionalDates.Contains(x.SelectedShippingDate.Value));

            var dates = ordersQuery
                .Select(x => new { x.SelectedShippingDate, ItemIds = x.OrderItems.Select(y => y.Id) })
                .ToList()
                .Where(x => x.ItemIds.Intersect(orderItems).Any())
                .Select(x => x.SelectedShippingDate)
                .GroupBy(x => x)
                .Select(x => x.FirstOrDefault())
                .Select(x => new ReportDateDto()
                {
                    Date = x
                }).ToList();

            var substituyingUsers = _substituteBuyerService.GetAll()
                .Where(x => (x.SelectedShippingDate > controlDate || additionalDates.Contains(x.SelectedShippingDate)) && x.SubstituteCustomerId == customerId)
                .ToList();

            if (substituyingUsers.Count > 0)
            {
                foreach (var substituteBuyer in substituyingUsers)
                {
                    if (dates.Where(x => x.Date == substituteBuyer.SelectedShippingDate).Any()) continue;
                    dates.Add(new ReportDateDto()
                    {
                        Date = substituteBuyer.SelectedShippingDate
                    });
                }
            }

            var orderStatusQuery = _orderReportStatusService.GetAll().Where(x => x.BuyerId == customerId);
            if (customerId != 3557507)
                orderStatusQuery = orderStatusQuery.Where(x => (x.ShippingDate > controlDate || additionalDates.Contains(x.ShippingDate)) && x.ShippingDate <= today);

            var orderStatus = orderStatusQuery.ToList();
            foreach (var item in dates)
            {
                var status = orderStatus.Where(x => x.ShippingDate == item.Date).FirstOrDefault();
                item.Date = item.Date.Value.ToUniversalTime();
                if (status == null) continue;
                item.StatusId = status.StatusTypeId;
                item.StatusValue = EnumHelper.GetDisplayName((ReportStatusType)status.StatusTypeId);
            }

            return Ok(dates.OrderByDescending(x => x.Date));
        }

        public IActionResult GetDeliveryDates(int page, int elementsPerPage)
        {
            int customerId = int.Parse(UserId);
            var routes = _shippingRouteService.GetAll().ToList();

            var userRoutes = _shippingRouteUserService.GetAll()
                .Where(x => x.UserInChargeId == customerId)
                .OrderByDescending(x => x.ResponsableDateUtc)
                .Select(x => new DeliveryDateDto()
                {
                    Date = x.ResponsableDateUtc,
                    RouteId = x.ShippingRouteId
                });
            var pagedList = new PagedList<DeliveryDateDto>(userRoutes, page, elementsPerPage, userRoutes.Count());

            foreach (var item in pagedList)
            {
                item.Date = item.Date.Value.ToUniversalTime();
                var route = routes.Where(x => x.Id == item.RouteId).FirstOrDefault();
                if (route == null) continue;
                item.RouteName = route.RouteName;
            }

            return Ok(pagedList);
        }

        [HttpPost]
        public IActionResult ReportTipWithCard([FromBody] ReportTipWithCardDto dto)
        {
            if (dto == null) return BadRequest("Ocurrió un problema con la solicitud. Por favor avisa a un administrador");
            if (dto.ReportedAmount <= 0) return BadRequest("Debes enviar un monto válido");
            int userId = int.Parse(UserId);
            Order order = _orderService.GetOrderById(dto.OrderId);
            if (order == null) return NotFound();
            var tipWithCard = new TipsWithCard()
            {
                Amount = dto.ReportedAmount,
                OrderId = order.Id,
                ReportedByUserId = userId
            };
            _tipsWithCardService.Insert(tipWithCard);
            return Ok();
        }

        [HttpPut]
        public IActionResult MarkOrderAsDelivered(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = "-",
                TotalWeight = null,
                AdminComment = "Envío creado desde la app",
                CreatedOnUtc = DateTime.UtcNow,
            };
            foreach (var item in order.OrderItems)
            {
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = item.Id,
                    Quantity = item.Quantity,
                    WarehouseId = item.Product.WarehouseId
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }
            _shipmentService.InsertShipment(shipment);
            _orderProcessingService.Ship(shipment, false);
            _orderProcessingService.Deliver(shipment, true);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = $"El usuario {UserEmail} ({UserId}) marcó la orden como entregada desde la app.",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            _orderProcessingService.CheckOrderStatus(order);

            return NoContent();
        }

        [HttpPut]
        public IActionResult UpdateOrderItem([FromBody] UpdateOrderItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Ocurrió un problema guardando la información en el servidor.");
            OrderItem orderItem = _orderService.GetOrderItemById(dto.OrderItemId);
            if (orderItem == null) return NotFound();
            Order order = orderItem.Order;
            var notBuyedReportedByBuyer = GetNotBuyedProductIdsReportedByBuyer(order.Id);
            decimal currentQuantity = OrderUtils.GetTotalQuantity(orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
            string currentUnit = OrderUtils.GetProductRequestedUnit(orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
            decimal currentPriceInclTax = orderItem.PriceInclTax;
            decimal currentPriceExclTax = orderItem.PriceExclTax;
            decimal currentOrderTotal = order.OrderTotal;

            OrderItemDto updatedOrderItem = new OrderItemDto();

            if (dto.RawQuantity > 0)
            {
                Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
                orderItem.Quantity = dto.RawQuantity;
                var updatedPrice = OrderUtils.GetUpdatedPrice(orderItem, currentQuantity);
                orderItem.PriceInclTax = updatedPrice;
                orderItem.PriceExclTax = _taxService.GetProductPrice(orderItem.Product, orderItem.PriceInclTax, false, customer, out _);
                if (orderItem.EquivalenceCoefficient > 0 || orderItem.WeightInterval > 0)
                {
                    orderItem.UnitPriceInclTax = orderItem.PriceInclTax;
                    orderItem.UnitPriceExclTax = orderItem.PriceExclTax;
                }
                decimal priceDifferenceInclTax = orderItem.PriceInclTax - currentPriceInclTax;
                decimal priceDifferenceExclTax = orderItem.PriceExclTax - currentPriceExclTax;
                order.OrderSubtotalInclTax += priceDifferenceInclTax;
                order.OrderSubtotalExclTax += priceDifferenceExclTax;
                order.OrderTotal += priceDifferenceInclTax;
                string balanceNote = string.Empty;
                if (!(order.OrderTotal > 0))
                {
                    var difference = order.OrderTotal * -1;
                    order.OrderTotal = 0;
                    if (order.CustomerBalanceUsedAmount > 0)
                    {
                        order.CustomerBalanceUsedAmount -= difference;
                        balanceNote = $". Se regresaron {difference:C} MXN de saldo al cliente.";
                    }
                }

                decimal newQuantity = OrderUtils.GetTotalQuantity(orderItem.Quantity, orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);
                string newUnit = OrderUtils.GetProductRequestedUnit(orderItem.EquivalenceCoefficient, orderItem.BuyingBySecondary, orderItem.WeightInterval);

                OrderItemLog orderItemLog = new OrderItemLog()
                {
                    CustomerId = int.Parse(UserId),
                    NewPrice = orderItem.PriceInclTax,
                    NewQuantity = orderItem.Quantity,
                    OrderItemId = orderItem.Id,
                    OrderId = orderItem.OrderId,
                    ProductId = orderItem.ProductId,
                    OriginalPrice = currentPriceInclTax,
                    OriginalQuantity = currentQuantity,
                };
                _orderItemLogService.Insert(orderItemLog);

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"El usuario {UserEmail} ({UserId}) modificó la cantidad entregada del producto '{orderItem.Product.Name}' de {currentQuantity} {currentUnit} a {newQuantity} {newUnit}. Se ajustó el precio a pagar del producto de {currentPriceInclTax} a {orderItem.PriceInclTax} y el total de la orden de {currentOrderTotal} a {order.OrderTotal}{balanceNote}"
                });

                updatedOrderItem = OrderUtils.GetOrderItemDto(orderItem, null, null, notBuyedReportedByBuyer, _pictureService);
            }
            else
            {
                NotDeliveredOrderItem notDeliveredOrderItem = new NotDeliveredOrderItem()
                {
                    AttributeDescription = orderItem.AttributeDescription,
                    DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                    AttributesXml = orderItem.AttributesXml,
                    DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                    BuyingBySecondary = orderItem.BuyingBySecondary,
                    NotDeliveredReason = dto.NotDeliveredReason ?? EnumHelper.GetDisplayName((NotDeliveredReason)dto.NotDeliveredReasonId),
                    EquivalenceCoefficient = orderItem.EquivalenceCoefficient,
                    ItemWeight = orderItem.ItemWeight,
                    NotDeliveredReasonId = dto.NotDeliveredReasonId,
                    OrderId = orderItem.OrderId,
                    OriginalProductCost = orderItem.OriginalProductCost,
                    PriceExclTax = orderItem.PriceExclTax,
                    PriceInclTax = orderItem.PriceInclTax,
                    ProductId = orderItem.ProductId,
                    Quantity = orderItem.Quantity,
                    ReportedByUserId = int.Parse(UserId),
                    SelectedPropertyOption = orderItem.SelectedPropertyOption,
                    UnitPriceExclTax = orderItem.UnitPriceExclTax,
                    UnitPriceInclTax = orderItem.UnitPriceInclTax,
                    WeightInterval = orderItem.WeightInterval,
                    OrderItemId = orderItem.Id
                };
                _notDeliveredOrderItemService.Insert(notDeliveredOrderItem);

                order.OrderSubtotalInclTax -= orderItem.PriceInclTax;
                order.OrderSubtotalExclTax -= orderItem.PriceExclTax;
                order.OrderTotal -= orderItem.PriceInclTax;
                string balanceNote = string.Empty;
                if (!(order.OrderTotal > 0))
                {
                    var difference = order.OrderTotal * -1;
                    order.OrderTotal = 0;
                    if (order.CustomerBalanceUsedAmount > 0)
                    {
                        order.CustomerBalanceUsedAmount -= difference;
                        balanceNote = $". Se regresaron {difference:C} MXN de saldo al cliente.";
                    }
                }

                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"El usuario {UserEmail} ({UserId}) reportó el producto '{orderItem.Product.Name}' como no entregado. El total de la orden pasó de {currentOrderTotal} a {order.OrderTotal}{balanceNote}"
                });

                updatedOrderItem = OrderUtils.GetOrderItemDto(null, notDeliveredOrderItem, orderItem.Product, notBuyedReportedByBuyer, _pictureService);

                _orderService.DeleteOrderItem(orderItem);
            }
            _orderService.UpdateOrder(order);

            if (order.OrderItems.Count == 0 && order.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Paid)
            {
                _orderProcessingService.CancelOrder(order, false);
                order.OrderNotes.Add(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"La orden fue cancelada automáticamente por el sistema debido a que todos sus productos se reportaron como no entregados."
                });
            }

            return Ok(updatedOrderItem);
        }

        [HttpPut]
        public IActionResult UpdatePaymentMethod(int orderId, string paymentMethodSystemName)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            var prevPaymentMethod = order.PaymentMethodSystemName;
            order.PaymentMethodSystemName = paymentMethodSystemName;
            order.OrderNotes.Add(new OrderNote
            {
                Note = $"El usuario {UserEmail} ({UserId}) cambió el método de pago de {prevPaymentMethod} a {paymentMethodSystemName}.",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            string newOptionName = OrderUtils.GetPaymentOptionName(paymentMethodSystemName);

            return Ok(newOptionName);
        }

        [HttpGet]
        public IActionResult GetOrderTotals(string orderIds)
        {
            List<int> orderIdsArray = orderIds.Split(',').Select(x => int.Parse(x)).ToList();
            List<OrderTotalsDto> orders = _orderService.GetAllOrdersQuery().Where(x => orderIdsArray.Contains(x.Id)).Select(x => new OrderTotalsDto()
            {
                OrderId = x.Id,
                Total = x.OrderTotal
            }).ToList();
            return Ok(orders);
        }

        public IActionResult GetOrderItems(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            List<NotDeliveredOrderItem> notDeliveredOrderItems = _notDeliveredOrderItemService.GetAll().Where(x => order.Id == x.OrderId).ToList();
            List<int> notDeliveredProductIds = notDeliveredOrderItems.Select(y => y.ProductId).ToList();
            List<Product> products = _productService.GetAllProductsQuery().Where(x => notDeliveredProductIds.Contains(x.Id)).ToList();
            var notBuyedReportedByBuyer = GetNotBuyedProductIdsReportedByBuyer(orderId);

            List<OrderItemDto> orderItems = order.OrderItems.OrderBy(z => z.Product.Name)
                        .Select(z => OrderUtils.GetOrderItemDto(z, null, null, notBuyedReportedByBuyer, _pictureService))
                        .Union(notDeliveredOrderItems
                            .Where(w => w.OrderId == order.Id)
                            .Select(w => OrderUtils.GetOrderItemDto(null, w, products.Where(q => q.Id == w.ProductId).FirstOrDefault(), notBuyedReportedByBuyer, _pictureService)))
                        .ToList();

            return Ok(orderItems.OrderByDescending(x => x.NotBuyedReportedByBuyer).ThenBy(x => x.ProductName));
        }

        private List<int> GetNotBuyedProductIdsReportedByBuyer(int orderId)
        {
            return _orderReportService.GetAll()
                 .Where(x => x.OrderId == orderId && x.NotBuyedReasonId.HasValue && x.NotBuyedReasonId.Value != 0 && (!x.SentToSupermarket.HasValue || !x.SentToSupermarket.Value))
                 .Select(x => x.ProductId)
                 .ToList();
        }

        public IActionResult GetDateOrders(DateTime date)
        {
            var parsedDate = date.Date;
            int customerId = int.Parse(UserId);

            var routeId = _shippingRouteUserService.GetAll()
                .Where(x => DbFunctions.TruncateTime(x.ResponsableDateUtc) == parsedDate && x.UserInChargeId == customerId)
                .Select(x => x.ShippingRouteId)
                .FirstOrDefault();
            if (routeId == 0) return NotFound();

            var orders = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == parsedDate && (x.RouteId == routeId || x.RescuedByRouteId.Value == routeId));

            var reportedTips = _tipsWithCardService.GetAll().Where(x => x.ReportedByUserId == customerId).ToList();
            var paymentMethods = _paymentService.LoadActivePaymentMethods()
                    .Select(y => y.ToModel())
                    .Where(y => y.SystemName == "Payments.CardOnDelivery" || y.SystemName == "Payments.CashOnDelivery" || y.SystemName == "Payments.MercadoPagoQr")
                    .Select(y => new SelectListItem()
                    {
                        Text = y.FriendlyName,
                        Value = y.SystemName
                    }).ToList();

            var orderDto = orders
                .GroupBy(x => x.ShippingAddress.Address1)
                .ToList()
                .Select(x => new OrderDto()
                {
                    PaymentMethods = paymentMethods,
                    GroupedOrders = x.Where(y => y.RouteId == routeId).Select(y => PrepareOrderData(y, reportedTips)).ToList(),
                    RescuedOrders = x.Where(y => y.RescuedByRouteId.HasValue && y.RescuedByRouteId.Value == routeId).Select(y => PrepareOrderData(y, reportedTips)).ToList()
                }).ToList();

            return Ok(
                    orderDto
                    .OrderBy(x => x.RescuedOrders.Select(y => y.RescueRouteDisplayOrder).FirstOrDefault())
                    .ThenBy(x => x.GroupedOrders.Select(y => y.RouteDisplayOrder).FirstOrDefault())
                    .ToList()
                );
        }

        private OrderData PrepareOrderData(Order order, List<TipsWithCard> reportedTips)
        {
            return new OrderData()
            {
                Id = order.Id,
                CustomOrderNumber = order.CustomOrderNumber,
                OrderDiscount = order.OrderDiscount,
                OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax,
                OrderTotal = order.OrderTotal,
                OrderShippingInclTax = order.OrderShippingInclTax,
                PaymentStatusId = order.PaymentStatusId,
                ShippingStatusId = order.ShippingStatusId,
                OriginalOrderTotal = order.OriginalOrderTotal,
                RouteDisplayOrder = order.RouteDisplayOrder,
                SelectedShippingDate = order.SelectedShippingDate.Value.ToUniversalTime(),
                SelectedShippingTime = order.SelectedShippingTime,
                PaymentMethodSystemName = order.PaymentMethodSystemName,
                PaymentMethodCommonName = OrderUtils.GetPaymentOptionName(order.PaymentMethodSystemName),
                TipWithCard = reportedTips.Where(z => z.OrderId == order.Id).Select(z => z.Amount).FirstOrDefault(),
                OrderNotes = order.OrderNotes.Where(z => z.DisplayToCustomer).Select(z => z.Note).ToList(),
                RescuedByRouteId = order.RescuedByRouteId,
                RescueRouteDisplayOrder = order.RescueRouteDisplayOrder,
                ShippingAddress = new OrderAddressDto()
                {
                    Address1 = order.ShippingAddress.Address1,
                    Address2 = order.ShippingAddress.Address2,
                    FirstName = order.ShippingAddress.FirstName,
                    LastName = order.ShippingAddress.LastName,
                    Latitude = order.ShippingAddress.Latitude,
                    Longitude = order.ShippingAddress.Longitude,
                    ZipPostalCode = order.ShippingAddress.ZipPostalCode,
                    CustomAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes, htmlEncode: false)
                }
            };
        }

        public IActionResult GetManufacturerSelectList()
        {
            var manufacturers = _manufacturerService.GetAllManufacturers().OrderBy(x => x.Name);

            //var approvedReports = _orderReportStatusService.GetAll()
            //    .Where(x => x.StatusTypeId == 2)
            //    .Select(x => new OrderReportFilterModel()
            //    {
            //        BuyerId = x.BuyerId,
            //        ShippingDate = x.ShippingDate
            //    }).ToList();

            //var customManufacturers = _orderReportService.GetAll()
            //    .Where(x => x.ShoppingStoreId != "" && x.ShoppingStoreId != " " && x.ShoppingStoreId != null)
            //    .ToList()
            //    .Where(x => approvedReports.Contains(new OrderReportFilterModel()
            //    {
            //        ShippingDate = x.OrderShippingDate,
            //        BuyerId = x.ReportedByCustomerId
            //    }))
            //    .Select(x => x.ShoppingStoreId)
            //    .GroupBy(x => x)
            //    .Select(x => x.FirstOrDefault())
            //    .Select(x => new Manufacturer() { Id = -1, Name = x });

            //var allManufacturers = manufacturers.Union(customManufacturers).OrderBy(x => x.Name);

            var manufacturerSelectList = new SelectList(manufacturers, "Id", "Name");

            return Ok(manufacturerSelectList);
        }

        public IActionResult GetPaymentRequestManufacturerSelectList()
        {
            var manufacturers = _manufacturerService.GetAllManufacturers().Where(x => x.IsPaymentWhithTransfer && !x.Deleted).OrderBy(x => x.Name);
            var manufacturerSelectList = new SelectList(manufacturers, "Id", "Name");
            return Ok(manufacturerSelectList);
        }

        [HttpPost]
        public IActionResult CreatePaymentRequest([FromBody] CreatePaymentRequestDto dto)
        {
            int customerId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null) return BadRequest();

            DateTime dateParsed = DateTime.ParseExact(dto.ShippingDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var paymentRequest = new BuyerPayment()
            {
                RequestedAmount = dto.Amount,
                BuyerId = customerId,
                ManufacturerId = dto.ManufacturerId,
                ShippingDate = dateParsed,
                PaymentStatusId = (int)BuyerPaymentStatus.Pending,
                Log = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + customer.GetFullName() + $" ({customerId}) creó una solicitud de pago.\n"
            };
            _buyerPaymentService.Insert(paymentRequest);

            try
            {
                foreach (var file in dto.Files)
                {
                    byte[] pictureFileBinary = Convert.FromBase64String(file.Base64);
                    var picture = _pictureService.InsertPicture(pictureFileBinary, file.MimeType, "");
                    _buyerPaymentTicketFileService.Insert(new BuyerPaymentTicketFile()
                    {
                        FileId = picture.Id,
                        BuyerPaymentId = paymentRequest.Id,
                        CreatedByCustomerId = customerId,
                    });
                }
            }
            catch (Exception e)
            {
                paymentRequest.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"Se eliminó la solicitud ya que hubo un error cargando los tickets. ERROR: {e.Message}\n";
                _buyerPaymentService.Delete(paymentRequest);
                return BadRequest(e.Message);
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetPaymentRequest(string date)
        {
            int customerId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null) return BadRequest();
            DateTime dateParsed = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var buyerPayment = _buyerPaymentService.GetAll().Where(x => x.BuyerId == customerId && x.ShippingDate == dateParsed).ToList();
            var manufacturers = _manufacturerService.GetAllManufacturers();
            var buyerPaymentIds = buyerPayment.Select(x => x.Id).ToList();
            var fileIds = _buyerPaymentTicketFileService.GetAll()
                .Where(x => buyerPaymentIds.Contains(x.BuyerPaymentId))
                .Select(x => new { x.FileId, x.BuyerPaymentId })
                .ToList();

            var dto = buyerPayment
                .Select(x => new BuyerPaymentDto()
                {
                    RequestedAmount = x.RequestedAmount,
                    BuyerId = x.BuyerId,
                    Id = x.Id,
                    ManufacturerId = x.ManufacturerId,
                    ManufacturerName = manufacturers.Where(y => y.Id == x.ManufacturerId).Select(y => y.Name).FirstOrDefault(),
                    PaymentFileId = x.PaymentFileId,
                    PaymentStatus = ((BuyerPaymentStatus)x.PaymentStatusId).GetDisplayName(),
                    PaymentStatusId = x.PaymentStatusId,
                    ShippingDate = x.ShippingDate,
                    FileIds = fileIds.Where(y => y.BuyerPaymentId == x.Id).Select(y => y.FileId).ToList()
                }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public virtual IActionResult TicketPicture(int id)
        {
            int customerId = int.Parse(UserId);
            var buyerPayment = _buyerPaymentTicketFileService.GetAll().Where(x => x.FileId == id && x.CreatedByCustomerId == customerId).Any();
            if (!buyerPayment) return Unauthorized();
            var picture = _pictureService.GetPictureById(id);
            byte[] bytes = null;
            string mimeType = null;

            if (picture == null)
            {
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    mimeType = "image/png";
                }
            }
            else
            {
                bytes = picture.PictureBinary;
                mimeType = picture.MimeType;
            }

            return File(bytes, mimeType);
        }

        // DEPRECATED 11-01-2022
        [HttpGet]
        public virtual IActionResult GetFile(int id)
        {
            int customerId = int.Parse(UserId);
            var file = _buyerPaymentByteFileService.GetById(id);
            if (file == null) return NotFound();

            var buyerPayment = _buyerPaymentService.GetAll()
                .Where(x => x.PaymentFileId == id || x.InvoiceFilePdfId == id || x.InvoiceFileXmlId == id)
                .Where(x => x.BuyerId == customerId)
                .FirstOrDefault();
            if (buyerPayment == null) return NotFound();
            var manufacturer = _manufacturerService.GetAllManufacturers().Where(x => x.Id == buyerPayment.ManufacturerId).FirstOrDefault();
            var buyer = _customerService.GetCustomerById(buyerPayment.BuyerId);

            return File(file.FileBytes, file.MimeType,
                $"{(buyerPayment.PaymentFileId == id ? "Comprobante de pago" : buyerPayment.InvoiceFilePdfId == id ? "Factura PDF" : buyerPayment.InvoiceFileXmlId == id ? "Factura XML" : "")}"
                + $" - {manufacturer?.Name ?? "NA"} - {(buyer != null ? buyer.GetFullName() : "NA")} - {buyerPayment.ShippingDate:dd-MM-yyyy}.{file.Extension}");
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult GetOpenFile(int id, int buyerId)
        {
            var file = _buyerPaymentByteFileService.GetById(id);
            if (file == null) return NotFound();

            var buyerPayment = _buyerPaymentService.GetAll()
                .Where(x => x.PaymentFileId == id || x.InvoiceFilePdfId == id || x.InvoiceFileXmlId == id)
                .Where(x => x.BuyerId == buyerId)
                .FirstOrDefault();
            if (buyerPayment == null) return NotFound();
            var manufacturer = _manufacturerService.GetAllManufacturers().Where(x => x.Id == buyerPayment.ManufacturerId).FirstOrDefault();
            var buyer = _customerService.GetCustomerById(buyerPayment.BuyerId);

            return File(file.FileBytes, file.MimeType,
                $"{(buyerPayment.PaymentFileId == id ? "Comprobante de pago" : buyerPayment.InvoiceFilePdfId == id ? "Factura PDF" : buyerPayment.InvoiceFileXmlId == id ? "Factura XML" : "")}"
                + $" - {manufacturer?.Name ?? "NA"} - {(buyer != null ? buyer.GetFullName() : "NA")} - {buyerPayment.ShippingDate:dd-MM-yyyy}.{file.Extension}");
        }

        [HttpDelete]
        public virtual IActionResult DeletePaymentRequest(int id)
        {
            int customerId = int.Parse(UserId);
            var paymentRequest = _buyerPaymentService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (paymentRequest == null) return NotFound();
            if (paymentRequest.BuyerId != customerId) return Unauthorized();

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null) return NotFound();

            paymentRequest.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + customer.GetFullName() + $" ({customerId}) eliminó la solicitud de pago.\n";
            _buyerPaymentService.Delete(paymentRequest);

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetSupermarketProducts(string date)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            int userId = int.Parse(UserId);
            bool userShouldBuySupermarket = _supermarketBuyerService.GetAll().Where(x => DbFunctions.TruncateTime(x.ShippingDate) == parsedDate && x.BuyerId == userId).Any();
            if (!userShouldBuySupermarket) return NoContent();

            var reports = _orderReportService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate && (x.SentToSupermarketByUserId.HasValue && x.SentToSupermarketByUserId.Value > 0)).ToList();
            var productIds = reports.Select(x => x.ProductId).ToList();

            var orders = OrderUtils.GetFilteredOrders(_orderService).Where(x => x.SelectedShippingDate.Value == parsedDate).ToList();
            var orderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            var filteredItems = orderItems.Where(x => productIds.Contains(x.ProductId));
            var groupedItems = filteredItems.GroupBy(x => x.Product).ToList();

            var sentToSupermarketCustomerIds = reports.Select(x => x.SentToSupermarketByUserId).ToList();
            var supermarketCustomers = _customerService.GetAllCustomersQuery().Where(x => sentToSupermarketCustomerIds.Contains(x.Id)).ToList();

            var dto = groupedItems.Select(x => ConvertToReportProductDto(x, parsedDate, reports, supermarketCustomers));

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetAllowedQuantities(int orderItemId)
        {
            var data = OrderUtils.GetAllowedQuantities(orderItemId, _orderService);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public IActionResult GetReportFiles([FromBody] string[] orderItemIds)
        {
            if (orderItemIds == null) return BadRequest("Ocurrió un problema descargando los datos. Por favor, notifica a un administrador");
            int userId = int.Parse(UserId);
            var reportFiles = _orderReportFileService.GetAll().Where(x => x.UploadedByUserId == userId).ToList().Where(x => orderItemIds.Any(x.OrderItemIds.Contains));
            var result = reportFiles.Select(x => new ReportFileDto()
            {
                FileName = x.FileName,
                FileType = x.FileType,
                ServerUrl = (_storeContext.CurrentStore.SslEnabled ? _storeContext.CurrentStore.SecureUrl : _storeContext.CurrentStore.Url) + x.FileUrl,
                ProductIds = x.ProductIds
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public IActionResult DeliverReport([FromBody] DeliverReportDto dto)
        {
            int customerId = int.Parse(UserId);
            DateTime parsedDate = dto.Date.ToLocalTime().Date;
            var reportStatusQuery = _orderReportStatusService
                .GetAll()
                .Where(x => DbFunctions.TruncateTime(x.ShippingDate) == parsedDate);
            var buyerReportStatus = reportStatusQuery.Where(x => x.BuyerId == customerId).FirstOrDefault();
            if (buyerReportStatus == null)
            {
                var substituyedUserIds = _substituteBuyerService.GetAll()
                    .Where(x => x.SubstituteCustomerId == customerId && x.SelectedShippingDate == parsedDate)
                    .Select(x => x.CustomerId)
                    .ToList();
                substituyedUserIds.Add(customerId);
                foreach (var userId in substituyedUserIds)
                {
                    if (reportStatusQuery.Where(x => x.BuyerId == userId).Any()) continue;
                    buyerReportStatus = new OrderReportStatus()
                    {
                        BuyerId = userId,
                        ShippingDate = parsedDate,
                        StatusTypeId = (int)ReportStatusType.Pending
                    };
                    _orderReportStatusService.Insert(buyerReportStatus);
                }

            }
            return NoContent();
        }

        public IActionResult GetNotDeliveredReasons()
        {
            var reasons = (Enum.GetValues(typeof(NotDeliveredReason)))
                .Cast<NotDeliveredReason>()
                .Select(x => new
                {
                    Text = EnumHelper.GetDisplayName(x),
                    Value = (int)x
                });

            return Ok(reasons);
        }

        public IActionResult GetNotBuyedReasons()
        {
            var reasons = (Enum.GetValues(typeof(NotBuyedReason)))
                .Cast<NotBuyedReason>()
                .Select(x => new
                {
                    Text = EnumHelper.GetDisplayName(x),
                    Value = (int)x
                });

            return Ok(reasons);
        }

        // DEPRECATED 11-02-2022
        public IActionResult GetReportProducts2(DateTime date)
        {
            var parsedDate = date.Date;
            int userId = int.Parse(UserId);

            bool notContains = _orderItemBuyerService.GetAll().Where(x => x.SelectedShippingDate == parsedDate && x.CustomerId == 0).Any();
            bool buyerDownload = _buyerListDownloadService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate).Select(x => x.AllowDownload).FirstOrDefault();

            if (notContains || !buyerDownload) return BadRequest("La descarga de datos aun no está disponible. Por favor, inténtalo de nuevo más tarde.");

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == parsedDate);

            List<int> buyerCustomerIds = new List<int>();
            var substituyingUsers = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.SubstituteCustomerId == userId)
                .ToList();
            var substituyingUserIds = substituyingUsers.Select(x => x.CustomerId).ToList();
            buyerCustomerIds.AddRange(substituyingUserIds);

            var isBeingSustituted = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.CustomerId == userId)
                .Any();
            if (!isBeingSustituted)
            {
                buyerCustomerIds.Add(userId);
            }

            var orderItemBuyer = _orderItemBuyerService.GetAll()
                .Where(x => buyerCustomerIds.Contains(x.CustomerId) && x.SelectedShippingDate == parsedDate)
                .Select(x => new { x.OrderId, x.OrderItemId, x.CustomerId, x.SelectedShippingDate })
                .ToList();

            var substituyingUserModel = substituyingUsers.Select(x => new SubstituyingUserModel()
            {
                OriginalCustomerId = x.CustomerId,
                SubstituteCustomerId = x.SubstituteCustomerId,
                OrderItemIds = orderItemBuyer.Where(y => y.CustomerId == x.CustomerId && y.SelectedShippingDate == x.SelectedShippingDate).Select(y => y.OrderItemId).ToList()
            }).ToList();


            var buyerOrderIds = orderItemBuyer.Select(x => x.OrderId).ToList();
            var orders = ordersQuery.Where(x => buyerOrderIds.Contains(x.Id)).ToList();

            var markedProducts = _markedBuyerItemService.GetAll().Where(x => x.OrderShippingDate == date).ToList();
            List<OrderItem> orderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            List<int> orderItemIdsOfTheDay = orderItems.Select(x => x.Id).ToList();

            List<int> orderItemIds = orderItemBuyer.Select(x => x.OrderItemId).ToList();
            var reports = _orderReportService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();
            orderItems = orderItems.Where(x => orderItemIds.Contains(x.Id)).ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var manufacturerGroup = orderItems
                    .GroupBy(x => ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                    .OrderBy(x => x.Key?.Name)
                    .ToList();

            var sentToSupermarketCustomerIds = reports.Where(x => x.SentToSupermarketByUserId.HasValue).Select(x => x.SentToSupermarketByUserId.Value).ToList();
            substituyingUserIds.AddRange(sentToSupermarketCustomerIds);
            var supermarketAndSubstituteCustomers = _customerService.GetAllCustomersQuery().Where(x => substituyingUserIds.Contains(x.Id)).ToList();

            List<List<IGrouping<Product, OrderItem>>> productGroup = new List<List<IGrouping<Product, OrderItem>>>();
            foreach (var manufacturer in manufacturerGroup)
            {
                productGroup.Add(manufacturer
                        .Select(x => x)
                        .GroupBy(x => x.Product)
                        .OrderBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.DisplayOrder)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId != 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key.Name)
                        .ToList());
            }

            List<ReportProductDto> dto = productGroup.SelectMany(x => x).Select(x => ConvertToReportProductDto(x, date, reports, supermarketAndSubstituteCustomers, substituyingUserModel, markedProducts)).ToList();

            return Ok(dto);
        }

        public IActionResult GetReportProducts3(DateTime date)
        {
            var parsedDate = date.Date;
            int userId = int.Parse(UserId);

            var orderIetmsNotContains = _orderItemBuyerService.GetAll().Where(x => !x.Deleted && x.SelectedShippingDate == parsedDate && x.CustomerId == 0);
            bool notContains = orderIetmsNotContains.Any();
            bool buyerDownload = _buyerListDownloadService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate).Select(x => x.AllowDownload).FirstOrDefault();
            bool buyerReport = _buyerListDownloadService.GetAll().Where(x => DbFunctions.TruncateTime(x.OrderShippingDate) == parsedDate).Select(x => x.AllowReport).FirstOrDefault() ?? false;
            if (!buyerDownload)
                return BadRequest("La descarga de datos aun no está disponible. Por favor, inténtalo de nuevo más tarde.");
            else if (notContains)
            {
                var orderIds = orderIetmsNotContains.Select(x => x.OrderId).Distinct().ToList();
                bool anyOrderNotCancelled = OrderUtils.GetFilteredOrders(_orderService)
                    .Where(x => orderIds.Contains(x.Id) && x.OrderStatusId != 40).Any();
                if (anyOrderNotCancelled)
                    return BadRequest("La descarga de datos aun no está disponible. Por favor, inténtalo de nuevo más tarde.");
            }

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
                .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == parsedDate);

            List<int> buyerCustomerIds = new List<int>();
            var substituyingUsers = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.SubstituteCustomerId == userId)
                .ToList();
            var substituyingUserIds = substituyingUsers.Select(x => x.CustomerId).ToList();
            buyerCustomerIds.AddRange(substituyingUserIds);

            var isBeingSustituted = _substituteBuyerService.GetAll()
                .Where(x => x.SelectedShippingDate == parsedDate && x.CustomerId == userId)
                .Any();
            if (!isBeingSustituted)
            {
                buyerCustomerIds.Add(userId);
            }

            var orderItemBuyer = _orderItemBuyerService.GetAll()
                .Where(x => buyerCustomerIds.Contains(x.CustomerId) && x.SelectedShippingDate == parsedDate)
                .Select(x => new { x.OrderId, x.OrderItemId, x.CustomerId, x.SelectedShippingDate })
                .ToList();

            var substituyingUserModel = substituyingUsers.Select(x => new SubstituyingUserModel()
            {
                OriginalCustomerId = x.CustomerId,
                SubstituteCustomerId = x.SubstituteCustomerId,
                OrderItemIds = orderItemBuyer.Where(y => y.CustomerId == x.CustomerId && y.SelectedShippingDate == x.SelectedShippingDate).Select(y => y.OrderItemId).ToList()
            }).ToList();


            var buyerOrderIds = orderItemBuyer.Select(x => x.OrderId).ToList();
            var orders = ordersQuery.Where(x => buyerOrderIds.Contains(x.Id)).ToList();

            var markedProducts = _markedBuyerItemService.GetAll().Where(x => x.OrderShippingDate == date).ToList();
            List<OrderItem> orderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            List<int> orderItemIdsOfTheDay = orderItems.Select(x => x.Id).ToList();

            List<int> orderItemIds = orderItemBuyer.Select(x => x.OrderItemId).ToList();
            var reports = _orderReportService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();
            orderItems = orderItems.Where(x => orderItemIds.Contains(x.Id)).ToList();

            var mainManufacturers = _productMainManufacturerService.GetAll().ToList();
            var manufacturerGroup = orderItems
                    .GroupBy(x => ProductUtils.GetMainManufacturer(x.Product.ProductManufacturers, mainManufacturers.Where(y => y.ProductId == x.ProductId).FirstOrDefault()))
                    .OrderBy(x => x.Key?.Name)
                    .ToList();

            var sentToSupermarketCustomerIds = reports.Where(x => x.SentToSupermarketByUserId.HasValue).Select(x => x.SentToSupermarketByUserId.Value).ToList();
            substituyingUserIds.AddRange(sentToSupermarketCustomerIds);
            var supermarketAndSubstituteCustomers = _customerService.GetAllCustomersQuery().Where(x => substituyingUserIds.Contains(x.Id)).ToList();

            List<List<IGrouping<Product, OrderItem>>> productGroup = new List<List<IGrouping<Product, OrderItem>>>();
            foreach (var manufacturer in manufacturerGroup)
            {
                productGroup.Add(manufacturer
                        .Select(x => x)
                        .GroupBy(x => x.Product)
                        .OrderBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.DisplayOrder)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key?.ProductCategories.Where(y => y.Category.ParentCategoryId != 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault()?.Name)
                        .ThenBy(x => x.Key.Name)
                        .ToList());
            }

            List<ReportProductDto> dto = productGroup.SelectMany(x => x).Select(x => ConvertToReportProductDto(x, date, reports, supermarketAndSubstituteCustomers, substituyingUserModel, markedProducts)).ToList();

            return Ok(new { CanReport = buyerReport, Products = dto });
        }

        private ReportProductDto ConvertToReportProductDto(IGrouping<Product, OrderItem> group,
            DateTime shippingDate,
            List<OrderReport> reports,
            List<Customer> supermarketAndSubstituteCustomers,
            List<SubstituyingUserModel> substituteBuyers = null,
            List<MarkedBuyerItem> markedProducts = null)
        {
            var mainManufacturer = _productMainManufacturerService.GetAll().Where(x => x.ProductId == group.Key.Id).FirstOrDefault();
            var mainManufaturer = ProductUtils.GetMainManufacturer(group.Key.ProductManufacturers, mainManufacturer);
            var originalBuyerId = substituteBuyers == null ? 0 : substituteBuyers.Where(x => x.OrderItemIds.Intersect(group.Select(y => y.Id)).Any()).Select(x => x.OriginalCustomerId).FirstOrDefault();

            return new ReportProductDto()
            {
                Date = shippingDate.ToUniversalTime(),
                ProductName = group.Key.Name,
                ProductPictureUrl = group.Key.ProductPictures.Count > 0 ? _pictureService.GetPictureUrl(group.Key.ProductPictures.FirstOrDefault().PictureId) : null,
                ProductId = group.Key.Id,
                RequestedQuantity = GetTotalQuantity(group.Key, group.Select(y => y.Quantity).DefaultIfEmpty().Sum()),
                RequestedUnit = (group.Key.EquivalenceCoefficient > 0 || group.Key.WeightInterval > 0) ? "kg" : "pz",
                RequestedQtyCost = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.RequestedQtyCost).FirstOrDefault(), //se está guardando la misma para todos
                ManufacturerId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.ManufacturerId ?? 0).FirstOrDefault(),
                BuyedQuantity = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.Quantity).FirstOrDefault(),//se está guardando la misma para todos
                CustomManufacturer = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.ShoppingStoreId).FirstOrDefault(),
                ControlPrice = group.Select(y => y.UnitPriceInclTax).DefaultIfEmpty().Average() * (1 - (group.Key.ProductCategories.Where(z => z.Category.ParentCategoryId != 0).Select(z => z.Category.PercentageOfUtility).FirstOrDefault() / 100)),
                UnitPrice = group.Select(y => y.UnitPriceInclTax).DefaultIfEmpty().Average(),
                UnitCost = reports.Where(y => y.ProductId == group.Key.Id).Select(z => z.UnitCost).FirstOrDefault(),
                SellPrice = group.Select(y => y.PriceInclTax).DefaultIfEmpty().Max(),
                NotBuyedReason = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.NotBuyedReason).FirstOrDefault(),
                NotBuyedReasonId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.NotBuyedReasonId).FirstOrDefault(),
                OrderItemIds = string.Join(",", group.Select(y => y.Id)),
                SentToSupermarket = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarket).FirstOrDefault(),
                Invoice = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.Invoice).FirstOrDefault(),
                IsMarked = markedProducts != null && markedProducts.Where(y => y.ProductId == group.Key.Id).Select(y => y.IsMarked).FirstOrDefault(),
                MainManufacturerId = mainManufaturer == null ? 0 : mainManufaturer.Id,
                MainManufacturerName = mainManufaturer?.Name,
                OriginalBuyerName = substituteBuyers == null ? null : supermarketAndSubstituteCustomers.Where(y => y.Id == originalBuyerId).FirstOrDefault()?.GetFullName(),
                SentToSupermarketByUserId = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault(),
                SentToSupermarketByUserFullName = reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault().HasValue &&
                reports.Where(y => y.ProductId == group.Key.Id).Select(y => y.SentToSupermarketByUserId).FirstOrDefault().Value > 0 ?
                    supermarketAndSubstituteCustomers.Where(y => y.Id == reports.Where(z => z.ProductId == group.Key.Id).Select(z => z.SentToSupermarketByUserId).FirstOrDefault().Value).FirstOrDefault()?.GetFullName()
                    : null
            };
        }

        private decimal GetTotalQuantity(Product product, decimal quantity)
        {
            decimal result = quantity;
            if (product.EquivalenceCoefficient > 0)
                result = ((quantity * 1000) / product.EquivalenceCoefficient) / 1000;
            else if (product.WeightInterval > 0)
                result = (quantity * product.WeightInterval) / 1000;
            return Math.Round(result, 2);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SaveReportFiles()
        {
            int userId = int.Parse(UserId);
            string productIds = null;
            string orderItemIds = null;
            try
            {
                productIds = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString())["productIds"];
            }
            catch (Exception e) { }

            try
            {
                orderItemIds = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString())["orderItemIds"];
            }
            catch (Exception e) { }

            var files = Request.Form.Files;
            if (files.Count == 0) return BadRequest("No se recibió ningún archivo.");

            var file = files.FirstOrDefault();
            if (file.Length == 0) return BadRequest("El archivo recibido es inválido.");

            string baseDirectoryName = "orders-report-media";
            string directoryPath = $"./wwwroot/{baseDirectoryName}/buyer-{userId}";
            Directory.CreateDirectory(directoryPath);
            string newFileName = $"{file.Name}";
            string fullPath = $"{directoryPath}/{newFileName}";

            int maxSize = 1500;
            Image image = Image.FromStream(file.OpenReadStream(), true, true);

            int width = image.Width;
            int heigth = image.Height;

            if (image.Width > maxSize)
            {
                width = maxSize;
                heigth = Convert.ToInt32(image.Height * maxSize / (double)image.Width);
            }
            else if (image.Height > maxSize)
            {
                width = Convert.ToInt32(image.Width * maxSize / (double)image.Height);
                heigth = maxSize;
            }
            var resized = new Bitmap(width, heigth);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(image, 0, 0, width, heigth);
                using (MemoryStream ms = new MemoryStream())
                {
                    var qualityParamId = System.Drawing.Imaging.Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityParamId, 75);
                    resized.Save(fullPath, ImageFormat.Jpeg);
                }
            }

            OrderReportFile orderReportFile = new OrderReportFile()
            {
                FileName = file.Name,
                FileType = file.ContentType,
                OrderId = 0,
                Size = (int)file.Length,
                UploadedByUserId = userId,
                FileUrl = $"/{baseDirectoryName}/buyer-{userId}/{newFileName}",
                Extension = Path.GetExtension(newFileName),
                ProductIds = productIds,
                OrderItemIds = orderItemIds
            };
            _orderReportFileService.Insert(orderReportFile);

            return NoContent();
        }

        [HttpPut]
        public IActionResult UpdateInvoice([FromBody] UpdateInvoiceDto dto)
        {
            int customerId = int.Parse(UserId);
            var parsedDate = DateTime.ParseExact(dto.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;
            var orderReport = _orderReportService.GetAll().Where(x => x.ProductId == dto.ProductId && x.OrderShippingDate == parsedDate).ToList();
            if (orderReport.Count == 0) return NoContent();
            foreach (var report in orderReport)
            {
                report.Invoice = dto.Invoice;
                _orderReportService.Update(report);

                OrderReportLog orderReportLog = new OrderReportLog()
                {
                    OrderId = report.OrderId,
                    Log = "Actualizó el folio.",
                    AuthorId = customerId
                };
                _orderReportLogService.Insert(orderReportLog);
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult SaveOrderReport([FromBody] List<SaveOrderBodyDto> dto)
        {
            var parsedDate = DateTime.ParseExact(dto.FirstOrDefault().Date, "dd-MM-yyyy", CultureInfo.InvariantCulture).Date;

            var ordersQuery = OrderUtils.GetFilteredOrders(_orderService)
            .Where(x => x.SelectedShippingDate.HasValue && DbFunctions.TruncateTime(x.SelectedShippingDate.Value) == parsedDate);

            if (dto.Count == 1)
            {
                var productId = dto.FirstOrDefault().ProductId;
                var notDeliveredItemsOrderIds = _notDeliveredOrderItemService.GetAll().Where(x => x.ProductId == productId).Select(x => x.OrderId).ToList();
                ordersQuery = ordersQuery.Where(x => x.OrderItems.Select(y => y.ProductId).Contains(productId) || notDeliveredItemsOrderIds.Contains(x.Id));
            }

            var orders = ordersQuery.ToList();

            int customerId = int.Parse(UserId);
            var orderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);
            var orderItemIds = orderItems.Select(x => x.Id).ToList();

            var orderItemBuyer = _orderItemBuyerService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();
            var orderReports = _orderReportService.GetAll().Where(x => orderItemIds.Contains(x.OrderItemId)).ToList();

            foreach (var product in dto)
            {
                if ((!product.RequestedQtyCost.HasValue || !product.UnitCost.HasValue || !product.BuyedQuantity.HasValue)
                    && (!product.NotBuyedReasonId.HasValue || product.NotBuyedReasonId == 0)) return BadRequest("Debes completar todos los campos para poder guardar.");
                var logText = string.Empty;

                var filteredItems = orderItems.Where(x => x.ProductId == product.ProductId).ToList();

                int totalItems = filteredItems.Count;
                int itemTotalQty = filteredItems.Select(x => x.Quantity).Sum();
                foreach (var item in filteredItems)
                {
                    int assignedToBuyerId = orderItemBuyer.Where(x => x.OrderItemId == item.Id).Select(x => x.CustomerId).FirstOrDefault();
                    var orderReport = orderReports.Where(x => x.OrderItemId == item.Id).FirstOrDefault();

                    if (orderReport == null)
                    {
                        orderReport = new OrderReport()
                        {
                            OrderItemId = item.Id,
                            ManufacturerId = product.ManufacturerId,
                            OrderId = item.OrderId,
                            RequestedQtyCost = product.RequestedQtyCost ?? 0, //Guardamos el mismo para todos porque sino al volver a descargar los datos se muestra mal en el formulario
                            UpdatedRequestedQtyCost = product.RequestedQtyCost ?? 0, //Guardamos el mismo para todos porque sino al volver a descargar los datos se muestra mal en el formulario
                            UnitCost = product.UnitCost ?? 0,
                            UpdatedUnitCost = product.UnitCost ?? 0,
                            Quantity = product.BuyedQuantity ?? 0,
                            UpdatedQuantity = product.BuyedQuantity ?? 0,
                            ShoppingStoreId = product.CustomManufacturer,
                            NotBuyedReason = !product.NotBuyedReasonId.HasValue ? null : product.NotBuyedReasonId == -1 ? product.NotBuyedReason : product.NotBuyedReasonId > 0 ? EnumHelper.GetDisplayName((NotBuyedReason)product.NotBuyedReasonId) : null,
                            NotBuyedReasonId = product.NotBuyedReasonId,
                            ProductId = product.ProductId,
                            ReportedByCustomerId = customerId,
                            OriginalBuyerId = assignedToBuyerId,
                            ReportedDateUtc = DateTime.UtcNow,
                            Invoice = product.Invoice,
                            OrderShippingDate = item.Order.SelectedShippingDate.Value,
                            SentToSupermarket = product.SentToSupermarket,
                            SentToSupermarketByUserId = product.SentToSupermarket.HasValue && product.SentToSupermarket.Value ? customerId : 0
                        };

                        logText = "Creó un reporte.";
                        _orderReportService.Insert(orderReport);
                    }
                    else
                    {
                        orderReport.OrderItemId = item.Id;
                        orderReport.ManufacturerId = product.ManufacturerId;
                        orderReport.OrderId = item.OrderId;
                        orderReport.RequestedQtyCost = product.RequestedQtyCost ?? 0; //Guardamos el mismo para todos porque sino al volver a descargar los datos se muestra mal en el formulario
                        orderReport.UpdatedRequestedQtyCost = product.RequestedQtyCost ?? 0; //Guardamos el mismo para todos porque sino al volver a descargar los datos se muestra mal en el formulario
                        orderReport.UnitCost = product.UnitCost ?? 0;
                        orderReport.UpdatedUnitCost = product.UnitCost ?? 0;
                        orderReport.Quantity = product.BuyedQuantity ?? 0; //Colocamos lo que se compró, aunque sea lo mismo para todos, porque luego al descargar la información, la que mostrará en el formualrio no será la real
                        orderReport.UpdatedQuantity = product.BuyedQuantity ?? 0; //Colocamos lo que se compró, aunque sea lo mismo para todos, porque luego al descargar la información, la que mostrará en el formualrio no será la real
                        orderReport.ShoppingStoreId = product.CustomManufacturer;
                        orderReport.NotBuyedReason = !product.NotBuyedReasonId.HasValue ? null : product.NotBuyedReasonId == -1 ? product.NotBuyedReason : product.NotBuyedReasonId > 0 ? EnumHelper.GetDisplayName((NotBuyedReason)product.NotBuyedReasonId) : null;
                        orderReport.NotBuyedReasonId = product.NotBuyedReasonId;
                        orderReport.ProductId = product.ProductId;
                        orderReport.ReportedByCustomerId = customerId;
                        orderReport.ReportedDateUtc = DateTime.UtcNow;
                        orderReport.Invoice = product.Invoice;
                        orderReport.OrderShippingDate = item.Order.SelectedShippingDate.Value;

                        if (orderReport.OriginalBuyerId == orderReport.ReportedByCustomerId)
                        {
                            orderReport.SentToSupermarket = orderReport.SentToSupermarketByUserId.HasValue && orderReport.SentToSupermarketByUserId > 0 ? true : product.SentToSupermarket;
                            if (orderReport.SentToSupermarket.HasValue)
                            {
                                orderReport.SentToSupermarketByUserId = product.SentToSupermarket.Value ? customerId : 0;
                            }
                        }

                        logText = "Actualizó un reporte.";
                        _orderReportService.Update(orderReport);
                    }

                    int.TryParse(UserId, out int userId);
                    OrderReportLog orderReportLog = new OrderReportLog()
                    {
                        OrderId = item.OrderId,
                        Log = logText,
                        AuthorId = userId
                    };
                    _orderReportLogService.Insert(orderReportLog);
                }
            }
            return NoContent();
        }


        [HttpPost]
        public IActionResult AddMarkedProducts([FromBody] SaveMarkedProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Ocurrió un problema guardando la información en el servidor.");

            var parsedDate = DateTime.ParseExact(dto.OrderShippingDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var data = new MarkedBuyerItem()
            {
                OrderShippingDate = parsedDate,
                BuyerId = dto.BuyerId,
                IsMarked = dto.IsMarked,
                ProductId = dto.ProductId
            };

            _markedBuyerItemService.Insert(data);

            return NoContent();
        }

        public IActionResult RemoveMarkedProducts([FromBody] SaveMarkedProductDto dto)
        {
            var parsedDate = DateTime.ParseExact(dto.OrderShippingDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            MarkedBuyerItem markedProduct = _markedBuyerItemService.GetAll()
                .Where(x => x.ProductId == dto.ProductId)
                .Where(y => y.OrderShippingDate == parsedDate)
                .FirstOrDefault();

            if (markedProduct == null) return NotFound();
            _markedBuyerItemService.Delete(markedProduct);

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetMainManufacturerId(int productId)
        {
            Product product = _productService.GetProductById(productId);
            if (product == null) return NotFound();
            var manufacturer = product.ProductManufacturers
                .Where(z => !z.Manufacturer.Deleted)
                .OrderByDescending(z => z.Manufacturer.IsPaymentWhithTransfer)
                .ThenByDescending(z => z.Manufacturer.IsIncludeInReportByManufacturer)
                .ThenByDescending(z => z.Id)
                .ThenBy(z => z.Manufacturer.DisplayOrder)
                .Select(z => z.Manufacturer)
                .FirstOrDefault();
            if (manufacturer == null) return NotFound();
            return Ok(manufacturer.Id);
        }

        private decimal GetTotalQuantity(int productId, List<int> orderItemsIds)
        {
            var product = _productService.GetProductById(productId);

            decimal quantity = 0;
            foreach (var item in orderItemsIds)
            {
                var orderItem = _orderItem.GetOrderItemById(item);
                var orderItemQuuantity = 0;
                if (orderItem != null)
                {
                    orderItemQuuantity = orderItem.Quantity;
                }
                quantity = orderItemQuuantity + quantity;
            }

            decimal result = quantity;
            if (product.EquivalenceCoefficient > 0)
                result = ((quantity * 1000) / product.EquivalenceCoefficient) / 1000;
            else if (product.WeightInterval > 0)
                result = (quantity * product.WeightInterval) / 1000;
            return Math.Round(result, 2);
        }

        private string GetUnitProduct(int productId)
        {
            var product = _productService.GetProductById(productId);
            string unit = (product.EquivalenceCoefficient > 0 || product.WeightInterval > 0) ? "kg" : "pz";
            return unit;
        }

        private OrderReportFile GetReportFileByProduct(int productId, List<OrderReportFile> orderReportFiles)
        {
            List<string> productsIds = new List<string>();
            var productsIdsInOrderReportFiles = orderReportFiles.Where(x => x.ProductIds.Contains(productId.ToString())).Select(x => x.ProductIds).ToList();
            foreach (var item in productsIdsInOrderReportFiles)
            {
                var arrayInItem = item.Split(',');
                foreach (var wordInArray in arrayInItem)
                {
                    if (productId == int.Parse(wordInArray))
                    {
                        productsIds.Add(item);
                    }
                }
            }
            var findOrderReportFilesWhitProduct = orderReportFiles.Where(x => productsIds.Contains(x.ProductIds));
            var file = findOrderReportFilesWhitProduct.FirstOrDefault();
            return file;
        }

        public List<SelectedList> GetProductManufacturers(int productId, string shoppingStoreId)
        {
            var manufacturers = _manufacturerService.GetProductManufacturersByProductId(productId);
            var productManufacturers = new List<SelectedList>();
            if (manufacturers != null)
            {
                productManufacturers = manufacturers.Select(x => new SelectedList()
                {
                    Id = x.ManufacturerId,
                    Name = x.Manufacturer.Name,
                    IsSelected = x.Manufacturer.Name == shoppingStoreId ? true : false
                }).ToList();
            }

            return productManufacturers;
        }

        private RoundedAmount RoundBuyerCashAmount(decimal costSum)
        {
            if (costSum == 0) return new RoundedAmount() { Amount = 0, AmountString = 0.ToString("C") };
            costSum += 50;
            costSum = Math.Ceiling(costSum / 100) * 100;
            return new RoundedAmount() { Amount = costSum, AmountString = costSum.ToString("C") };
        }

        [HttpGet]
        public IActionResult OrderReportDetails(int buyerId, string date)
        {
            if (date == null) return AccessDeniedView();
            var dateSelected = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            OrderReportDetailsViewModel model = new OrderReportDetailsViewModel();

            var orderStatus = _orderReportStatusService.GetAll().Where(x => x.BuyerId == buyerId && x.ShippingDate == dateSelected).OrderBy(x => x.CreatedOnUtc).FirstOrDefault();
            if (orderStatus == null) return NotFound();
            var orderReports = _orderReportService.GetAll()
                .Where(x => x.ReportedByCustomerId == buyerId && x.OrderShippingDate == dateSelected && x.Deleted == false)
                .GroupBy(x => x.ProductId)
                .ToList();

            var manufacturers = _manufacturerService.GetAllManufacturers().OrderBy(x => x.Name).ToList();
            var productMainManufacturer = _productMainManufacturerService.GetAll().ToList();

            var orders = _orderService.GetOrders()
             .Where(x => x.SelectedShippingDate.HasValue && x.SelectedShippingDate.Value == dateSelected)
             .Where(x => x.OrderStatus != OrderStatus.Cancelled &&
             !(x.PaymentMethodSystemName == "Payments.PayPalStandard" && x.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Pending))
             .ToList();

            List<OrderItem> parsedOrderItems = orders.GetGroceryOrderItems(_notDeliveredOrderItemService, _orderService, _productService);

            List<int> orderItemIdsOfTheDay = parsedOrderItems.Select(x => x.Id).ToList();
            var orderItemBuyerQuery = _orderItemBuyerService.GetAll().Where(x => orderItemIdsOfTheDay.Contains(x.OrderItemId));
            orderItemBuyerQuery = orderItemBuyerQuery.Where(x => x.CustomerId == buyerId);
            var orderItemBuyer = orderItemBuyerQuery.ToList();
            var orderItemIdsFilter = orderItemBuyer.Where(x => x.CustomerId == buyerId).Select(x => x.OrderItemId);
            var filteredOrderItems = parsedOrderItems.Where(x => orderItemIdsFilter.Contains(x.Id)).ToList();

            var groupedByProduct = filteredOrderItems.GroupBy(x => x.Product.ProductCategories.Where(y => y.Category.ParentCategoryId == 0 && !y.Category.Deleted).Select(y => y.Category).FirstOrDefault())
                    .OrderBy(x => x.Key?.DisplayOrder)
                    .ToList();

            model.BuyerId = buyerId;
            model.CurrentStatus = (ReportStatusType)orderStatus.StatusTypeId;
            model.CurrentUserName = _workContext.CurrentCustomer.GetFullName();
            model.BuyerName = _customerService.GetCustomerById(buyerId).GetFullName();
            model.OrderShippigDate = dateSelected;
            model.DayOfWeek = dateSelected.ToString("dddd", new CultureInfo("es-MX"));

            model.Products = new List<ProductData>();

            foreach (var item in groupedByProduct)
            {
                var tester = item
                       .Select(x => x)
                       .GroupBy(x => x.Product)
                       .OrderBy(x => x.Key.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).OrderBy(y => y.Manufacturer.DisplayOrder).FirstOrDefault()?.Manufacturer.Name)
                       .ThenBy(x => x.Key.Name)
                       .ThenBy(x => x.Key.ProductManufacturers.Where(y => !y.Manufacturer.Deleted).OrderBy(y => y.Manufacturer.DisplayOrder).FirstOrDefault()?.DisplayOrder)
                       .ToList();

                var productsInGroup = tester.Select(x =>
                {
                    var productReports = orderReports.Where(y => y.Key == x.Key.Id).SelectMany(y => y).ToList();
                    var updatedQuantity = productReports.Select(y => y.UpdatedQuantity).FirstOrDefault();
                    var quantity = productReports.Select(y => y.Quantity).FirstOrDefault();
                    var updatedRequestedQuantityCost = productReports.Select(y => y.UpdatedRequestedQtyCost).FirstOrDefault();
                    var requestedQtyCost = productReports.Select(y => y.RequestedQtyCost).FirstOrDefault();
                    var updatedUnitCost = productReports.Select(z => z.UpdatedUnitCost).FirstOrDefault();
                    var unitCost = productReports.Select(z => z.UnitCost).FirstOrDefault();

                    return new ProductData()
                    {
                        ProductId = x.Key.Id,
                        ProductName = _productService.GetProductById(x.Key.Id).Name,

                        ProductCostKgPz = Math.Round(updatedUnitCost, 2),

                        ProductCostKgPzOriginal = productReports.Select(y => y.UnitCost).FirstOrDefault(),

                        ProductQuantity = updatedQuantity.HasValue ? Math.Round(updatedQuantity.Value, 2) : quantity.Value,

                        ProductQuantityOriginal = quantity.Value,

                        ProductAmountTotal = updatedRequestedQuantityCost.HasValue ? Math.Round(updatedRequestedQuantityCost.Value, 2) : requestedQtyCost,

                        ProductAmountTotalOriginal = productReports.Select(z => z.RequestedQtyCost).FirstOrDefault(),

                        NoBuyedReazon = productReports.Select(z => z.NotBuyedReason).FirstOrDefault(),

                        RequestedQuantity = GetTotalQuantity(x.Key.Id, x.Where(y => y.ProductId == x.Key.Id).Select(y => y.Id).ToList()),

                        RequestedUnit = GetUnitProduct(x.Key.Id),
                        NumberOrders = x.Select(y => y.OrderId).Count(),

                        SentToSuperMarket = productReports.Select(y => y.SentToSupermarket).FirstOrDefault() ?? false,

                        IsProductUpdated = (updatedUnitCost != unitCost) ||
                                         (updatedQuantity.HasValue && updatedQuantity.Value != quantity.Value) ||
                                         (updatedRequestedQuantityCost.HasValue && updatedRequestedQuantityCost.Value != requestedQtyCost)
                    };
                }).ToList();

                model.Products.AddRange(productsInGroup);
            }

            model.TotalProductsUpdated = model.Products.Where(x => x.IsProductUpdated).Count();

            var buyerMoney = OrderUtils.GetBuyerCashAndTransferAmount(parsedOrderItems,
                buyerId,
                orderItemBuyer,
                manufacturers,
                productMainManufacturer);

            var moneyToBuyer = RoundBuyerCashAmount(buyerMoney.Cash);
            model.BuyerCashAmountString = moneyToBuyer.AmountString;
            model.BuyerCashAmount = moneyToBuyer.Amount;

            model.BuyerCardAmount = buyerMoney.Card;
            model.BuyerCardAmountString = buyerMoney.Card.ToString("C");

            var orderReportTransfer = _orderReportTransferService.GetAll().Where(x => x.BuyerId == buyerId && x.OrderShippingDate == dateSelected).FirstOrDefault();
            if (orderReportTransfer == null)
            {
                orderReportTransfer = new OrderReportTransfer();
            }
            model.OrderReportTransferId = orderReportTransfer.Id;
            model.AmountTotalTransfer = orderReportTransfer.TransferAmount;
            model.AmountTotalSpent = model.Products.Sum(x => x.ProductAmountTotal);
            model.AmountTotalInCash = model.AmountTotalSpent - model.AmountTotalTransfer - model.BuyerCardAmount;
            model.AmountTotalReturned = model.BuyerCashAmount - model.AmountTotalInCash;

            return Ok(model);
        }
    }

    public class SubstituyingUserModel
    {
        public int OriginalCustomerId { get; set; }
        public List<int> OrderItemIds { get; set; }
        public int SubstituteCustomerId { get; set; }

    }
}
