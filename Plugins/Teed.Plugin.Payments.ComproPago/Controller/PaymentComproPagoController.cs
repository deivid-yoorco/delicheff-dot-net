using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Teed.Plugin.Payments.ComproPago.Models;
using CompropagoSdk;
using CompropagoSdk.Factory;
using System.Net.Http;
using System.Threading.Tasks;
using CompropagoSdk.Factory.Models;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using System;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Messages;
using Nop.Services.Events;
using Nop.Core.Domain.Localization;
using Nop.Services.Catalog;
using Newtonsoft.Json.Linq;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Payments;

namespace Teed.Plugin.Payments.ComproPago.Controller
{
    public class PaymentComproPagoController : BasePaymentController
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRewardPointService _rewardPointService;
        private readonly IProductService _productService;
        private readonly IStockLogService _stockLogService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ComproPagoPaymentSettings _comproPagoPaymentSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        private readonly string[] ProvidersToExclude = new[] { "BTC", "ETH", "BCH", "LTC", "XRP" };

        #endregion

        #region Ctor

        public PaymentComproPagoController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            IEventPublisher eventPublisher,
            IRewardPointService rewardPointService,
            IProductService productService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            LocalizationSettings localizationSettings,
            ComproPagoPaymentSettings comproPagoPaymentSettings,
            IStockLogService stockLogService,
            IProductAttributeParser productAttributeParser)
        {
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _eventPublisher = eventPublisher;
            _localizationSettings = localizationSettings;
            _rewardPointService = rewardPointService;
            _productService = productService;
            _comproPagoPaymentSettings = comproPagoPaymentSettings;
            _stockLogService = stockLogService;
            _productAttributeParser = productAttributeParser;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var comproPagoPaymentSettings = _settingService.LoadSetting<ComproPagoPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = comproPagoPaymentSettings.UseSandbox,
                AdditionalFee = comproPagoPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = comproPagoPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                PrivateKey = comproPagoPaymentSettings.PrivateKey,
                PublicKey = comproPagoPaymentSettings.PublicKey
            };
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(comproPagoPaymentSettings, x => x.UseSandbox, storeScope);
                model.PrivateKey_OverrideForStore = _settingService.SettingExists(comproPagoPaymentSettings, x => x.PrivateKey, storeScope);
                model.PublicKey_OverrideForStore = _settingService.SettingExists(comproPagoPaymentSettings, x => x.PublicKey, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(comproPagoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(comproPagoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.ComproPago/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var comproPagoPaymentSettings = _settingService.LoadSetting<ComproPagoPaymentSettings>(storeScope);

            //save settings
            comproPagoPaymentSettings.UseSandbox = model.UseSandbox;
            comproPagoPaymentSettings.PrivateKey = model.PrivateKey;
            comproPagoPaymentSettings.PublicKey = model.PublicKey;
            comproPagoPaymentSettings.AdditionalFee = model.AdditionalFee;
            comproPagoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(comproPagoPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(comproPagoPaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(comproPagoPaymentSettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(comproPagoPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(comproPagoPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        public IActionResult WebHook([FromBody] object json)
        {
            JObject response = new JObject();

            CpOrderInfo info = Factory.CpOrderInfo(json.ToString());
            if (info.id == null || info.id == "")
            {
                response["status"] = "error";
                response["message"] = "invalid request";
                response["short_id"] = null;
                response["reference"] = null;
                return Content(response.ToString(), "application/json");
            }

            try
            {
                Client client = new Client(
                    _comproPagoPaymentSettings.PublicKey,
                    _comproPagoPaymentSettings.PrivateKey,
                    _comproPagoPaymentSettings.UseSandbox
                );

                if (info.id == "ch_0000000_0000_0000_000000")
                {
                    response["status"] = "success";
                    response["message"] = "Test success";
                    response["short_id"] = null;
                    response["reference"] = null;
                    return Content(response.ToString(), "application/json");
                }

                CpOrderInfo verified = client.Api.VerifyOrder(info.id);
                Order order = _orderService.GetOrderById(int.Parse(verified.order_info.order_id));
                if (order == null)
                {
                    response["status"] = "error";
                    response["message"] = $"Order not found. OrderId: {verified.order_info.order_id}";
                    response["short_id"] = null;
                    response["reference"] = null;
                    return Content(response.ToString(), "application/json");
                }

                switch (verified.type)
                {
                    case "charge.success":
                        MarkOrderAsPaid(order);
                        break;
                    case "charge.expired":
                        MarkOrderAsCancelled(order);
                        break;
                    case "charge.pending":
                        // Do Nothing;
                        break;
                    default:
                        response["status"] = "error";
                        response["message"] = "invalid request type";
                        response["short_id"] = null;
                        response["reference"] = null;
                        return Content(response.ToString(), "application/json");
                }

                response["status"] = "success";
                response["message"] = "OK";
                response["short_id"] = null;
                response["reference"] = $"order-{verified.order_info.order_id}";
                return Content(response.ToString(), "application/json");
            }
            catch (Exception e)
            {
                response["status"] = "error";
                response["message"] = e.Message;
                response["short_id"] = null;
                response["reference"] = $"error-{Guid.NewGuid()}";
                return Content(response.ToString(), "application/json");
            }
        }

        [HttpGet]
        public IActionResult GetAllProviders(double orderTotal = 0.0)
        {
            Provider[] providers = new Provider[] { };
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName("Payments.ComproPago");

            if (paymentMethod != null && paymentMethod.IsPaymentMethodActive(_paymentSettings))
            {
                try
                {
                    Client client = new Client(
                        _comproPagoPaymentSettings.PrivateKey,
                        _comproPagoPaymentSettings.PublicKey,
                        _comproPagoPaymentSettings.UseSandbox
                    );

                    if (orderTotal == 0.0)
                    {
                        providers = client.Api.ListProviders();
                    }
                    else
                    {
                        providers = client.Api.ListProviders().Where(x => x.transaction_limit > orderTotal).ToArray();
                    }
                }
                catch (Exception) { }
            }

            return Ok(providers.Where(x => !ProvidersToExclude.Contains(x.internal_name)));
        }

        #endregion

        #region Private Methos

        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderProcessingService.MarkOrderAsPaid(order);

            //add a note
            AddOrderNote(order, "Pago registrado por ComproPago");
        }

        protected virtual void MarkOrderAsCancelled(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            SetOrderStatus(order, OrderStatus.Cancelled, true);
            AddOrderNote(order, "Order has been cancelled (ComproPago Expired)");

            ReturnBackRedeemedRewardPoints(order);

            //Adjust inventory
            foreach (var orderItem in order.OrderItems)
            {
                var previousStock = orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock ? orderItem.Product.StockQuantity :
                   _productAttributeParser.FindProductAttributeCombination(orderItem.Product, orderItem.AttributesXml).StockQuantity;

                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.CancelOrder"), order.Id));

                _stockLogService.InsertStockLog(new StockLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    NewStock = previousStock + orderItem.Quantity,
                    OldStock = previousStock,
                    Product = orderItem.Product.Name,
                    ProductId = orderItem.Product.Id,
                    SKU = orderItem.Product.Sku,
                    User = _workContext.CurrentCustomer.GetFullName(),
                    ChangeType = TypeChangeEnum.Cancel
                });
                
            }

            _eventPublisher.Publish(new OrderCancelledEvent(order));
        }

        protected virtual void ReturnBackRedeemedRewardPoints(Order order)
        {
            //were some points redeemed when placing an order?
            if (order.RedeemedRewardPointsEntry == null)
                return;

            //return back
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, -order.RedeemedRewardPointsEntry.Points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForOrder"), order.CustomOrderNumber));
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