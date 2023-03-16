using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Dtos.Order;
using Teed.Plugin.Api.Dtos.Products;
using Teed.Plugin.Api.Helper;

namespace Teed.Plugin.Api.Controllers
{
    public class OrderController : ApiBaseController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IAddressService _addressService;
        private readonly ICurrencyService _currencyService;
        private readonly ITaxService _taxService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IDiscountService _discountService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IStockLogService _stockLogService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ProductAttributeConverter _productAttributeConverter;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public OrderController(IOrderService orderService,
            ICustomNumberFormatter customNumberFormatter,
            IWebHelper webHelper,
            IAddressService addressService,
            ICurrencyService currencyService,
            IStoreContext storeContext,
            ITaxService taxService,
            IProductService productService,
            IDiscountService discountService,
            IProductAttributeParser productAttributeParser,
            ICustomerService customerService,
            IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService,
            IProductAttributeFormatter productAttributeFormatter,
            ICustomerActivityService customerActivityService,
            IWorkflowMessageService workflowMessageService,
            IEventPublisher eventPublisher,
            IOrderProcessingService orderProcessingService,
            IPictureService pictureService,
            IWorkContext workContext,
            IStockLogService stockLogService,
            ProductAttributeConverter productAttributeConverter,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            _orderService = orderService;
            _customNumberFormatter = customNumberFormatter;
            _webHelper = webHelper;
            _addressService = addressService;
            _currencyService = currencyService;
            _storeContext = storeContext;
            _productAttributeConverter = productAttributeConverter;
            _taxService = taxService;
            _productService = productService;
            _customerService = customerService;
            _priceCalculationService = priceCalculationService;
            _localizationService = localizationService;
            _productAttributeFormatter = productAttributeFormatter;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _pictureService = pictureService;
            _orderSettings = orderSettings;
            _workContext = workContext;
            _productAttributeParser = productAttributeParser;
            _discountService = discountService;
            _stockLogService = stockLogService;
            _orderProcessingService = orderProcessingService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetCustomerOrders(int page, int elementsPerPage)
        {
            var userId = int.Parse(UserId);
            var dto = _orderService.SearchOrders(customerId: userId, pageIndex: page, pageSize: elementsPerPage).Select(x => new GetCustomerOrdersDto()
            {
                Id = x.Id,
                OrderNumber = x.CustomOrderNumber,
                CreationDate = x.CreatedOnUtc,
                OrderTotal = x.OrderTotal,
                OrderSubtotal = x.OrderSubtotalInclTax,
                OrderShipping = x.OrderShippingInclTax,
                OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                IsCancelled = x.OrderStatus == OrderStatus.Cancelled,
                PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                SelectedShippingDate = x.SelectedShippingDate.HasValue ? (DateTime?)x.SelectedShippingDate.Value.ToUniversalTime() : null,
                SelectedShippingTime = x.SelectedShippingTime,
                ShippingAddress = new UserAddressDto()
                {
                    Address1 = x.ShippingAddress.Address1
                },
                OrderItems = x.OrderItems.Select(y => new ProductDto()
                {
                    Id = y.ProductId,
                    BuyingBySecondary = y.BuyingBySecondary,
                    EquivalenceCoefficient = y.Product.EquivalenceCoefficient,
                    PictureUrl = "/Product/ProductImage?id=" + y.ProductId,
                    Name = y.Product.Name,
                    CurrentCartQuantity = y.Quantity,
                    SelectedPropertyOption = y.SelectedPropertyOption,
                    Sku = y.Product.Sku,
                    SubTotal = y.PriceInclTax,
                    UnitPrice = y.UnitPriceInclTax,
                    WeightInterval = ProductHelper.GetWeightInterval(y.Product),
                    IsInWishlist = x.Customer.ShoppingCartItems.Where(z => z.ProductId == y.Id && z.ShoppingCartType == ShoppingCartType.Wishlist).Any(),
                    Stock = y.Product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock ? 1000 : y.Product.StockQuantity
                }).ToList()
            }).ToList();

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult ReOrder(int orderId)
        {
            int userId = int.Parse(UserId);
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();
            if (order.CustomerId != userId) return Unauthorized();

            _orderProcessingService.ReOrder(order);

            return NoContent();
        }

        //[HttpGet]
        //public IActionResult ValidateCouponAndGetDiscount(string discountcouponcode, string customerId)
        //{
        //    if (discountcouponcode != null)
        //        discountcouponcode = discountcouponcode.Trim();

        //    Customer customer = _customerService.GetCustomerById(int.Parse(customerId));
        //    if (customer == null) return NotFound();

        //    if (!string.IsNullOrWhiteSpace(discountcouponcode))
        //    {
        //        //we find even hidden records here. this way we can display a user-friendly message if it's expired
        //        var discounts = _discountService.GetAllDiscountsForCaching(couponCode: discountcouponcode, showHidden: true)
        //            .Where(d => d.RequiresCouponCode)
        //            .ToList();
        //        if (discounts.Any())
        //        {
        //            var userErrors = new List<string>();
        //            var anyValidDiscount = discounts.Any(discount =>
        //            {
        //                var validationResult = _discountService.ValidateDiscount(discount, customer, new[] { discountcouponcode });
        //                userErrors.AddRange(validationResult.Errors);
        //                return validationResult.IsValid;
        //            });

        //            if (anyValidDiscount)
        //            {
        //                var discount = _discountService.GetAllDiscountsForCaching(couponCode: discountcouponcode).FirstOrDefault();
        //                if (discount != null && _discountService.ValidateDiscount(discount, customer).IsValid)
        //                {

        //                }
        //            }
        //            else
        //            {
        //                return BadRequest(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
        //            }
        //        }
        //        else
        //            return BadRequest(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
        //    }

        //    return BadRequest(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));

        //}

        //[HttpGet]
        //public IActionResult GetOrder(int orderId)
        //{
        //    Order order = _orderService.GetOrderById(orderId);
        //    if (order == null) return NotFound();

        //    var dto = new GetOrderDto()
        //    {
        //        Id = order.Id,
        //        OrderNumber = order.CustomOrderNumber,
        //        OrderTime = order.CreatedOnUtc,
        //        OrderTotal = order.OrderTotal,
        //        OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
        //        PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
        //        ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
        //        ShippingAddress = new ShippingAddress()
        //        {
        //            Address1 = order.ShippingAddress.Address1,
        //            Address2 = order.ShippingAddress.Address2,
        //            City = order.ShippingAddress.City,
        //            Country = order.ShippingAddress.Country.Name,
        //            CountryId = order.ShippingAddress.CountryId.Value,
        //            Email = order.ShippingAddress.Email,
        //            FirstName = order.ShippingAddress.FirstName,
        //            Id = order.Id,
        //            LastName = order.ShippingAddress.LastName,
        //            PhoneNumber = order.ShippingAddress.PhoneNumber,
        //            StateProvince = order.ShippingAddress.StateProvince.Name,
        //            StateProvinceId = order.ShippingAddress.StateProvinceId.Value,
        //            ZipPostalCode = order.ShippingAddress.ZipPostalCode
        //        },
        //        OrderItems = order.OrderItems.Select(y => new ProductDto()
        //        {
        //            Id = y.ProductId,
        //            Name = y.Product.Name,
        //            Pictures = y.Product.ProductPictures.Count == 0 ? new List<string>() { _pictureService.GetDefaultPictureUrl() } : y.Product.ProductPictures.Select(z => _pictureService.GetPictureUrl(z.Id)).ToList(),
        //            Quantity = y.Quantity,
        //            SelectedAttributes = _productAttributeParser.ParseProductAttributeMappings(y.AttributesXml).Select(a => new Dtos.Products.SelectedAttribute()
        //            {
        //                AttributeName = a.ProductAttribute.Name,
        //                AttributeId = a.ProductAttribute.Id,
        //                AttributeValueId = _productAttributeParser.ParseProductAttributeValues(y.AttributesXml, a.Id).FirstOrDefault().Id,
        //                AttributeValueName = _productAttributeParser.ParseProductAttributeValues(y.AttributesXml, a.Id).FirstOrDefault().Name,
        //            }).ToList(),
        //            Price = y.Product.Price
        //        }).ToList()
        //    };

        //    return Ok(dto);
        //}

        //[HttpPost]
        //public virtual IActionResult AddNewOrder([FromBody] AddNewOrderDto dto)
        //{
        //    Customer customer = _customerService.GetCustomerById(int.Parse(UserId));
        //    if (customer == null) return NotFound();
        //    int tax = 16;
        //    var orderSubtotalExclTax = dto.OrderSubtotal / (((decimal)tax / 100) + 1);
        //    var currencyId = customer.GetAttribute<string>(SystemCustomerAttributeNames.CurrencyId);

        //    var order = new Order
        //    {
        //        StoreId = _storeContext.CurrentStore.Id,
        //        OrderGuid = Guid.NewGuid(),
        //        CustomerId = customer.Id,
        //        CustomerLanguageId = _storeContext.CurrentStore.DefaultLanguageId,
        //        CustomerTaxDisplayType = TaxDisplayType.IncludingTax,
        //        CustomerIp = _webHelper.GetCurrentIpAddress(),
        //        OrderSubtotalInclTax = dto.OrderSubtotal,
        //        OrderSubtotalExclTax = orderSubtotalExclTax,
        //        OrderSubTotalDiscountInclTax = 0,
        //        OrderSubTotalDiscountExclTax = 0,
        //        OrderShippingInclTax = dto.OrderShipping,
        //        OrderShippingExclTax = dto.OrderShipping,
        //        PaymentMethodAdditionalFeeInclTax = decimal.Zero,
        //        PaymentMethodAdditionalFeeExclTax = decimal.Zero,
        //        TaxRates = tax.ToString() + ":" + (dto.OrderSubtotal - orderSubtotalExclTax).ToString(),
        //        OrderTax = dto.OrderSubtotal - orderSubtotalExclTax,
        //        OrderTotal = dto.OrderTotal,
        //        RefundedAmount = decimal.Zero,
        //        OrderDiscount = decimal.Zero,
        //        CheckoutAttributeDescription = string.Empty,
        //        CheckoutAttributesXml = null,
        //        CustomerCurrencyCode = currencyId == null ? "MXN" : _currencyService.GetCurrencyById(int.Parse(currencyId)).CurrencyCode,
        //        CurrencyRate = currencyId == null ? 1 : _currencyService.GetCurrencyById(int.Parse(currencyId)).Rate,
        //        AffiliateId = 0,
        //        OrderStatus = OrderStatus.Pending,
        //        AllowStoringCreditCardNumber = false,
        //        CardType = string.Empty,
        //        CardName = string.Empty,
        //        CardNumber = string.Empty,
        //        MaskedCreditCardNumber = string.Empty,
        //        CardCvv2 = string.Empty,
        //        CardExpirationMonth = string.Empty,
        //        CardExpirationYear = string.Empty,
        //        PaymentMethodSystemName = "Paypal.Mobile",
        //        AuthorizationTransactionId = null,
        //        AuthorizationTransactionCode = null,
        //        AuthorizationTransactionResult = null,
        //        CaptureTransactionId = null,
        //        CaptureTransactionResult = null,
        //        SubscriptionTransactionId = null,
        //        PaymentStatus = PaymentStatus.Pending,
        //        PaidDateUtc = null,
        //        BillingAddress = _addressService.GetAddressById(dto.ShippingAddress.Id),
        //        ShippingAddress = _addressService.GetAddressById(dto.ShippingAddress.Id),
        //        ShippingStatus = ShippingStatus.NotYetShipped,
        //        ShippingMethod = "Estafeta",
        //        PickUpInStore = false,
        //        PickupAddress = null,
        //        ShippingRateComputationMethodSystemName = dto.ShippingRateComputationMethodSystemName,
        //        CustomValuesXml = null,
        //        VatNumber = null,
        //        CreatedOnUtc = DateTime.UtcNow,
        //        CustomOrderNumber = string.Empty,
        //        Customer = customer,
        //        BillingAddressId = dto.ShippingAddress.Id,
        //        PickupAddressId = null,
        //        ShippingStatusId = (int)ShippingStatus.NotYetShipped,
        //        ShippingAddressId = dto.ShippingAddress.Id,
        //        OrderStatusId = (int)OrderStatus.Pending
        //    };

        //    _orderService.InsertOrder(order);

        //    //generate and set custom order number
        //    order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
        //    _orderService.UpdateOrder(order);

        //    foreach (var item in dto.OrderProducts)
        //    {
        //        Product product = _productService.GetProductById(item.Id);
        //        var attributesXml = _productAttributeConverter.ConvertToXml(item.SelectedAttributes.Select(x => x.AttributeValueId).ToArray(), item.Id);

        //        var attributeDescription =
        //            _productAttributeFormatter.FormatAttributes(product, attributesXml, customer);

        //        var orderItem = new OrderItem
        //        {
        //            OrderItemGuid = Guid.NewGuid(),
        //            Order = order,
        //            ProductId = item.Id,
        //            UnitPriceInclTax = _taxService.GetProductPrice(product, item.Price, true, customer, out decimal _),
        //            UnitPriceExclTax = _taxService.GetProductPrice(product, item.Price, false, customer, out decimal _),
        //            PriceInclTax = _taxService.GetProductPrice(product, item.Price, true, customer, out decimal _),
        //            PriceExclTax = _taxService.GetProductPrice(product, item.Price, false, customer, out decimal _),
        //            OriginalProductCost = _priceCalculationService.GetProductCost(product, attributesXml),
        //            AttributeDescription = attributeDescription,
        //            AttributesXml = attributesXml,
        //            Quantity = item.Quantity,
        //            DiscountAmountInclTax = 0,
        //            DiscountAmountExclTax = 0,
        //            DownloadCount = 0,
        //            IsDownloadActivated = false,
        //            LicenseDownloadId = 0,
        //            ItemWeight = item.Weight,
        //            RentalStartDateUtc = null,
        //            RentalEndDateUtc = null
        //        };
        //        order.OrderItems.Add(orderItem);

        //        var previousStock = product.ManageInventoryMethod == ManageInventoryMethod.ManageStock ? product.StockQuantity :
        //           _productAttributeParser.FindProductAttributeCombination(product, attributesXml).StockQuantity;

        //        _productService.AdjustInventory(product, -item.Quantity, attributesXml,
        //            string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.PlaceOrder"),
        //                order.Id));

        //        _stockLogService.InsertStockLog(new StockLog()
        //        {
        //            CreatedOnUtc = DateTime.UtcNow,
        //            NewStock = previousStock - item.Quantity,
        //            OldStock = previousStock,
        //            Product = product.Name,
        //            ProductId = product.Id,
        //            SKU = product.Sku,
        //            User = _workContext.CurrentCustomer.GetFullName(),
        //            ChangeType = TypeChangeEnum.Buy
        //        });
        //    }

        //    _orderService.UpdateOrder(order);

        //    SendNotificationsAndSaveNotes(order);

        //    _customerActivityService.InsertActivity("PublicStore.PlaceOrder", _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"), order.Id);

        //    return Ok(order.Id);
        //}

        [HttpPut]
        public virtual IActionResult MarkOrderAsPaid(int orderId)
        {
            Order order = _orderService.GetOrderById(orderId);
            if (order == null) return NotFound();

            MarkOrderAsPaid(order);

            return NoContent();
        }

        #endregion

        #region private methods

        protected virtual void SendNotificationsAndSaveNotes(Order order)
        {
            //notes, messages
            AddOrderNote(order, "Order placed");

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
            {
                AddOrderNote(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifier: {orderPlacedStoreOwnerNotificationQueuedEmailId}.");
            }

            var orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService
                .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, null, null);
            if (orderPlacedCustomerNotificationQueuedEmailId > 0)
            {
                AddOrderNote(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifier: {orderPlacedCustomerNotificationQueuedEmailId}.");
            }
        }

        protected virtual void AddOrderNote(Order order, string note)
        {
            order.OrderNotes.Add(new OrderNote
            {
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _orderService.UpdateOrder(order);
        }

        protected virtual void ProcessOrderPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //raise event
            _eventPublisher.Publish(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    null, null);

                _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            }
        }

        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            _orderService.UpdateOrder(order);

            int customerId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(customerId);
            //add a note
            AddOrderNote(order, $"La orden fue marcada como pagada por {customer?.GetFullName()} ({customerId})");

            CheckOrderStatus(order);

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                ProcessOrderPaid(order);
            }
        }

        public virtual void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatus == PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            switch (order.OrderStatus)
            {
                case OrderStatus.Pending:
                    if (order.PaymentStatus == PaymentStatus.Authorized ||
                        order.PaymentStatus == PaymentStatus.Paid)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }

                    if (order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                        order.ShippingStatus == ShippingStatus.Shipped ||
                        order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }
                    break;
                //is order complete?
                case OrderStatus.Cancelled:
                case OrderStatus.Complete:
                    return;
            }

            if (order.PaymentStatus != PaymentStatus.Paid)
                return;

            bool completed;

            if (order.ShippingStatus == ShippingStatus.ShippingNotRequired)
            {
                //shipping is not required
                completed = true;
            }
            else
            {
                //shipping is required
                if (_orderSettings.CompleteOrderWhenDelivered)
                {
                    completed = order.ShippingStatus == ShippingStatus.Delivered;
                }
                else
                {
                    completed = order.ShippingStatus == ShippingStatus.Shipped ||
                                order.ShippingStatus == ShippingStatus.Delivered;
                }
            }

            if (completed)
            {
                SetOrderStatus(order, OrderStatus.Complete, true);
            }
        }

        protected virtual void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
                return;

            //set and save new order status
            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            //order notes, notifications
            AddOrderNote(order, $"Order status has been changed to {os.ToString()}");

            if (prevOrderStatus != OrderStatus.Complete &&
                os == OrderStatus.Complete
                && notifyCustomer)
            {
                //notification
                var orderCompletedCustomerNotificationQueuedEmailId = _workflowMessageService
                    .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, null,
                    null);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    AddOrderNote(order, $"\"Order completed\" email (to customer) has been queued. Queued email identifier: {orderCompletedCustomerNotificationQueuedEmailId}.");
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                //notification
                var orderCancelledCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    AddOrderNote(order, $"\"Order cancelled\" email (to customer) has been queued. Queued email identifier: {orderCancelledCustomerNotificationQueuedEmailId}.");

                }
            }
        }

        #endregion
    }
}
